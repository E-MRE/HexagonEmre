using UnityEngine;

public class Hexagon : MonoBehaviour
{
    // Variables
    private GridManager gridManager;
    public Color color;
    public Vector2 lerpPosition;
    public int coordinateX, coordinateY;
    public bool isLerp = false;
    private bool instantiateLerp = false;
    private bool isBomb;
    private int lerpCount;
    private int timer;
    private TextMesh textMesh;

    // Start is called before the first frame update
    void Start()
    {
        gridManager = GridManager.instance;
    }

    // Update is called once per frame
    void Update()
    {
        // hexagons change their position using isLerp.
        if (isLerp)
        {
            transform.position = Vector3.Lerp(transform.position, lerpPosition, Time.deltaTime * Constants.LERP_VALUE);

            if (Vector3.Distance(transform.position, lerpPosition) <= Constants.LERP_DISTANCE)
            {
                isLerp = false;
                AddHexInfo();
            }
        }

        // after explosion new instated hexagons go down using this block
        if (instantiateLerp)
        {
            transform.position = Vector3.Lerp(transform.position, lerpPosition, Time.deltaTime * Constants.LERP_VALUE);

            if (Vector3.Distance(transform.position, lerpPosition) <= Constants.LERP_DISTANCE)
            {
                instantiateLerp = false;
                AddHexInfo();
            }
        }
    }

    // End of the lerp this function adds informations of hexagon
    private void AddHexInfo()
    {
        transform.position = lerpPosition;
        coordinateY += lerpCount;
        lerpCount = 0;

        //if hexagon inside of the grid 
        if (coordinateY >= 0 && coordinateY < gridManager.gridHeight)
        {
            //the new position is saved and the name is changed
            gridManager.SetGameGridSpecial(this, coordinateX, coordinateY);
            gameObject.name = Constants.NAME_HEXAGON + " - " + coordinateX + coordinateY;
        }
    }

    // This function using lerp changes hexagon's position.
    // lerpCount is important for the available position.
    public void ChangePositionWithLerp(int lerpCount)
    {
        this.lerpCount = lerpCount;
        lerpPosition = new Vector2(transform.position.x, transform.position.y - Constants.HEX_VERTICAL_DISTANCE * lerpCount);
        isLerp = true;
    }

    //This function changes hexagon position without lerp. Also new coordinate informations is saved.
    public void ChangePosition(Vector2 position, int newCoordinateX, int newCoordinateY)
    {
        coordinateX = newCoordinateX;
        coordinateY = newCoordinateY;

        transform.position = position;
        gameObject.name = Constants.NAME_HEXAGON + " - " + coordinateX + coordinateY;
    }

    //Instantiated hexagons goes down to right position.
    public void Reborn(int lerpCount)
    {
        this.lerpCount = lerpCount;
        lerpPosition = new Vector2(transform.position.x, transform.position.y - Constants.HEX_VERTICAL_DISTANCE * lerpCount);

        //Checks this hexagon is bomb.
        if (isBomb)
            SetBomb();

        instantiateLerp = true;
    }

    //If hexagon is bomb this function sets timer
    private void SetBomb()
    {
        transform.GetChild(0).gameObject.SetActive(true);
        textMesh = transform.GetChild(0).Find("Timer").GetComponent<TextMesh>();
        SetTimer(Constants.BOMB_TIME);
    }

    #region Getters & Setters
    public NeighbourHexagon GetNeighbours() => NeighbourManager.instance.FindNeighbours(coordinateX, coordinateY);

    public void IsBomb(bool value) => isBomb = value;

    public int GetTimer()
    {
        return timer;
    }

    public void SetTimer(int value)
    {
        timer = value;
        textMesh.text = timer.ToString();

        if (timer == 0)
            gridManager.GameOver();
    }
    #endregion
}
