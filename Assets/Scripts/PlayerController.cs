using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : BaseSingleton<PlayerController>
{
    [SerializeField] private float moveSpeed = 5, sprintMult = 5;
    [SerializeField] private int edgeScrollBuffer = 10;
    [SerializeField] private float zoomSpeed = 20;
    [SerializeField] private float minZoom = 10, maxZoom = -30;

    private MapData mapData;
    private float minPosX, minPosY, maxPosX, maxPosY;
    private MasterGrid masterGrid;

    public GameObject followCamTarget = null;

    private Vector3 mousePos, screenPos;
    private Vector3 dragLMBstart, dragLMBend;


    public GameObject itemPrefab;


    private bool b_IsLMB = false, b_IsRMB = false, b_IsMMB = false;

    private void Awake()
    {
        //assign reference to map size
        mapData = FindAnyObjectByType<MapData>();
        minPosX = mapData.getOriginPos().x;     //negative
        minPosY = mapData.getOriginPos().y;
        maxPosX = -mapData.getOriginPos().x;    //double negative
        maxPosY = -mapData.getOriginPos().y;

        masterGrid = gameObject.transform.parent.parent.Find("Grid System").GetComponent<MasterGrid>();
    }

    // Update is called once per frame
    void Update()
    {
        if (minPosX == maxPosX && minPosY == maxPosY)           //primitive bounds lock since min and max pos should not be equal,
                                                                //should be replaced with screen loading to ensure managers are created before controllers
        {
            minPosX = mapData.getOriginPos().x;
            minPosY = mapData.getOriginPos().y;
            maxPosX = -mapData.getOriginPos().x;
            maxPosY = -mapData.getOriginPos().y;
        }
        
        //mouse input
        {
            screenPos = Input.mousePosition;
            screenPos.z -= Camera.main.transform.position.z;
            mousePos = Camera.main.ScreenToWorldPoint(screenPos);

            //check if click on UI
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                if (!b_IsLMB && Input.GetMouseButton(0))           //LMB down
                {
                    b_IsLMB = true;
                    dragLMBstart = screenPos;
                }
                else if (b_IsLMB && Input.GetMouseButton(0))       //LMB pressed, exclude first and last frame
                {
                    dragLMBend = screenPos;

                    switch (masterGrid.getRenderLayer())
                    {
                        case 1:             //heat
                            double temp = Mathf.Clamp((float)masterGrid.arrayHeat.getGridObject(mousePos) + 100, mapData.getMinTemp(), mapData.getMaxTemp());
                            masterGrid.arrayHeat.setGridObject(mousePos, temp);
                            break;
                        case 2:             //access
                            masterGrid.pathfindingGrid.getGridObject(mousePos).isWalkable = false;
                            masterGrid.pathfindingGrid.setRebuild(true);
                            break;
                        case 3:             //radiation
                            double rads = Mathf.Clamp((float)masterGrid.arrayRadiation.getGridObject(mousePos) + 25, 0, 100);
                            masterGrid.arrayRadiation.setGridObject(mousePos, rads);
                            break;
                        case 4:             //oxygen
                            double oxygen = Mathf.Clamp((float)masterGrid.arrayOxygen.getGridObject(mousePos) + 25, 0, 100);
                            masterGrid.arrayOxygen.setGridObject(mousePos, oxygen);
                            break;
                        default:            //0
                            break;
                    }
                }
                else if (b_IsLMB && !Input.GetMouseButton(0))      //LMB up
                {
                    b_IsLMB = false;
                    //dragLMBstart, dragLMBend

                    //place item
                    //generate new item stat
                    switch (masterGrid.getRenderLayer())
                    {
                        case 1:             //heat
                            break;
                        case 2:             //access
                            break;
                        case 3:             //radiation
                            break;
                        case 4:             //oxygen
                            break;
                        default:            //0
                            ItemStat newItem = itemPrefab.GetComponent<ItemStat>();
                            if (masterGrid.pathfindingGrid.getGridObject(mousePos) != null)
                            {
                                newItem.xCoord = (int)Mathf.Clamp(mousePos.x, -mapData.getWidth() / 2, mapData.getWidth() / 2);
                                newItem.yCoord = (int)Mathf.Clamp(mousePos.y, -mapData.getHeight() / 2, mapData.getHeight() / 2);
                                GameObject tempItem = Instantiate(itemPrefab, new Vector3(newItem.xCoord, newItem.yCoord, 0), Quaternion.identity);
                                tempItem.transform.SetParent(masterGrid.inventoryManager.transform); 
                            }
                            break;
                    }
                    
                    dragLMBstart = dragLMBend = Vector3.zero;   //reset the position
                }
                else
                {
                    //null
                }

                if (!b_IsRMB && Input.GetMouseButton(1))           //RMB down
                {
                    b_IsRMB = true;

                }
                else if (b_IsRMB && Input.GetMouseButton(1))       //RMB pressed, exclude first and last frame
                {
                    switch (masterGrid.getRenderLayer())
                    {
                        case 1:             //heat
                            double temp = Mathf.Clamp((float)masterGrid.arrayHeat.getGridObject(mousePos) - 100, mapData.getMinTemp(), mapData.getMaxTemp());
                            masterGrid.arrayHeat.setGridObject(mousePos, temp);
                            break;
                        case 2:             //access
                            masterGrid.pathfindingGrid.getGridObject(mousePos).isWalkable = true;
                            masterGrid.pathfindingGrid.setRebuild(true);
                            break;
                        case 3:             //radiation
                            double rads = Mathf.Clamp((float)masterGrid.arrayRadiation.getGridObject(mousePos) - 25, 0, 100);
                            masterGrid.arrayRadiation.setGridObject(mousePos, rads);
                            break;
                        case 4:             //oxygen
                            double oxygen = Mathf.Clamp((float)masterGrid.arrayOxygen.getGridObject(mousePos) - 25, 0, 100);
                            masterGrid.arrayOxygen.setGridObject(mousePos, oxygen);
                            break;
                        default:            //0
                            break;
                    }
                }
                else if (b_IsRMB && !Input.GetMouseButton(1))      //RMB up
                {
                    b_IsRMB = false;
                    switch (masterGrid.getRenderLayer())
                    {
                        case 1:             //heat
                            break;
                        case 2:             //access
                            break;
                        case 3:             //radiation
                            break;
                        case 4:             //oxygen
                            break;
                        default:            //0
                            masterGrid.npcController.spawnEntity((int)mousePos.x, (int)mousePos.y, masterGrid.npcController.prefabDrone);
                            break;
                    }
                }
                else
                {
                    //null
                }

                if (!b_IsMMB && Input.GetMouseButton(2))           //MMB down
                {
                    b_IsMMB = true;
                }
                else if (b_IsMMB && Input.GetMouseButton(2))       //MMB pressed, exclude first and last frame
                {
                    switch (masterGrid.getRenderLayer())
                    {
                        case 1:             //heat
                            break;
                        case 2:             //access
                            PathNode startNode = masterGrid.pathfindingGrid.getGridObject(masterGrid.npcController.transform.GetChild(0).position);
                            PathNode endNode = masterGrid.pathfindingGrid.getGridObject(mousePos);

                            masterGrid.npcController.transform.GetChild(0).GetComponent<BaseEntity>().setTargetPos(mousePos);

                            BaseEntity[] allChildren = masterGrid.npcController.transform.GetComponentsInChildren<BaseEntity>();
                            foreach (BaseEntity child in allChildren)
                            {
                                child.setTargetPos(mousePos);
                            }
                            break;
                        case 3:             //radiation
                            break;
                        case 4:             //oxygen
                            break;
                        default:            //0
                            break;
                    }
                }
                else if (b_IsMMB && !Input.GetMouseButton(2))      //MMB up
                {
                    b_IsMMB = false;
                }
                else
                {
                    //null
                }
            }
        }

        //camera input
        {
            if (followCamTarget == null)
            {
                Vector3 moveDir = new Vector3(0, 0, 0);
                //wasd movement, mouse edge scroll
                if (Input.GetKey(KeyCode.W) || Input.mousePosition.y > Screen.height - edgeScrollBuffer) moveDir.y += moveSpeed;
                if (Input.GetKey(KeyCode.A) || Input.mousePosition.x < edgeScrollBuffer) moveDir.x -= moveSpeed;
                if (Input.GetKey(KeyCode.S) || Input.mousePosition.y < edgeScrollBuffer) moveDir.y -= moveSpeed;
                if (Input.GetKey(KeyCode.D) || Input.mousePosition.x > Screen.width - edgeScrollBuffer) moveDir.x += moveSpeed;

                transform.position += moveDir * Time.unscaledDeltaTime * (Input.GetKey(KeyCode.LeftShift) ? sprintMult : 1);
                //zoom
                if (Input.mouseScrollDelta.y > 0) transform.position = new Vector3(transform.position.x, transform.position.y,
                                                  Mathf.Lerp(transform.position.z, transform.position.z + zoomSpeed, Time.unscaledDeltaTime * zoomSpeed));
                if (Input.mouseScrollDelta.y < 0) transform.position = new Vector3(transform.position.x, transform.position.y,
                                                  Mathf.Lerp(transform.position.z, transform.position.z - zoomSpeed, Time.unscaledDeltaTime * zoomSpeed));
            }
            else
            {
                //disable follow cam
                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D)) followCamTarget = null;

                //has a follow cam target, follow location to target
                transform.position = new Vector3(followCamTarget.transform.position.x, followCamTarget.transform.position.y, maxZoom);
            }

            //camera bounds

            if (transform.position.x <= minPosX) transform.position = new Vector3(minPosX, transform.position.y, transform.position.z);
            if (transform.position.x >= maxPosX) transform.position = new Vector3(maxPosX, transform.position.y, transform.position.z);
            if (transform.position.y <= minPosY) transform.position = new Vector3(transform.position.x, minPosY, transform.position.z);
            if (transform.position.y >= maxPosY) transform.position = new Vector3(transform.position.x, maxPosY, transform.position.z);
            if (transform.position.z >= minZoom) transform.position = new Vector3(transform.position.x, transform.position.y, minZoom);
            if (transform.position.z <= maxZoom) transform.position = new Vector3(transform.position.x, transform.position.y, maxZoom);
        }
    }
}
