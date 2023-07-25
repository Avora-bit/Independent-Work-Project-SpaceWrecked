using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

public class ObjectManager : MonoBehaviour
{
    public MasterGrid masterGrid;
    public InventoryManager inventoryManager;
    public NPCManager npcManager;
    public TaskManager taskManager;


    //object manager to be a parent class where all children has a hitbox and relative location. the placement objects is governed by the player controller and blueprint system
    //use of objects to create interact spots and jobs


    private void Awake()
    {
        masterGrid = transform.parent.Find("Grid System").gameObject.GetComponent<MasterGrid>();
        inventoryManager = transform.parent.Find("Inventory Manager").gameObject.GetComponent<InventoryManager>();
        npcManager = transform.parent.Find("NPC Manager").gameObject.GetComponent<NPCManager>();
        taskManager = transform.parent.Find("Task Manager").gameObject.GetComponent<TaskManager>();
    }
}
