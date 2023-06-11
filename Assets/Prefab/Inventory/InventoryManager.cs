using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    //stores data about all items within children
    public MasterGrid masterGrid;
    public NPCController npcController;            //sibling

    private void Awake()
    {
        masterGrid = transform.parent.Find("Grid System").gameObject.GetComponent<MasterGrid>();
        npcController = transform.parent.Find("Inventory Manager").gameObject.GetComponent<NPCController>();
    }

    private void Update()
    {
        //null
    }
}