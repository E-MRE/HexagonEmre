using UnityEngine;

//This model holds the coordinate data about the hexagons' neighbors
public class NeighbourHexagon
{
    public Vector2 downNeighbour { get; set; }
    public Vector2 downRightNeighbour { get; set; }
    public Vector2 downLeftNeighbour { get; set; }
    public Vector2 upNeighbour { get; set; }
    public Vector2 upRightNeighbour { get; set; }
    public Vector2 upLeftNeighbour { get; set; }
}
