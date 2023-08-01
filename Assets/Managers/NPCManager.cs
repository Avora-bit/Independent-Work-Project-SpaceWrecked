using System.Collections.Generic;
using UnityEngine;
using static BaseEntity;

public class NPCManager : MonoBehaviour
{
    MapData mapData;

    public MasterGrid masterGrid;
    public InventoryManager inventoryManager;
    public ObjectManager objectManager;
    public TaskManager taskManager;

    public GameObject prefabHuman, prefabMech, prefabDrone;

    public List<Task> taskList = new List<Task>();

    private void Awake()
    {
        mapData = FindObjectOfType<MapData>();

        masterGrid = transform.parent.Find("Grid System").gameObject.GetComponent<MasterGrid>();
        inventoryManager = transform.parent.Find("Inventory Manager").gameObject.GetComponent<InventoryManager>();
        objectManager = transform.parent.Find("Object Manager").gameObject.GetComponent<ObjectManager>();
        taskManager = transform.parent.Find("Task Manager").gameObject.GetComponent<TaskManager>();

        spawnEntity(0, 0, prefabDrone);
    }

    private void Update()
    {
        //assign target/task
        foreach (Transform child in transform)
        {
            BaseEntity childEntity = child.GetComponent<BaseEntity>();
            if (childEntity.currFSMState == FSMstates.IDLE && childEntity.taskRef == null)
            {
                //task type from 0-7
                //task priority from -3 to 3
                //logic to select tasks
                int taskValue = 0;
                childEntity.taskRef = taskManager.assignTask(childEntity, taskValue);
            }
        }
    }

    public GameObject spawnEntity(int x, int y, GameObject prefab)
    {
        GameObject entity = Instantiate(prefab, transform);
        entity.transform.position = new Vector3(x, y, 0);
        entity.GetComponent<BaseEntity>().setMapInstance(masterGrid);
        return entity;
    }
}
