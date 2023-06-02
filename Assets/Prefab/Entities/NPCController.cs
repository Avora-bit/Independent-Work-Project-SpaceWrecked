using UnityEngine;

public class NPCController : MonoBehaviour
{
    MapData mapData;
    MasterGrid masterGrid;

    [SerializeField] public GameObject prefabHuman, prefabDrone;

    BaseEntity testingNPC;

    private void Awake()
    {
        mapData = FindObjectOfType<MapData>();
        masterGrid = transform.parent.GetChild(1).gameObject.GetComponent<MasterGrid>();

        //for (int i = 0; i < 100; i++)
        //{
        //    spawnEntity(prefabDrone);
        //}
        spawnEntity(prefabDrone);
        testingNPC = transform.GetChild(0).GetComponent<BaseEntity>();
    }

    private void Update()
    {
        //constantly try to pick up items
        //ask inventory controller
        //find nearest
        //pathfind
        //pickup
        testingNPC.setTargetPos(masterGrid.findNearest((int)testingNPC.transform.position.x, (int)testingNPC.transform.position.y, "Aluminium").transform.position);

    }

    public GameObject spawnEntity(GameObject prefab)
    {
        GameObject entity = Instantiate(prefab, transform);
        int xPos = Random.Range(-mapData.getWidth() / 2, mapData.getWidth() / 2);
        int yPos = Random.Range(-mapData.getHeight() / 2, mapData.getHeight() / 2);
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
