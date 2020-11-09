using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseMonoBehaviour : Constants
{
    /// <summary>
    /// It gives gameobject's width and height size
    /// </summary>
    /// <param name="hexagonPrefab">Gameobject for the find width and height</param>
    /// <param name="hexagonWidth">width of gameobject</param>
    /// <param name="hexagonHeight">height of gameobject</param>
    protected void FindGameObjectScale(GameObject hexagonPrefab, out float hexagonWidth, out float hexagonHeight)
    {
        hexagonWidth = hexagonPrefab.transform.localScale.x;
        hexagonHeight = hexagonPrefab.transform.localScale.y;
    }

    /// <summary>
    /// Gives the positions required to centre the grid
    /// </summary>
    /// <returns>Returns position for the grid</returns>
    protected Vector3 CalculateGridPosition()
    {
        float posX = -((GRID_WIDTH - 1) * HEX_HORIZONTAL_DISTANCE) / 2;
        float posY = ((GRID_HEIGHT - 1) * HEX_VERTICAL_DISTANCE) / 2;

        return new Vector3(posX, posY, 0);
    }

    /// <summary>
    /// Hexagon is odd or not. It gives this information.
    /// </summary>
    /// <param name="positionX">Hexagon's coordinate of x</param>
    /// <returns>true => is odd  false => it's not odd</returns>
    public bool IsOddHexagon(int positionX) => positionX % 2 == 0 ? false : true;

    /// <summary>
    /// It's useful for the find mouse position.
    /// </summary>
    /// <returns>returns mouse position</returns>
    public Vector2 FindMousePosition()
    {
        Vector3 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return new Vector2(position.x, position.y);
    }

    /// <summary>
    /// For mobile devices it gives touch position of user fingers
    /// </summary>
    /// <returns>returns vector2 position of touch</returns>
    public Vector2 FindTouchPosition()
    {
        Vector3 position = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
        return new Vector2(position.x, position.y);
    }

    /// <summary>
    /// It returns distance of 2 positions.
    /// </summary>
    /// <param name="lastPosition">last click or touch position</param>
    /// <param name="firstPosition">first click or touch position</param>
    /// <returns>returns distance</returns>
    public float FindDistance(float lastPosition, float firstPosition) => lastPosition - firstPosition;

    /// <summary>
    /// It checks hexagon's neighbours using coordinate system.
    /// </summary>
    /// <param name="position">coordinate of neighbour</param>
    /// <returns>returns true or false</returns>
    public bool IsValidNeighbour(Vector2 position) => !(position.x < 0 || position.x >= GRID_WIDTH || position.y < 0 || position.y >= GRID_HEIGHT);

    /// <summary>
    /// This function calculates of hexagons real position using coordinates.
    /// </summary>
    /// <param name="gridPosition">coordinates of hexagon</param>
    /// <returns>Hexagon's real position</returns>
    public Vector3 CalculateWorldPosition(Vector2 gridPosition)
    {
        float posX = gridPosition.x * HEX_HORIZONTAL_DISTANCE;
        float posY = -(gridPosition.y * HEX_VERTICAL_DISTANCE);

        if (gridPosition.x % 2 != 0)
            posY -= HEX_VERTICAL_DISTANCE / 2;

        return new Vector3(posX, posY, 0);
    }

    /// <summary>
    /// This function converts the two-dimensional array to list
    /// </summary>
    /// <param name="gameGrid">two-dimensional array</param>
    /// <returns>Hexagon list</returns>
    public List<Hexagon> ArrayToList(Hexagon[,] gameGrid)
    {
        List<Hexagon> list = new List<Hexagon>();
        for (int x = 0; x < GRID_WIDTH; x++)
        {
            for (int y = 0; y < GRID_HEIGHT; y++)
            {
                list.Add(gameGrid[x, y]);
            }
        }

        return list;
    }
}
