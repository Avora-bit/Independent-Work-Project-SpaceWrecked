using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boundary : MonoBehaviour
{
    private MapData mapData;
    [SerializeField] private GameObject boundaryPrefab;

    private void Awake()
    {
        mapData = FindObjectOfType<MapData>();

        int height = mapData.getHeight();
        int width = mapData.getWidth();

        //bottom left and right
        GameObject cornerBoundary = Instantiate(boundaryPrefab, transform);
        cornerBoundary.transform.position = new Vector3(mapData.getOriginPos().x - 8, mapData.getOriginPos().y - 8, 0);
        cornerBoundary = Instantiate(boundaryPrefab, transform);
        cornerBoundary.transform.position = new Vector3(-mapData.getOriginPos().x + 8 - 1, mapData.getOriginPos().y - 8, 0);
        //top left and right
        cornerBoundary = Instantiate(boundaryPrefab, transform);
        cornerBoundary.transform.position = new Vector3(mapData.getOriginPos().x - 8, -mapData.getOriginPos().y + 8 - 1, 0);
        cornerBoundary = Instantiate(boundaryPrefab, transform);
        cornerBoundary.transform.position = new Vector3(-mapData.getOriginPos().x + 8 - 1, -mapData.getOriginPos().y + 8 - 1, 0);

        //left and right
        for (int i = 0; i < height/16; i++)
        {
            GameObject leftBoundary = Instantiate(boundaryPrefab, transform);
            leftBoundary.transform.position = new Vector3(mapData.getOriginPos().x - 8, mapData.getOriginPos().y + 8 + i * 16, 0);

            GameObject rightBoundary = Instantiate(boundaryPrefab, transform);
            rightBoundary.transform.position = new Vector3(-mapData.getOriginPos().x + 8 - 1, mapData.getOriginPos().y + 8 + i * 16, 0);
        }
        //up and down
        for (int i = 0; i < width/16; i++)
        {
            GameObject downBoundary = Instantiate(boundaryPrefab, transform);
            downBoundary.transform.position = new Vector3(mapData.getOriginPos().x + 8 + i * 16, -mapData.getOriginPos().y + 8 - 1, 0);

            GameObject upBoundary = Instantiate(boundaryPrefab, transform);
            upBoundary.transform.position = new Vector3(mapData.getOriginPos().x + 8 + i * 16, mapData.getOriginPos().y - 8, 0);
        }
    }
}
