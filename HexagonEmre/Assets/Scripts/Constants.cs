using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Constants : MonoBehaviour
{
    // All constants holding here.
    protected float ROTATE_VALUE = 5f;
    public const float HEX_HORIZONTAL_DISTANCE = 0.78f;
    public const float HEX_VERTICAL_DISTANCE = 0.88f;
    protected const float SWIPE_DISTANCE = 0.1f;
    public static float LERP_VALUE = 6f;
    public static float LERP_DISTANCE = 0.05f;

    public static int GRID_WIDTH { get => GridManager.instance.gridWidth; }
    public static int GRID_HEIGHT { get => GridManager.instance.gridHeight; }

    protected const int SELECTION_COMBINATIONS = 6;
    protected const int SCORE_VALUE = 5;
    public static int BOMB_TIME = 5;
    public static int BOMB_VALUE = 1000;

    public static readonly string NAME_HEXAGON = "Hexagon";
    public static readonly string TAG_HEXAGON = "Hexagon";

    public static bool BOMB_READY = false;

    protected readonly Vector3 OUTLINE_SCALE = new Vector3(1.15f, 1.15f, 0);
}
