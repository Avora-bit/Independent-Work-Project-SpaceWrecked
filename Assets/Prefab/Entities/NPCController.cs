using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour
{
    MapData mapData;
    public MasterGrid masterGrid;
    public InventoryManager inventoryManager;

    public GameObject prefabHuman, prefabDrone;

    BaseEntity testingNPC;

    public List<Task> taskList = new List<Task>();

    private void Awake()
    {
        mapData = FindObjectOfType<MapData>();
        masterGrid = transform.parent.Find("Grid System").gameObject.GetComponent<MasterGrid>();
        inventoryManager = transform.parent.Find("Inventory Manager").gameObject.GetComponent<InventoryManager>();
        
        //for (int i = 0; i < 100; i++)
        //{
        //    spawnEntity(prefabDrone);
        //}
        //spawnEntity(prefabDrone);
        //testingNPC = transform.GetChild(0).GetComponent<BaseEntity>();
    }

    private void Update()
    {
        //constantly try to pick up items
        //if (testingNPC.getItemPtr() == null)
        //{
        //    Debug.Log("waiting for item");
        //    testingNPC.setItemPtr(masterGrid.findNearest((int)testingNPC.transform.position.x, (int)testingNPC.transform.position.y, "Aluminium"));
        //}
        //else
        //{

        //}

        //assign target
        //check movement vector
        //assign path

    }

    public GameObject spawnEntity(GameObject prefab)
    {
        GameObject entity = Instantiate(prefab, transform);
        int xPos = Random.Range(0, mapData.getWidth() / 2);
        int yPos = Random.Range(0, mapData.getHeight() / 2);
        entity.transform.position = new Vector3(xPos, yPos, 0);
        entity.GetComponent<BaseEntity>().setMapInstance(masterGrid);
        return entity;
    }
    public GameObject spawnEntity(int x, int y, GameObject prefab)
    {
        GameObject entity = Instantiate(prefab, transform);
        entity.transform.position = new Vector3(x, y, 0);
        entity.GetComponent<BaseEntity>().setMapInstance(masterGrid);
        return entity;
    }
}
