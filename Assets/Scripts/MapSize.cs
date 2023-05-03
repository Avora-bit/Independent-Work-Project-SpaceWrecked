using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapSize : MonoBehaviour
{
    //values should not be changed after session creation
    [SerializeField] private int width, height;
    [SerializeField] private float cellSize = 1f;

    public int getWidth() { return width; }
    public int getHeight() { return height; }
    public float getCellSize() { return cellSize; }
}
