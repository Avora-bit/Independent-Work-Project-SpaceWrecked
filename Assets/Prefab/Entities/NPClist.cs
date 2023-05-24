using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPClist : MonoBehaviour
{
    MapData mapData;
    TestGrid testGrid;

    [SerializeField] public GameObject prefabHuman, prefabDrone;

    private void Awake()
    {
        mapData = FindObjectOfType<MapData>();
        testGrid = transform.parent.GetChild(1).gameObject.GetComponent<TestGrid>();


        spawnEntity(prefabHuman);
    }

    private void Update()
    {

    }

    public void spawnEntity(GameObject prefab)
    {
        GameObject entity = Instantiate(prefab, transform);
        int xPos = Random.Range(0, mapData.getWidth() + (int)mapData.getOriginPos().x);
        int yPos = Random.Range(0, mapData.getHeight() + (int)mapData.getOriginPos().y);
        entity.transform.position = new Vector3(xPos, yPos, 0);
        entity.GetComponent<BaseEntity>().setMapInstance(testGrid);
    }
}