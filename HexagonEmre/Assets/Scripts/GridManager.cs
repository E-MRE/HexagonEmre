using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class GridManager : BaseMonoBehaviour
{
    //Variables
    public static GridManager instance = null;
    public GameObject hexagonPrefab;
    public GameObject hexagonParent;
    public GameObject outlineParent;
    public GameObject panelGameOver;

    public HexagonManager hexagonManager;

    public Sprite hexagonSprite;
    public Sprite outlineSprite;

    public List<Color> colorList;
    private List<Hexagon> bombList;

    public int gridWidth = 8;
    public int gridHeight = 9;
    private int selectionStatus;

    private float hexagonWidth, hexagonHeight;

    private bool selectedHexes, isGameOver;
    private bool isHexagonRotation = false;
    private bool isHexagonExplosion = false;
    private bool isGameReady = false;

    private Vector2 selectedPosition;
    private List<Hexagon> selectedHexGroup;
    private List<Hexagon> selectedGroup;
    private Hexagon[,] gameGrid;
    private Hexagon selectedHexagon;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        selectedGroup = new List<Hexagon>();
        bombList = new List<Hexagon>();
        gameGrid = new Hexagon[gridWidth, gridHeight];

        FindGameObjectScale(hexagonPrefab, out hexagonWidth, out hexagonHeight);
        InitializeGrid();
    }

    //Initialize grid
    private void InitializeGrid()
    {
        CreateGrid();
        hexagonParent.transform.position = CalculateGridPosition();

        isGameOver = false;
        isGameReady = true;
    }

    //Creates grid (Width x Height)
    private void CreateGrid()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                GameObject hexagon = Instantiate(hexagonPrefab, hexagonParent.transform) as GameObject;
                hexagonManager.ConfigureHexagon(hexagon, x, y);
                gameGrid[x, y] = hexagon.GetComponent<Hexagon>();
            }
        }
    }

    //Checks availabiles of the variables 
    public bool InputAvailabile()
    {
        return !isGameOver && !isHexagonRotation && !isHexagonExplosion && isGameReady;
    }

    //This function selects hexagon with collider
    public void SelectHexagons(Collider2D collider)
    {
        if (selectedHexagon == null || !selectedHexagon.GetComponent<Collider2D>().Equals(collider))
        {
            selectedHexagon = collider.gameObject.GetComponent<Hexagon>();
            selectedPosition.x = selectedHexagon.coordinateX;
            selectedPosition.y = selectedHexagon.coordinateY;
            selectionStatus = 0;
        }
        else
        {
            selectionStatus = (++selectionStatus) % SELECTION_COMBINATIONS;
        }

        DestroyOutline();
        BuildOutline();
    }

    //Destroy all of outlines
    private void DestroyOutline()
    {
        if (outlineParent.transform.childCount > 0)
        {
            foreach (Transform child in outlineParent.transform)
                Destroy(child.gameObject);
        }
    }

    //When users select hexagon this function builds outlines 
    private void BuildOutline()
    {
        //Gets selected group
        hexagonManager.FindHexagonGroup(selectedPosition, ref selectedHexagon, ref selectedGroup);
        hexagonManager.FindParentPosition();

        //Creates outline group
        foreach (Hexagon outlinedHexagon in selectedGroup)
        {
            GameObject outline = new GameObject("Outline");
            GameObject selectedHex = new GameObject("SelectedHex");

            //Adds components and configures
            outline.AddComponent<SpriteRenderer>();
            outline.GetComponent<SpriteRenderer>().sprite = outlineSprite;
            outline.GetComponent<SpriteRenderer>().color = Color.white;
            outline.transform.position = new Vector3(outlinedHexagon.transform.position.x, outlinedHexagon.transform.position.y, -1);
            outline.transform.localScale = OUTLINE_SCALE;

            selectedHex.AddComponent<SpriteRenderer>();
            selectedHex.GetComponent<SpriteRenderer>().sprite = hexagonSprite;
            selectedHex.GetComponent<SpriteRenderer>().color = outlinedHexagon.GetComponent<SpriteRenderer>().color;
            selectedHex.transform.position = new Vector3(outlinedHexagon.transform.position.x, outlinedHexagon.transform.position.y, -2);
            selectedHex.transform.localScale = outlinedHexagon.transform.localScale;

            outline.transform.parent = selectedHex.transform;
            selectedHex.transform.parent = outlineParent.transform;

            //if this hexagon is bomb added text mesh from parent
            if(bombList != null && bombList.Count>0)
                if (bombList.Contains(outlinedHexagon))
                {
                    var timer = Instantiate(outlinedHexagon.gameObject, selectedHex.transform) as GameObject;
                    timer.transform.parent = selectedHex.transform;
                    timer.transform.localPosition = new Vector3(0, 0, 0);
                }
        }
    }

    //This function sets rotation
    public void RotateHexes(bool clockWise)
    {
        isHexagonRotation = true;
        ROTATE_VALUE = clockWise ? -5 : 5;

        hexagonManager.HideOrShowHexagons(selectedGroup, false);
        StartCoroutine(RotationCoroutine(clockWise));
    }

    //It rotates outline objects
    private IEnumerator RotationCoroutine(bool clockWise)
    {
        outlineParent.transform.Rotate(0f, 0f, ROTATE_VALUE);
        yield return new WaitForSeconds(0.01f);

        //check degrees
        int degree = Math.Abs((int)outlineParent.transform.eulerAngles.z);
        if (degree != 0)
        {
            List<Hexagon> explodedHexagons = null;
            //it checks hexagons degree is 120 or 240
            if ((degree > 115 && degree <= 120) || (degree > 235 && degree <= 240))
            {
                //swipe objects and check explosion
                hexagonManager.SwipeHexagons(selectedGroup, clockWise);
                explodedHexagons = SearchExplodedHexagons();
            }

            //if there isn't hexagon continues to rotation
            if (explodedHexagons == null || explodedHexagons.Count == 0)
            {
                StartCoroutine(RotationCoroutine(clockWise));
            }
            else
            {
                isHexagonExplosion = true;
                isHexagonRotation = false;
                hexagonManager.HideOrShowHexagons(selectedGroup, true);
                DestroyOutline();

                //adds score and moves
                ScoreManager.instance.AddScore(explodedHexagons.Count);
                ScoreManager.instance.AddMove();

                while (true)
                {
                    var coordinates = hexagonManager.DestroyHexagons(explodedHexagons);
                    int countX;

                    for (int x = 0; x < coordinates.Count; x++)
                    {
                        countX = explodedHexagons.Where(c => c.coordinateX == coordinates[x]).ToList().Count;
                        hexagonManager.MoveHexagons(explodedHexagons, coordinates[x], countX);
                        hexagonManager.InstantiateHexagon(coordinates[x], countX);
                    }

                    yield return new WaitForSeconds(1.25f);

                    //again checks explosion objects
                    explodedHexagons.Clear();
                    explodedHexagons = SearchExplodedHexagons();
                    if (explodedHexagons == null || explodedHexagons.Count == 0)
                        break;
                    else
                        ScoreManager.instance.AddScore(explodedHexagons.Count);
                }

                outlineParent.transform.rotation = new Quaternion(0, 0, 0, 0);
                isHexagonExplosion = false;
                BuildOutline();
            }
        }
        else
        {
            hexagonManager.SwipeHexagons(selectedGroup, clockWise);
            isHexagonRotation = false;
            hexagonManager.HideOrShowHexagons(selectedGroup, true);
        }
    }

    //private List<Hexagon> FindExplodedHexagons()
    //{
    //    List<Hexagon> explodedHexes = new List<Hexagon>();
    //    Vector2 neighbourFirst, neighbourSecond;
    //    var neighbourManager = NeighbourManager.instance;

    //    for (int i = 0; i < selectedGroup.Count; i++)
    //    {
    //        var neighbours = selectedGroup[i].GetNeighbours();
    //        for (int status = 0; status < SELECTION_COMBINATIONS; status++)
    //        {
    //            var checkAvailable = neighbourManager.CheckNeighbourAvailable(neighbours, status, out neighbourFirst, out neighbourSecond);
    //            if (checkAvailable)
    //            {
    //                var hexagonFirst = gameGrid[(int)neighbourFirst.x, (int)neighbourFirst.y];
    //                var hexagonSecond = gameGrid[(int)neighbourSecond.x, (int)neighbourSecond.y];

    //                if (selectedGroup[i].color == hexagonFirst.color && selectedGroup[i].color == hexagonSecond.color)
    //                {
    //                    if (!explodedHexes.Contains(selectedGroup[i]))
    //                        explodedHexes.Add(selectedGroup[i]);

    //                    if (!explodedHexes.Contains(hexagonFirst))
    //                        explodedHexes.Add(hexagonFirst);

    //                    if (!explodedHexes.Contains(hexagonSecond))
    //                        explodedHexes.Add(hexagonSecond);
    //                }
    //            }
    //        }
    //    }

    //    return explodedHexes;
    //}


    //This functions checks all of hexagons inside of the grid. And returns explosion hexagons
    private List<Hexagon> SearchExplodedHexagons()
    {
        List<Hexagon> explodedHexes = new List<Hexagon>();
        List<Hexagon> neighbourList = new List<Hexagon>();

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                var neighbours = gameGrid[x, y].GetNeighbours();

                NeighbourManager.instance.AddAvailableNeighbours(ref neighbourList, neighbours, x, y);
                NeighbourManager.instance.CheckNeighbourColors(ref explodedHexes, neighbourList, x, y);
                neighbourList.Clear();
            }
        }

        return explodedHexes;
    }

    //it is finished the game
    public void GameOver()
    {
        isGameOver = true;
        isGameReady = false;
        panelGameOver.SetActive(true);
    }

    #region Getters & Setters
    public Hexagon GetSelectedHexagon()
    {
        return selectedHexagon;
    }

    public Hexagon[,] GetGameGrid()
    {
        return gameGrid;
    }

    public void SetGameGrid(Hexagon[,] value)
    {
        gameGrid = value;
    }

    public void SetGameGridSpecial(Hexagon value, int coordinateX, int coordinateY)
    {
        gameGrid[coordinateX, coordinateY] = value;
    }

    public void SetExplodedStatus(bool value)
    {
        isHexagonExplosion = value;
    }

    public bool GetGameStatus()
    {
        return isGameReady && !isGameOver;
    }

    public void SetGameOver(bool value)
    {
        isGameOver = value;
    }

    public List<Hexagon> GetBombList()
    {
        return bombList;
    }

    public void AddBombList(Hexagon value)
    {
        bombList.Add(value);
    }

    public int GetSelectionStatus()
    {
        return selectionStatus;
    }

    public void SetSelectionStatus(int value)
    {
        selectionStatus = value;
    }
    #endregion
}
