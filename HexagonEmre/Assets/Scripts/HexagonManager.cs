using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class HexagonManager : BaseMonoBehaviour
{
    private GridManager gridManager;

    private void Start()
    {
        gridManager = GridManager.instance;
    }

    // This function is configured hexagons
    public void ConfigureHexagon(GameObject hexagon, int x, int y)
    {
        hexagon.transform.position = CalculateWorldPosition(new Vector2(x, y));
        hexagon.name = NAME_HEXAGON + " - " + x + y;
        hexagon.GetComponent<Hexagon>().coordinateX = x;
        hexagon.GetComponent<Hexagon>().coordinateY = y;

        var color = SetHexagonColor(hexagon, gridManager.colorList);
        hexagon.GetComponent<Hexagon>().color = color;
        hexagon.GetComponent<SpriteRenderer>().color = color;
    }

    //With this function set colors
    public Color SetHexagonColor(GameObject hexagon, List<Color> colorList)
    {
        var neighbourManager = NeighbourManager.instance;
        var neighbourColorList = neighbourManager.GetNeighbourColors(hexagon.GetComponent<Hexagon>(), gridManager.GetGameGrid());

        int sayi;
        while (true)
        {
            sayi = Random.Range(0, 4096);
            sayi %= colorList.Count;

            int index = neighbourColorList.IndexOf(colorList[sayi]);
            if (index == -1)
                break;
        }

        return colorList[sayi];
    }

    //Using selected hexagon finds group
    public void FindHexagonGroup(Vector2 selectedPosition, ref Hexagon selectedHexagon, ref List<Hexagon> selectedGroup)
    {
        Vector2 firstHexPosition, secondHexPosition;
        selectedHexagon = gridManager.GetGameGrid()[(int)selectedPosition.x, (int)selectedPosition.y];
        FindOtherHexagons(out firstHexPosition, out secondHexPosition);
        selectedGroup.Clear();
        selectedGroup.Add(selectedHexagon);
        selectedGroup.Add(gridManager.GetGameGrid()[(int)firstHexPosition.x, (int)firstHexPosition.y].GetComponent<Hexagon>());
        selectedGroup.Add(gridManager.GetGameGrid()[(int)secondHexPosition.x, (int)secondHexPosition.y].GetComponent<Hexagon>());
    }

    //It gets available hexagons using coordinates
    //This vectors for the selected hexagons' coordinates
    public void FindOtherHexagons(out Vector2 firstHex, out Vector2 secondHex)
    {
        var selectedHexagon = gridManager.GetSelectedHexagon();
        NeighbourHexagon neighbours = selectedHexagon.GetNeighbours();
        int selectionStatus = gridManager.GetSelectionStatus();

        bool breakLoop = false;
        firstHex = Vector2.zero;
        secondHex = Vector2.zero;

        while (!breakLoop)
        {
            switch (selectionStatus)
            {
                case 0: firstHex = neighbours.upNeighbour; secondHex = neighbours.upRightNeighbour; break;
                case 1: firstHex = neighbours.upRightNeighbour; secondHex = neighbours.downRightNeighbour; break;
                case 2: firstHex = neighbours.downRightNeighbour; secondHex = neighbours.downNeighbour; break;
                case 3: firstHex = neighbours.downNeighbour; secondHex = neighbours.downLeftNeighbour; break;
                case 4: firstHex = neighbours.downLeftNeighbour; secondHex = neighbours.upLeftNeighbour; break;
                case 5: firstHex = neighbours.upLeftNeighbour; secondHex = neighbours.upNeighbour; break;
                default: firstHex = Vector2.zero; secondHex = Vector2.zero; break;
            }


            bool firstSituation = IsValidNeighbour(firstHex);
            bool secondSituation = IsValidNeighbour(secondHex);

            if (!firstSituation || !secondSituation)
            {
                selectionStatus = (++selectionStatus) % SELECTION_COMBINATIONS;
                gridManager.SetSelectionStatus(selectionStatus);
            }
            else
            {
                breakLoop = true;
            }
        }
    }

    // It set position of the outline for the rotate hexagons
    public void FindParentPosition()
    {
        float positionX;
        float positionY;

        var selectedHexagon = gridManager.GetSelectedHexagon();

        //This position defines using selection status
        switch (gridManager.GetSelectionStatus())
        {
            case 0:
                positionX = selectedHexagon.transform.Find("Corners").Find("RightUp").transform.position.x;
                positionY = selectedHexagon.transform.Find("Corners").Find("RightUp").transform.position.y;

                gridManager.outlineParent.transform.position = new Vector3(positionX, positionY, 0);
                break;
            case 1:
                positionX = selectedHexagon.transform.Find("Corners").Find("Right").transform.position.x;
                positionY = selectedHexagon.transform.Find("Corners").Find("Right").transform.position.y;

                gridManager.outlineParent.transform.position = new Vector3(positionX, positionY, 0);
                break;
            case 2:
                positionX = selectedHexagon.transform.Find("Corners").Find("RightDown").transform.position.x;
                positionY = selectedHexagon.transform.Find("Corners").Find("RightDown").transform.position.y;

                gridManager.outlineParent.transform.position = new Vector3(positionX, positionY, 0);
                break;
            case 3:
                positionX = selectedHexagon.transform.Find("Corners").Find("LeftDown").transform.position.x;
                positionY = selectedHexagon.transform.Find("Corners").Find("LeftDown").transform.position.y;

                gridManager.outlineParent.transform.position = new Vector3(positionX, positionY, 0);
                break;
            case 4:
                positionX = selectedHexagon.transform.Find("Corners").Find("Left").transform.position.x;
                positionY = selectedHexagon.transform.Find("Corners").Find("Left").transform.position.y;

                gridManager.outlineParent.transform.position = new Vector3(positionX, positionY, 0);
                break;
            case 5:
                positionX = selectedHexagon.transform.Find("Corners").Find("LeftUp").transform.position.x;
                positionY = selectedHexagon.transform.Find("Corners").Find("LeftUp").transform.position.y;

                gridManager.outlineParent.transform.position = new Vector3(positionX, positionY, 0);
                break;
            default:
                gridManager.outlineParent.transform.position = new Vector3(0, 0, 0);
                break;
        }
    }

    //Using coordinates, new hexagons instantiated in this function.
    //It needs x coordinate and count of instantiate
    public void InstantiateHexagon(int coordinateX, int count)
    {
        for (int y = (count * -1); y < 0; y++)
        {
            //Instantiate hexagon and defines is that a bomb or not
            GameObject newHexagon = Instantiate(gridManager.hexagonPrefab, gridManager.hexagonParent.transform) as GameObject;
            if (y == -1 && BOMB_READY)
            {
                newHexagon.GetComponent<Hexagon>().IsBomb(true);
                gridManager.AddBombList(newHexagon.GetComponent<Hexagon>());
                BOMB_READY = false;
            }
            else
            {
                newHexagon.GetComponent<Hexagon>().IsBomb(false);
            }

            //Hexagon configuration
            ConfigureHexagon(newHexagon, coordinateX, y);

            //This position is added for the correct position. Because hexagonParent position is not (0,0,0)
            newHexagon.transform.position += gridManager.hexagonParent.transform.position;

            //For the lerp, reborn function is called. Count for the lerp times
            newHexagon.GetComponent<Hexagon>().Reborn(count);
        }
    }

    //This function checks null hexagon in the grid
    public bool CheckNullHexegons()
    {
        string hexName = NAME_HEXAGON + " - ";
        var gameGrid = gridManager.GetGameGrid();
        List<Hexagon> lostCells = new List<Hexagon>();
        List<string> nullHexes = new List<string>();

        //All hexagon objects get using tag
        GameObject[] hexagons = GameObject.FindGameObjectsWithTag(TAG_HEXAGON);

        for (int x = 0; x < GRID_WIDTH; x++)
        {
            for (int y = 0; y < GRID_HEIGHT; y++)
            {
                //checking have gameobjects same name
                var list = hexagons.Where(c => c.name == (hexName + x + y)).ToList();
                if (list.Count > 1)
                    for (int i = 0; i < list.Count - 1; i++)
                        lostCells.Add(list[0].GetComponent<Hexagon>());

                //null hexagons' coordinates are added list
                if (gameGrid[x, y] == null)
                {
                    string coordinates = x + "," + y;
                    nullHexes.Add(coordinates);
                }
            }
        }

        //Both lists have childs we calling function for the fill null positions
        if (lostCells != null && lostCells.Count > 0 && nullHexes != null && nullHexes.Count > 0)
            FillHexagons(lostCells, nullHexes);

        return true;
    }

    //This function fills null positions of the grid
    private void FillHexagons(List<Hexagon> lostCells, List<string> nullHexes)
    {
        for (int i = 0; i < lostCells.Count; i++)
        {
            string[] splitCoordinates = nullHexes[i].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            int x = int.Parse(splitCoordinates[0]);
            int y = int.Parse(splitCoordinates[1]);

            var position = CalculateWorldPosition(new Vector2(x, y));
            position += gridManager.hexagonParent.transform.position;

            lostCells[i].GetComponent<Hexagon>().ChangePosition(position, x, y);
            gridManager.SetGameGridSpecial(lostCells[i], x, y);
        }
    }

    //In this function, hexagons destroy with list
    //It returns x coordinates for the instantiate
    public List<int> DestroyHexagons(List<Hexagon> explodedHexagons)
    {
        List<int> differentCoordinateX = new List<int>();
        var bombList = gridManager.GetBombList();

        for (int i = 0; i < explodedHexagons.Count; i++)
        {
            //Hexagons is controlled for the bomb
            var hexagon = gridManager.GetGameGrid()[explodedHexagons[i].coordinateX, explodedHexagons[i].coordinateY];
            var index = bombList.IndexOf(hexagon);

            //if it's a bomb, it is removed from its list
            if (index != -1)
                bombList.RemoveAt(index);

            //Hexagon is destroyed and its position doing null
            Destroy(hexagon.gameObject);
            gridManager.SetGameGridSpecial(null, explodedHexagons[i].coordinateX, explodedHexagons[i].coordinateY);

            //for the different coordinates x adding the list
            if (!differentCoordinateX.Contains(explodedHexagons[i].coordinateX))
                differentCoordinateX.Add(explodedHexagons[i].coordinateX);
        }

        return differentCoordinateX;
    }

    //This function swipes hexagons in the selected group with each other
    public void SwipeHexagons(List<Hexagon> selectedGroup, bool clockWise)
    {
        //Getting hexagons fron list
        var hexagonFirst = selectedGroup[0];
        var hexagonSecond = selectedGroup[1];
        var hexagonThird = selectedGroup[2];

        //Getting positions from hexagons
        var firstPosition = hexagonFirst.transform.position;
        var secondPosition = hexagonSecond.transform.position;
        var thirdPosition = hexagonThird.transform.position;

        //Getting coordinates from positions
        int coordinateFirstX = hexagonFirst.coordinateX;
        int coordinateSecondX = hexagonSecond.coordinateX;
        int coordinateThirdX = hexagonThird.coordinateX;

        int coordinateFirstY = hexagonFirst.coordinateY;
        int coordinateSecondY = hexagonSecond.coordinateY;
        int coordinateThirdY = hexagonThird.coordinateY;

        //checking rotation direction
        if (clockWise)
        {
            //in that area, all hexagongs swiping each other
            hexagonFirst.ChangePosition(secondPosition, coordinateSecondX, coordinateSecondY);
            gridManager.SetGameGridSpecial(hexagonFirst, coordinateSecondX, coordinateSecondY);

            hexagonSecond.ChangePosition(thirdPosition, coordinateThirdX, coordinateThirdY);
            gridManager.SetGameGridSpecial(hexagonSecond, coordinateThirdX, coordinateThirdY);

            hexagonThird.ChangePosition(firstPosition, coordinateFirstX, coordinateFirstY);
            gridManager.SetGameGridSpecial(hexagonThird, coordinateFirstX, coordinateFirstY);
        }
        else
        {
            //in that area, all hexagongs swiping each other
            hexagonFirst.ChangePosition(thirdPosition, coordinateThirdX, coordinateThirdY);
            gridManager.SetGameGridSpecial(hexagonFirst, coordinateThirdX, coordinateThirdY);

            hexagonSecond.ChangePosition(firstPosition, coordinateFirstX, coordinateFirstY);
            gridManager.SetGameGridSpecial(hexagonSecond, coordinateFirstX, coordinateFirstY);

            hexagonThird.ChangePosition(secondPosition, coordinateSecondX, coordinateSecondY);
            gridManager.SetGameGridSpecial(hexagonThird, coordinateSecondX, coordinateSecondY);
        }
    }

    //This function moves hexagons using coordinates
    public bool MoveHexagons(List<Hexagon> explodedHexagons, int coordinateX, int lerpCount)
    {
        //Getting grid
        var gameGrid = gridManager.GetGameGrid();
        List<int> emptyCoordinateY = new List<int>();

        //on the x coordinates hexagons check down neighbour is null or not
        for (int y = GRID_HEIGHT - 2; y >= 0; y--)
        {

            if (gameGrid[coordinateX, y] != null)
            {
                int count = 0;
                for (int a = y; a < GRID_HEIGHT; a++)
                {
                    if (gameGrid[coordinateX, a] == null)
                        count++;
                }

                gameGrid[coordinateX, y].ChangePositionWithLerp(count);
            }
        }

        return true;
    }

    //This function sets visibility of selected groups
    public void HideOrShowHexagons(List<Hexagon> selectedGroup, bool isActive)
    {
        foreach (var selected in selectedGroup)
        {
            gridManager.GetGameGrid()[selected.coordinateX, selected.coordinateY].gameObject.SetActive(isActive);
        }
    }
}
