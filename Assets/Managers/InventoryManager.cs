using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    //stores data about all items within children
    public MasterGrid masterGrid;
    public ObjectManager objectManager;
    public NPCManager npcManager;
    public TaskManager taskManager;

    private void Awake()
    {
        masterGrid = transform.parent.Find("Grid System").gameObject.GetComponent<MasterGrid>();
        objectManager = transform.parent.Find("Object Manager").gameObject.GetComponent<ObjectManager>();
        npcManager = transform.parent.Find("NPC Manager").gameObject.GetComponent<NPCManager>();
        taskManager = transform.parent.Find("Task Manager").gameObject.GetComponent<TaskManager>();
    }
}