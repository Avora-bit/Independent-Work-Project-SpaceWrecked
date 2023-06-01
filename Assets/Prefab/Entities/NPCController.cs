using UnityEngine;

public class NPCController : MonoBehaviour
{
    MapData mapData;
    TestGrid testGrid;

    [SerializeField] public GameObject prefabHuman, prefabDrone;

    private void Awake()
    {
        mapData = FindObjectOfType<MapData>();
        testGrid = transform.parent.GetChild(1).gameObject.GetComponent<TestGrid>();

        //for (int i = 0; i<100; i++)
        //{
        //    spawnEntity(prefabHuman);
        //}
        spawnEntity(prefabHuman);
    }

    private void Update()
    {

    }

    public void spawnEntity(GameObject prefab)
    {
        GameObject entity = Instantiate(prefab, transform);
        int xPos = Random.Range(-mapData.getWidth()/2, mapData.getWidth()/2);
        int yPos = Random.Range(-mapData.getHeight()/2, mapData.getHeight()/2);
        entity.transform.position = new Vector3(xPos, yPos, 0);
        entity.GetComponent<BaseEntity>().setMapInstance(testGrid);
    }
}
