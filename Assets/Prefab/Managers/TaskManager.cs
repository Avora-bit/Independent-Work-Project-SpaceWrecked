using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    public MasterGrid masterGrid;
    public InventoryManager inventoryManager;
    public ObjectManager objectManager;
    public NPCManager npcManager;

    List<Task> taskList = new List<Task>();
    TimeController timeController;

    bool sortedToday = false;

    private void Awake()
    {
        masterGrid = transform.parent.Find("Grid System").gameObject.GetComponent<MasterGrid>();
        inventoryManager = transform.parent.Find("Inventory Manager").gameObject.GetComponent<InventoryManager>();
        objectManager = transform.parent.Find("Object Manager").gameObject.GetComponent<ObjectManager>();
        npcManager = transform.parent.Find("NPC Manager").gameObject.GetComponent<NPCManager>();

        timeController = FindObjectOfType<TimeController>();
    }

    void Update()
    {
        //update the task every 0000, so the new tasks are created unordered
        //ignore new tasks as current tasks are more important, prevent unneeded sorting, reduce computation
        if (!sortedToday && timeController.getHours() == 0)
        {
            sortedToday = true;
            taskList.Sort((p1, p2) => p1.priority.CompareTo(p2.priority));          //sorting based on strength of priority
            //ignore the skill/work type as they are unordered
        }
        else if (sortedToday && timeController.getHours() == 1)
        {
            sortedToday = false;
        }
    }

    public Task assignTask(BaseEntity entity, int type)
    {
        //search the list for the type, return the task
        Task assign = null;
        foreach (Task task in taskList)
        {
            if (task.type == type)
            {
                assign = task;      //return the first one found
                assign.entity = entity;
                break;
            }
        }
        return assign;
    }
}
