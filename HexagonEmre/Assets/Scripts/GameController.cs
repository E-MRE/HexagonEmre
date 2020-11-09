using System;
using UnityEngine;
using UnityEngine.UI;

public class GameController : BaseMonoBehaviour
{
    //Variables
    public bool isMobile = true, isEditor = true;
    private bool validTouch;
    private bool isSelection = false;

    private GridManager gridManager;
    private Vector2 touchStartPosition, touchEndPosition, touchPosition;
    private Collider2D touchCollider;
    private Hexagon selectedHexagon;

    // Start is called before the first frame update
    void Start()
    {
        // Controller is set for mobile or editor
#if UNITY_EDITOR
        isMobile = false;
        isEditor = true;
#elif UNITY_ANDROID
        isMobile = true;
        isEditor = false;

#elif UNITY_IOS
        isMobile = true;
        isEditor = false;
#endif

        gridManager = GridManager.instance;
    }

    // Update is called once per frame
    void Update()
    {
        //It checks user on the mobile device and everything available
        if (isMobile && gridManager.InputAvailabile())
        {
            //If the user touch on the screen
            if (Input.touchCount > 0)
            {
                //It gets a touch position and looking is there any hexagon on this position
                touchPosition = FindTouchPosition();
                touchCollider = Physics2D.OverlapPoint(touchPosition);
                selectedHexagon = gridManager.GetSelectedHexagon();

                //Then this position is checked to see if this is a selection or a rotation.
                TouchDetection();
                CheckSelectionTouch(touchCollider);
                CheckRotationTouch();
            }
        }
        //It checks user on the editor and everything available
        else if (isEditor && gridManager.InputAvailabile())
        {
            //if user clicks on the screen
            if (Input.GetMouseButtonDown(0))
            {
                //It gets a start click's position and looking is there any hexagon on this position
                touchPosition = FindMousePosition();
                touchCollider = Physics2D.OverlapPoint(touchPosition);
                selectedHexagon = gridManager.GetSelectedHexagon();

                //Then this position is checked
                ClickDetection(touchPosition);
            }

            //When user mouse click up 
            if (Input.GetMouseButtonUp(0))
            {
                //It gets a end of the click position and looking distance. If there isn't distance
                touchEndPosition = FindMousePosition();
                bool isDistance = HasDistance(touchStartPosition, touchEndPosition);

                if (!isDistance)
                {
                    //Selection is checked
                    CheckSelectionClick(touchCollider);
                }
                else
                {
                    //if there is distance. then hexagons rotating
                    RotateHexagons();
                }
            }
        }
    }

    //This function is decided to hexagons rotation.
    // Using touch start position and end position we can find which direction users to drag 
    private bool SetClockWish(Vector2 endPosition, Vector2 firstPosition)
    {
        //Getting distances of x and y
        float distanceX = endPosition.x - firstPosition.x;
        float distanceY = endPosition.y - firstPosition.y;

        //Getting selected hexagon's position
        var selectedPosition = selectedHexagon.transform.position;

        //We should know which direction is bigger than other
        //If user on the x position hexagon rotation can be different
        bool rotateOnX = Math.Abs(distanceX) > Math.Abs(distanceY);
        bool swipeRightUp = rotateOnX ? distanceX > 0 : distanceY > 0;
        bool touchHex = rotateOnX ? endPosition.y > selectedPosition.y : endPosition.x > selectedPosition.x;
        bool clockWise = rotateOnX ? swipeRightUp == touchHex : swipeRightUp != touchHex;

        validTouch = false;
        return clockWise;
    }

    //This function checks the distance of 2 vectors and decided to there is a distance or not
    private bool HasDistance(Vector3 firstPosition, Vector3 secondPosition)
    {
        float distanceX = FindDistance(firstPosition.x, secondPosition.x);
        float distanceY = FindDistance(firstPosition.y, secondPosition.y);

        bool swipeX = Math.Abs(distanceX) > SWIPE_DISTANCE;
        bool swipeY = Math.Abs(distanceY) > SWIPE_DISTANCE;

        return swipeX || swipeY;
    }

    #region Click Methods
    //Gets click's start position
    private void ClickDetection(Vector2 clickPosition)
    {
        validTouch = true;
        touchStartPosition = clickPosition;
    }

    //This function checks selected hexagons
    private void CheckSelectionClick(Collider2D collider)
    {
        if (collider != null && collider.transform.tag == TAG_HEXAGON && validTouch)
        {
            validTouch = false;
            isSelection = true;
            gridManager.SelectHexagons(collider);
        }
    }

    //This function says rotate the selected hexagons if everything is okay
    private void RotateHexagons()
    {
        if (validTouch && isSelection)
        {
            if (selectedHexagon != null)
            {
                bool rotateClockWise = SetClockWish(touchEndPosition, touchPosition);
                gridManager.RotateHexes(rotateClockWise);
            }
        }
    }
    #endregion

    #region Touch Methods
    //Gets touching start position
    private void TouchDetection()
    {
        if (Input.GetTouch(0).phase == TouchPhase.Began)
        {
            validTouch = true;
            touchStartPosition = FindTouchPosition();
        }
    }

    //This function checks selected hexagons
    private void CheckSelectionTouch(Collider2D collider)
    {
        if (collider != null && collider.transform.tag == TAG_HEXAGON)
        {
            if (Input.GetTouch(0).phase == TouchPhase.Ended && validTouch)
            {
                validTouch = false;
                isSelection = true;
                gridManager.SelectHexagons(collider);
            }
        }
    }

    //This function says rotate the selected hexagons if everything is okay
    private void CheckRotationTouch()
    {
        if (Input.GetTouch(0).phase == TouchPhase.Moved && validTouch && isSelection)
        {
            var currentPosition = FindTouchPosition();
            bool isDistance = HasDistance(currentPosition, touchStartPosition);
            if (isDistance)
            {
                bool rotateClockWise = SetClockWish(currentPosition, touchStartPosition);
                gridManager.RotateHexes(rotateClockWise);
            }
        }
    }
    #endregion
}
