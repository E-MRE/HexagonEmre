using System;
using System.Collections.Generic;
using UnityEngine;

public class NeighbourManager : BaseMonoBehaviour
{
    public static NeighbourManager instance = null;
    private GridManager gridManager;
    private NeighbourHexagon neighbourHexes;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    //It returns hexagon's coordinates of neighbours
    public NeighbourHexagon FindNeighbours(int coordinateX, int coordinateY)
    {
        if (gridManager == null)
            gridManager = GridManager.instance;

        neighbourHexes = new NeighbourHexagon();
        bool isOdd = gridManager.IsOddHexagon(coordinateX);

        neighbourHexes.downNeighbour = new Vector2(coordinateX, coordinateY + 1);
        neighbourHexes.upNeighbour = new Vector2(coordinateX, coordinateY - 1);
        neighbourHexes.upLeftNeighbour = new Vector2(coordinateX - 1, isOdd ? coordinateY : coordinateY - 1);
        neighbourHexes.upRightNeighbour = new Vector2(coordinateX + 1, isOdd ? coordinateY : coordinateY - 1);
        neighbourHexes.downLeftNeighbour = new Vector2(coordinateX - 1, isOdd ? coordinateY + 1 : coordinateY);
        neighbourHexes.downRightNeighbour = new Vector2(coordinateX + 1, isOdd ? coordinateY + 1 : coordinateY);

        return neighbourHexes;
    }

    //It returns neighbour colors from grid
    public List<Color> GetNeighbourColors(Hexagon hexagon, Hexagon[,] gameGrid)
    {
        Hexagon firstHexagon, secondHexagon;
        List<Color> bannedColors = new List<Color>();
        Vector2 firstHexCoordinate, secondHexCoordinate;

        var neighbours = FindNeighbours(hexagon.coordinateX, hexagon.coordinateY);

        for (int status = 0; status < SELECTION_COMBINATIONS; status++)
        {
            var isAvailable = CheckNeighbourAvailable(neighbours, status, out firstHexCoordinate, out secondHexCoordinate);

            if (isAvailable)
            {
                firstHexagon = gameGrid[(int)firstHexCoordinate.x, (int)firstHexCoordinate.y];
                secondHexagon = gameGrid[(int)secondHexCoordinate.x, (int)secondHexCoordinate.y];

                if (firstHexagon != null && secondHexagon != null)
                {
                    if (firstHexagon.color == secondHexagon.color)
                    {
                        bannedColors.Add(firstHexagon.color);
                    }
                }

                if (status == 5 && gameGrid[(int)neighbours.upNeighbour.x, (int)neighbours.upNeighbour.y] != null)
                    bannedColors.Add(gameGrid[(int)neighbours.upNeighbour.x, (int)neighbours.upNeighbour.y].color);
            }

        }

        return bannedColors;
    }

    //This function checks available neighbours
    public bool CheckNeighbourAvailable(NeighbourHexagon neighbours, int checkStatus, out Vector2 firstHexCoordinate, out Vector2 secondHexCoordinate)
    {
        switch (checkStatus)
        {
            case 0:
                firstHexCoordinate = neighbours.upNeighbour;
                secondHexCoordinate = neighbours.upRightNeighbour;
                break;
            case 1:
                firstHexCoordinate = neighbours.upRightNeighbour;
                secondHexCoordinate = neighbours.downRightNeighbour;
                break;
            case 2:
                firstHexCoordinate = neighbours.downRightNeighbour;
                secondHexCoordinate = neighbours.downNeighbour;
                break;
            case 3:
                firstHexCoordinate = neighbours.downNeighbour;
                secondHexCoordinate = neighbours.downLeftNeighbour;
                break;
            case 4:
                firstHexCoordinate = neighbours.downLeftNeighbour;
                secondHexCoordinate = neighbours.upLeftNeighbour;
                break;
            case 5:
                firstHexCoordinate = neighbours.upLeftNeighbour;
                secondHexCoordinate = neighbours.upNeighbour;
                break;
            default:
                firstHexCoordinate = new Vector2(-1, -1);
                secondHexCoordinate = new Vector2(-1, -1);
                break;
        }

        bool firstSituation = IsValidNeighbour(firstHexCoordinate);
        bool secondSituation = IsValidNeighbour(secondHexCoordinate);

        return firstSituation && secondSituation;
    }

    //It returns same colors of the neighbours
    public void CheckNeighbourColors(ref List<Hexagon> explodedHexes, List<Hexagon> neighbourList, int x, int y)
    {
        var gameGrid = gridManager.GetGameGrid();
        for (int k = 0; k < neighbourList.Count - 1; ++k)
        {
            if (neighbourList[k] != null && neighbourList[k + 1] != null)
            {
                if (neighbourList[k].color == gameGrid[x, y].color && neighbourList[k + 1].color == gameGrid[x, y].color)
                {
                    if (!explodedHexes.Contains(neighbourList[k]))
                        explodedHexes.Add(neighbourList[k]);
                    if (!explodedHexes.Contains(neighbourList[k + 1]))
                        explodedHexes.Add(neighbourList[k + 1]);
                    if (!explodedHexes.Contains(gameGrid[x, y]))
                        explodedHexes.Add(gameGrid[x, y]);
                }
            }
        }
    }

    //It adds available neighbours to the list
    public void AddAvailableNeighbours(ref List<Hexagon> neighbourList, NeighbourHexagon neighbours, int x, int y)
    {
        var gameGrid = gridManager.GetGameGrid();

        if (IsValidNeighbour(neighbours.upNeighbour))
            neighbourList.Add(gameGrid[(int)neighbours.upNeighbour.x, (int)neighbours.upNeighbour.y]);
        else neighbourList.Add(null);

        if (IsValidNeighbour(neighbours.upRightNeighbour))
            neighbourList.Add(gameGrid[(int)neighbours.upRightNeighbour.x, (int)neighbours.upRightNeighbour.y]);
        else neighbourList.Add(null);

        if (IsValidNeighbour(neighbours.downRightNeighbour))
            neighbourList.Add(gameGrid[(int)neighbours.downRightNeighbour.x, (int)neighbours.downRightNeighbour.y]);
        else neighbourList.Add(null);
    }
}
