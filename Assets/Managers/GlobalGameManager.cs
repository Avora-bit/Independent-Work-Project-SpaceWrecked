using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalGameManager : BaseSingleton<GlobalGameManager>
{
    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        //data should persist when changing scenes
    }

    //linking code for siblings
    /*
     public MasterGrid masterGrid;
    public InventoryManager inventoryManager;
    public ObjectManager objectManager;
    public NPCManager npcManager;
    public TaskManager taskManager;

    masterGrid = transform.parent.Find("Grid System").gameObject.GetComponent<MasterGrid>();
    inventoryManager = transform.parent.Find("Inventory Manager").gameObject.GetComponent<InventoryManager>();
        objectManager = transform.parent.Find("Object Manager").gameObject.GetComponent<ObjectManager>();
        npcManager = transform.parent.Find("NPC Manager").gameObject.GetComponent<NPCManager>();
        taskManager = transform.parent.Find("Task Manager").gameObject.GetComponent<TaskManager>();
     */
}
