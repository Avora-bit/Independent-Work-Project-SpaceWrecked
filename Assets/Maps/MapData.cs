using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapData : MonoBehaviour
{
    //values should not be changed after session creation
    [SerializeField] private int width = 128, height = 128;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private int minTemp = -300, maxTemp = 1000;
    private Vector3 originPos;
    public enum overlay_Layers
    {
        FLOOR, WALL, CEILING,
        POWER, WATER, HEAT, OXYGEN,
        LIGHTING, RADIATION,
        ACCESS, ROOM,
        OTHER
    };

    private void Awake()
    {
        originPos = new Vector3(-(width * cellSize + cellSize) / 2, -(height * cellSize + cellSize) / 2, 0);
    }

    public int getWidth() { return width; }
    public int getHeight() { return height; }
    public float getCellSize() { return cellSize; }
    public Vector3 getOriginPos() { return new Vector3(-(width * cellSize + cellSize) / 2, -(height * cellSize + cellSize) / 2, 0); }           //fix for game creation, load

    public int getMinTemp() { return minTemp; }
    public int getMaxTemp() { return maxTemp; }
}
