using System;
using System.Collections.Generic;
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

    [SerializeField] private GameObject cursor;
    private List<GameObject> dragHintList;
    public GameObject BuildHintPrefab;
    public GameObject BlueprintPrefab;

    public GameObject followCamTarget = null;

    public Vector3 mousePos, screenPos;
    public Vector3 dragLMBstart, dragLMBend;

    public GameObject itemPrefab;

    private bool b_IsLMB = false, b_IsRMB = false, b_IsMMB = false;

    private Vector3 offsets;            //map offsets

    private LineRenderer cameraBounds;

    public enum BuildMode
    {
        None, 
        BuildFloor,
        DestroyFloor,
        BuildWall,
        DestroyWall
    }

    private BuildMode buildMode = BuildMode.None;
    private TileMapObject.TileType tileType = TileMapObject.TileType.None;

    public string BlueprintPlacedMessage = "Blueprint Placed";
    public string cannotbuildMessage = "Cannot Build Here!";
    public string InteractionSpotBlockedMessage = "Interaction Spot Blocked!";

    private void Awake()
    {
        //assign reference to map size
        mapData = FindAnyObjectByType<MapData>();
        minPosX = mapData.getOriginPos().x + mapData.getCellSize() * 16;     //negative
        minPosY = mapData.getOriginPos().y + mapData.getCellSize() * 16;
        maxPosX = -mapData.getOriginPos().x - mapData.getCellSize() * 16;    //double negative
        maxPosY = -mapData.getOriginPos().y - mapData.getCellSize() * 16;

        masterGrid = gameObject.transform.parent.parent.Find("Grid System").GetComponent<MasterGrid>();
        offsets = mapData.getOriginPos() + new Vector3(mapData.getCellSize() / 2, mapData.getCellSize() / 2, 0);

        dragHintList = new List<GameObject>();
        cameraBounds = transform.GetChild(0).GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        //mouse input
        {
            screenPos = Input.mousePosition;
            screenPos.z -= Camera.main.transform.position.z;
            mousePos = Camera.main.ScreenToWorldPoint(screenPos);

            if (masterGrid.pathfindingGrid.getGridObject(mousePos) != null)
            {
                cursor.SetActive(true);
                cursor.transform.position = new Vector3(masterGrid.pathfindingGrid.getGridObject(mousePos).x, masterGrid.pathfindingGrid.getGridObject(mousePos).y, 0) + offsets;
            }
            else
            {
                //location is null
                cursor.SetActive(false);
            }

            // Clean up old drag previews
            while (dragHintList.Count > 0)          //use object pooling
            {
                GameObject go = dragHintList[0];
                dragHintList.RemoveAt(0);
                Destroy(go);
            }

            //check if cursor is on UI
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                //zoom
                if (Input.mouseScrollDelta.y > 0) transform.position = new Vector3(transform.position.x, transform.position.y,
                                                  Mathf.Lerp(transform.position.z, transform.position.z + zoomSpeed, Time.unscaledDeltaTime * zoomSpeed));
                if (Input.mouseScrollDelta.y < 0) transform.position = new Vector3(transform.position.x, transform.position.y,
                                                  Mathf.Lerp(transform.position.z, transform.position.z - zoomSpeed, Time.unscaledDeltaTime * zoomSpeed));

                if (!b_IsLMB && Input.GetMouseButton(0))           //LMB down
                {
                    b_IsLMB = true;
                    dragLMBstart = mousePos;
                }
                else if (b_IsLMB && Input.GetMouseButton(0))       //LMB pressed, exclude first and last frame
                {
                    dragLMBend = mousePos;

                    switch (masterGrid.getRenderLayer())
                    {
                        case 1:             //heat
                            double temp = Mathf.Clamp((float)masterGrid.arrayHeat.getGridObject(mousePos) + 100, mapData.getMinTemp(), mapData.getMaxTemp());
                            masterGrid.arrayHeat.setGridObject(mousePos, temp);
                            break;
                        case 2:             //access
                            masterGrid.pathfindingGrid.getGridObject(mousePos).isSolid = true;
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
                        default:            //tilemap
                            //set floor to tile
                            //start drag
                            {
                                int start_x = (int)(dragLMBstart.x/mapData.getCellSize() - mapData.getOriginPos().x);
                                int end_x = (int)(dragLMBend.x / mapData.getCellSize() - mapData.getOriginPos().x);
                                int start_y = (int)(dragLMBstart.y / mapData.getCellSize() - mapData.getOriginPos().y);
                                int end_y = (int)(dragLMBend.y / mapData.getCellSize() - mapData.getOriginPos().y);

                                if (end_x < start_x) (start_x, end_x) = (end_x, start_x);
                                if (end_y < start_y) (start_y, end_y) = (end_y, start_y);

                                for (int x = start_x; x <= end_x; x++)
                                {
                                    for (int y = start_y; y <= end_y; y++)
                                    {
                                        if (masterGrid.tilemapGrid.getGridObject(x, y) != null)
                                        {
                                            // Display the building hint on top of this tile position
                                            GameObject go = Instantiate(BuildHintPrefab, new Vector3(x + offsets.x, y + offsets.y, 0), Quaternion.identity);
                                            go.transform.SetParent(masterGrid.transform.GetChild(3), true);     //create hints in new child
                                            dragHintList.Add(go);
                                        }
                                    }
                                }
                            }

                            break;
                    }
                }
                else if (b_IsLMB && !Input.GetMouseButton(0))      //LMB up
                {
                    b_IsLMB = false;

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
                        default:            //tilemap
                            //end drag
                            {
                                foreach (Transform child in masterGrid.transform.GetChild(3))       //check all build hints
                                {
                                    if (buildMode != BuildMode.None && buildMode != BuildMode.DestroyFloor && buildMode != BuildMode.DestroyWall)
                                    {
                                        masterGrid.tilemapGrid.getGridObject(child.position).setTileType(tileType);
                                        masterGrid.tilemapGrid.setRebuild(true);
                                        masterGrid.pathfindingGrid.getGridObject(child.position).isSpace = false;           //hardcoded value
                                        masterGrid.pathfindingGrid.setRebuild(true);
                                    }
                                    else if (buildMode == BuildMode.DestroyFloor || buildMode == BuildMode.DestroyWall)
                                    {
                                        masterGrid.tilemapGrid.getGridObject(child.position).setTileType(TileMapObject.TileType.None);
                                        masterGrid.tilemapGrid.setRebuild(true);
                                        masterGrid.pathfindingGrid.getGridObject(child.position).isSpace = true;            //hardcoded value
                                        masterGrid.pathfindingGrid.setRebuild(true);
                                    }
                                    
                                    //spawn prefabs 
                                    GameObject go = Instantiate(BlueprintPrefab, child.transform.position, Quaternion.identity);
                                    go.transform.SetParent(masterGrid.transform.GetChild(3), true);         //create blueprints in new children

                                }
                            }

                            break;
                    }

                    //dragLMBstart, dragLMBend
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
                            masterGrid.pathfindingGrid.getGridObject(mousePos).isSolid = false;
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
                        default:            //tilemap
                            //spawn new item
                            ItemStat newItem = itemPrefab.GetComponent<ItemStat>();
                            if (masterGrid.pathfindingGrid.getGridObject(mousePos) != null)
                            {
                                GameObject tempItem = Instantiate(itemPrefab, cursor.transform);
                                tempItem.transform.SetParent(masterGrid.inventoryManager.transform);
                            }
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
                        default:            //tilemap
                            //spawn new NPC
                            masterGrid.npcController.spawnEntity((int)mousePos.x, (int)mousePos.y, masterGrid.npcController.prefabDrone);
                            masterGrid.npcController.spawnEntity((int)mousePos.x, (int)mousePos.y, masterGrid.npcController.prefabHuman);
                            masterGrid.npcController.spawnEntity((int)mousePos.x, (int)mousePos.y, masterGrid.npcController.prefabMech);

                            //reset the control scheme
                            

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
                            //PathNode startNode = masterGrid.pathfindingGrid.getGridObject(masterGrid.npcController.transform.GetChild(0).position);
                            //PathNode endNode = masterGrid.pathfindingGrid.getGridObject(mousePos);

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
                Vector3 moveDir = Vector3.zero;
                //wasd movement, mouse edge scroll
                if (Input.GetKey(KeyCode.W) || Input.mousePosition.y > Screen.height - edgeScrollBuffer) moveDir.y += moveSpeed;
                if (Input.GetKey(KeyCode.A) || Input.mousePosition.x < edgeScrollBuffer) moveDir.x -= moveSpeed;
                if (Input.GetKey(KeyCode.S) || Input.mousePosition.y < edgeScrollBuffer) moveDir.y -= moveSpeed;
                if (Input.GetKey(KeyCode.D) || Input.mousePosition.x > Screen.width - edgeScrollBuffer) moveDir.x += moveSpeed;

                transform.position += Time.unscaledDeltaTime * (Input.GetKey(KeyCode.LeftShift) ? sprintMult : 1) * moveDir;
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

            Vector3 topleft = Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height, -Camera.main.transform.position.z));
            Vector3 topright = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, -Camera.main.transform.position.z));
            Vector3 bottomleft = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, -Camera.main.transform.position.z));
            Vector3 bottomright = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0, -Camera.main.transform.position.z));
            
            Vector3[] lineVector = new Vector3[4];
            lineVector[0] = topleft;
            lineVector[1] = topright;
            lineVector[2] = bottomright;
            lineVector[3] = bottomleft;

            cameraBounds.SetPositions(lineVector);
        }
    }

    //building
    public void ResetControl()
    {
        buildMode = BuildMode.None;
        tileType = TileMapObject.TileType.None;
    }
    public void setBuildMode(string buildMode)
    {
        BuildMode parsed_mode = (BuildMode)Enum.Parse(typeof(BuildMode), buildMode);
        this.buildMode = parsed_mode;
    }
    public void setMaterial(string tileType)
    {
        TileMapObject.TileType parsed_mode = (TileMapObject.TileType)Enum.Parse(typeof(TileMapObject.TileType), tileType);
        this.tileType = parsed_mode;
    }
}
