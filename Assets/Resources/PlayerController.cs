using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
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
    private string tileStringData;

    [SerializeField] private GameObject cursor;
    private List<GameObject> dragHintList;
    public GameObject BuildHintPrefab;
    public GameObject BlueprintPrefab;

    public GameObject followCamTarget = null;

    public Vector3 mousePos, screenPos;
    public Vector3 dragLMBstart, dragLMBend;
    public Vector3 mouseRollBack;
    public bool mousePosChanged = false;

    public GameObject itemPrefab;
    public GameObject buildPrefab;

    private int placeRotation = 0;

    private Vector3 offsets;            //map offsets

    private LineRenderer cameraBounds;

    public enum BuildMode
    {
        None,                   //disable drag
        BuildFloor,             //2d drag
        DestroyFloor,           //2d drag
        BuildWall,              //directional drag
        DestroyWall,            //2d drag
        BuildInteractable,      //disable drag
        DestroyInteractable,    //disable drag
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
            mousePosChanged = false;
            mouseRollBack = cursor.transform.position;           //stores last frame mouse position/selection
            screenPos = Input.mousePosition;
            screenPos.z -= Camera.main.transform.position.z;
            mousePos = Camera.main.ScreenToWorldPoint(screenPos);

            if (buildMode == BuildMode.BuildInteractable && buildPrefab != null)
            {
                //render the gameobject on the cursor, press QE to rotate clockwise
                if (Input.GetKeyDown(KeyCode.Q)) placeRotation += 90;
                if (Input.GetKeyDown(KeyCode.E)) placeRotation -= 90;
                if (buildPrefab != null) buildPrefab.transform.eulerAngles = new Vector3(0, 0, placeRotation);
            }
            else placeRotation = 0;

            if (placeRotation < 0) placeRotation += 360;
            if (placeRotation > 360) placeRotation -= 360;

            if (!EventSystem.current.IsPointerOverGameObject())
            {
                cursor.SetActive(true);
                Vector3 clampedPos = new Vector3(Mathf.Clamp(mousePos.x, offsets.x, offsets.x + mapData.getWidth() * mapData.getCellSize()),
                                                 Mathf.Clamp(mousePos.y, offsets.y, offsets.y + mapData.getHeight() * mapData.getCellSize()), 0);
                cursor.transform.position = new Vector3(masterGrid.pathfindingGrid.getGridObject(clampedPos).x, masterGrid.pathfindingGrid.getGridObject(clampedPos).y, 0) + offsets;
                tileStringData = masterGrid.pathfindingGrid.getGridObject(cursor.transform.position).ToString() + "\n" +
                                  masterGrid.pathfindingGrid.getGridObject(cursor.transform.position).isSpace + "\n" +
                                  masterGrid.pathfindingGrid.getGridObject(cursor.transform.position).isSolid + "\n";
            }
            else
            {
                //location is null
                cursor.SetActive(false);
            }
            if (mouseRollBack != cursor.transform.position)
            {
                mousePosChanged = true;
                //tile 
                cursor.transform.GetChild(1).GetChild(0).GetComponent<TextMeshPro>().text = tileStringData;
            }
            else
            {
                //null, else lock
            }

            //check if cursor is not on UI
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                //zoom
                if (Input.mouseScrollDelta.y > 0) transform.position = new Vector3(transform.position.x, transform.position.y,
                                                  Mathf.Lerp(transform.position.z, transform.position.z + zoomSpeed, Time.unscaledDeltaTime * zoomSpeed));
                if (Input.mouseScrollDelta.y < 0) transform.position = new Vector3(transform.position.x, transform.position.y,
                                                  Mathf.Lerp(transform.position.z, transform.position.z - zoomSpeed, Time.unscaledDeltaTime * zoomSpeed));

                if (Input.GetMouseButtonDown(0))           //LMB down
                {
                    dragLMBstart = mousePos;
                }
                else if (Input.GetMouseButton(0))       //LMB pressed, exclude first and last frame
                {
                    dragLMBend = mousePos;

                    switch (masterGrid.getRenderLayer())
                    {
                        case 1:             //heat
                            double temp = Mathf.Clamp((float)masterGrid.arrayHeat.getGridObject(mousePos) + 100, mapData.getMinTemp(), mapData.getMaxTemp());
                            masterGrid.arrayHeat.setGridObject(mousePos, temp);
                            break;
                        case 2:             //access            //null for now
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
                            //start drag
                            int start_x = (int)(dragLMBstart.x / mapData.getCellSize() - mapData.getOriginPos().x);
                            int end_x = (int)(dragLMBend.x / mapData.getCellSize() - mapData.getOriginPos().x);
                            int start_y = (int)(dragLMBstart.y / mapData.getCellSize() - mapData.getOriginPos().y);
                            int end_y = (int)(dragLMBend.y / mapData.getCellSize() - mapData.getOriginPos().y);

                            if (end_x < start_x) (start_x, end_x) = (end_x, start_x);               //swap data for inverse drag direction
                            if (end_y < start_y) (start_y, end_y) = (end_y, start_y);               //ensure start position is bottom left

                            if (mousePosChanged)
                            {
                                resetDraggable();
                                switch (buildMode)
                                {
                                    case BuildMode.BuildFloor:
                                    case BuildMode.DestroyFloor:
                                    case BuildMode.DestroyWall:
                                    case BuildMode.DestroyInteractable:
                                        //2D drag
                                        //blanket over the square
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

                                                    //check the tile and the associated layer
                                                    bool state = true;
                                                    if (buildMode != BuildMode.DestroyWall && buildMode != BuildMode.DestroyInteractable)
                                                    {
                                                        if (!masterGrid.pathfindingGrid.getGridObject(x, y).isSpace) state = false;
                                                        if (buildMode == BuildMode.DestroyFloor)
                                                        {
                                                            state = !state;     //inverse state for destroy
                                                            if (masterGrid.pathfindingGrid.getGridObject(x, y).isSolid) state = !state;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (!masterGrid.pathfindingGrid.getGridObject(x, y).isSolid) state = false;
                                                    }
                                                    go.GetComponent<BuildHint>().Check(state);

                                                }
                                            }
                                        }
                                        break;
                                    case BuildMode.BuildWall:
                                        //directional drag
                                        //compare which direction is longer
                                        //directional drag
                                        {
                                            int distX = end_x - start_x;
                                            int distY = end_y - start_y;
                                            bool isVertical = true;
                                            if (distX > distY) isVertical = false;      //horizontal
                                            if (isVertical)
                                            {
                                                for (int y = start_y; y <= end_y; y++)
                                                {
                                                    if (masterGrid.tilemapGrid.getGridObject(masterGrid.tilemapGrid.getGridObject(dragLMBstart).x, y) != null)
                                                    {
                                                        //Display the building hint on top of this tile position
                                                        GameObject go = Instantiate(BuildHintPrefab, new Vector3(masterGrid.tilemapGrid.getGridObject(dragLMBstart).x + offsets.x, y + offsets.y, 0), Quaternion.identity);
                                                        go.transform.SetParent(masterGrid.transform.GetChild(3), true);     //create hints in new child
                                                        dragHintList.Add(go);

                                                        //check the tile and the associated layer
                                                        bool state = true;
                                                        if (masterGrid.pathfindingGrid.getGridObject(masterGrid.tilemapGrid.getGridObject(dragLMBstart).x, y).isSpace ||
                                                            masterGrid.pathfindingGrid.getGridObject(masterGrid.tilemapGrid.getGridObject(dragLMBstart).x, y).isSolid) state = false;
                                                        go.GetComponent<BuildHint>().Check(state);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                for (int x = start_x; x <= end_x; x++)
                                                {
                                                    if (masterGrid.tilemapGrid.getGridObject(x, masterGrid.tilemapGrid.getGridObject(dragLMBstart).y) != null)
                                                    {
                                                        //Display the building hint on top of this tile position
                                                        GameObject go = Instantiate(BuildHintPrefab, new Vector3(x + offsets.x, masterGrid.tilemapGrid.getGridObject(dragLMBstart).y + offsets.y, 0), Quaternion.identity);
                                                        go.transform.SetParent(masterGrid.transform.GetChild(3), true);     //create hints in new child
                                                        dragHintList.Add(go);

                                                        //check the tile and the associated layer
                                                        bool state = true;
                                                        if (masterGrid.pathfindingGrid.getGridObject(x, masterGrid.tilemapGrid.getGridObject(dragLMBstart).y).isSpace ||
                                                            masterGrid.pathfindingGrid.getGridObject(x, masterGrid.tilemapGrid.getGridObject(dragLMBstart).y).isSolid) state = false;
                                                        go.GetComponent<BuildHint>().Check(state);
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                    case BuildMode.None: //disable drag
                                    case BuildMode.BuildInteractable:
                                        //render the gameobject on the cursor, press QE to rotate clockwise
                                        //display buildhints based on where the object is being placed
                                        //display the red state if either the tile is space or occupied
                                        
                                        break;
                                    default:
                                        break;
                                }
                            }
                            break;
                    }
                }
                else if (Input.GetMouseButtonUp(0))      //LMB up
                {
                    List<Vector2> occupancy;
                    switch (masterGrid.getRenderLayer())
                    {
                        case 1:             //heat
                            break;
                        case 2:             //access        null for now
                            break;
                        case 3:             //radiation
                            break;
                        case 4:             //oxygen
                            break;
                        default:            //tilemap
                            //end drag
                            {
                                switch (buildMode)
                                {
                                    default:
                                    case BuildMode.None:
                                        break;
                                    case BuildMode.BuildFloor:
                                        foreach (Transform child in masterGrid.transform.GetChild(3))
                                        {
                                            if (!child.gameObject.GetComponent<BuildHint>().state) continue;
                                            masterGrid.tilemapGrid.getGridObject(child.position).setTileType(tileType);
                                            masterGrid.pathfindingGrid.getGridObject(child.position).isSpace = false;
                                        }
                                        break;
                                    case BuildMode.DestroyFloor:
                                        foreach (Transform child in masterGrid.transform.GetChild(3))
                                        {
                                            if (!child.gameObject.GetComponent<BuildHint>().state) continue;
                                            masterGrid.tilemapGrid.getGridObject(child.position).setTileType(TileMapObject.TileType.None);
                                            masterGrid.pathfindingGrid.getGridObject(child.position).isSpace = true;
                                        }
                                        break;
                                    case BuildMode.BuildWall:
                                        foreach (Transform child in masterGrid.transform.GetChild(3))
                                        {
                                            if (!child.gameObject.GetComponent<BuildHint>().state) continue;
                                            //spawn wall here
                                            GameObject wallInstance = Resources.Load<GameObject>("Inventory/Structure/" + tileType.ToString() + "Wall");
                                            wallInstance = Instantiate(wallInstance, child.position, Quaternion.Euler(Vector3.zero));
                                            wallInstance.GetComponent<InstalledObject>().install();
                                            wallInstance.transform.parent = masterGrid.objectManager.transform;
                                            occupancy = wallInstance.GetComponent<InstalledObject>().GetGridPositionList();
                                            foreach (Vector2 gridPos in occupancy)
                                            {
                                                masterGrid.pathfindingGrid.getGridObject(gridPos).isSolid = true;
                                            }
                                        }
                                        break;
                                    case BuildMode.DestroyWall:
                                    case BuildMode.DestroyInteractable:
                                        foreach (Transform child in masterGrid.transform.GetChild(3))
                                        {
                                            if (!child.gameObject.GetComponent<BuildHint>().state) continue;
                                            //check which tile and what is on the tile
                                            foreach (Transform installedObject in masterGrid.objectManager.transform)
                                            {
                                                occupancy = installedObject.gameObject.GetComponent<InstalledObject>().GetGridPositionList();
                                                foreach (Vector2 gridPos in occupancy)
                                                {
                                                    if (new Vector3(gridPos.x, gridPos.y, 0) == child.position)
                                                    {
                                                        //destroy object and tiles to empty
                                                        foreach (Vector2 destroyablePos in occupancy)
                                                        {
                                                            masterGrid.pathfindingGrid.getGridObject(destroyablePos).isSolid = false;
                                                        }
                                                        Destroy(installedObject.gameObject);
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                    case BuildMode.BuildInteractable:
                                        //place on the cursor with respect to rotation
                                        GameObject newObject = Instantiate(buildPrefab, cursor.transform.position, Quaternion.Euler(new Vector3(0, 0, placeRotation)));
                                        newObject.transform.parent = masterGrid.objectManager.transform;
                                        newObject.GetComponent<InstalledObject>().install();
                                        occupancy = newObject.GetComponent<InstalledObject>().GetGridPositionList();
                                        bool openSlot = true;
                                        foreach (Vector2 gridPos in occupancy)
                                        {
                                            if (masterGrid.pathfindingGrid.getGridObject(gridPos).isSolid || masterGrid.pathfindingGrid.getGridObject(gridPos).isSpace)
                                            {
                                                openSlot = false;
                                                Destroy(newObject);
                                                cursor.transform.GetChild(1).GetChild(0).GetComponent<TextMeshPro>().text = cannotbuildMessage;
                                                break;
                                            }
                                        }
                                        if (openSlot)
                                        {
                                            foreach (Vector2 gridPos in occupancy)
                                            {
                                                masterGrid.pathfindingGrid.getGridObject(gridPos).isSolid = true;
                                            }
                                        }
                                        break;
                                        //spawn prefabs 
                                        //if (!masterGrid.pathfindingGrid.getGridObject(child.position).isSolid)
                                        //{   //not solid, then place solid item
                                        //    GameObject go = Instantiate(BlueprintPrefab, child);
                                        //    go.transform.SetParent(masterGrid.transform.GetChild(4), true);         //create blueprints in new children
                                        //}
                                        //else
                                        //{   //is solid, or is space, then can place floor
                                        //    GameObject go = Instantiate(BlueprintPrefab, child);
                                        //    go.transform.SetParent(masterGrid.transform.GetChild(4), true);         //create blueprints in new children
                                        //}
                                }

                                masterGrid.arrayLight.setRebuild(true);
                                masterGrid.tilemapGrid.setRebuild(true);
                                masterGrid.pathfindingGrid.setRebuild(true);
                                resetDraggable();       //clear the parent class
                            }
                            break;
                    }
                    dragLMBstart = dragLMBend = Vector3.zero;   //reset the position
                }
                else
                {
                    //null
                }

                if (Input.GetMouseButtonDown(1))           //RMB down
                {

                }
                else if (Input.GetMouseButton(1))       //RMB pressed, exclude first and last frame
                {
                    switch (masterGrid.getRenderLayer())
                    {
                        case 1:             //heat
                            double temp = Mathf.Clamp((float)masterGrid.arrayHeat.getGridObject(mousePos) - 100, mapData.getMinTemp(), mapData.getMaxTemp());
                            masterGrid.arrayHeat.setGridObject(mousePos, temp);
                            break;
                        case 2:             //access                //null for now
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
                            if (itemPrefab != null && cursor.activeInHierarchy == true)
                            {
                                GameObject tempItem = Instantiate(itemPrefab, cursor.transform);
                                tempItem.transform.SetParent(masterGrid.inventoryManager.transform, true);
                            }
                            break;
                    }
                }
                else if (Input.GetMouseButtonUp(1))      //RMB up
                {
                    switch (masterGrid.getRenderLayer())
                    {
                        case 1:             //heat
                            break;
                        case 2:             //access            //null for now
                            break;
                        case 3:             //radiation
                            break;
                        case 4:             //oxygen
                            break;
                        default:            //tilemap
                            //spawn new NPC
                            //masterGrid.npcManager.spawnEntity((int)mousePos.x, (int)mousePos.y, masterGrid.npcManager.prefabDrone);
                            //masterGrid.npcManager.spawnEntity((int)mousePos.x, (int)mousePos.y, masterGrid.npcManager.prefabHuman);
                            //masterGrid.npcManager.spawnEntity((int)mousePos.x, (int)mousePos.y, masterGrid.npcManager.prefabMech);

                            //reset the control scheme

                            break;
                    }
                }
                else
                {
                    //null
                }

                if (Input.GetMouseButtonDown(2))           //MMB down
                {

                }
                else if (Input.GetMouseButton(2))       //MMB pressed, exclude first and last frame
                {
                    switch (masterGrid.getRenderLayer())
                    {
                        case 1:             //heat
                            break;
                        case 2:             //access                //null for now
                            break;
                        case 3:             //radiation
                            break;
                        case 4:             //oxygen
                            break;
                        default:            //0
                            break;
                    }
                }
                else if (Input.GetMouseButtonUp(2))      //MMB up
                {

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

    //build hint
    public void resetDraggable()
    {
        while (dragHintList.Count > 0)
        {
            GameObject go = dragHintList[0];
            dragHintList.RemoveAt(0);
            Destroy(go);
        }
    }

    //building
    public void ResetControl()
    {
        buildMode = BuildMode.None;
        tileType = TileMapObject.TileType.None;
        Destroy(itemPrefab);
        Destroy(buildPrefab);
    }
    public void setBuildMode(string buildMode)
    {
        BuildMode parsed = (BuildMode)Enum.Parse(typeof(BuildMode), buildMode);
        this.buildMode = parsed;
    }
    public void setMaterial(string tileType)
    {
        Destroy(buildPrefab);
        TileMapObject.TileType parsed = (TileMapObject.TileType)Enum.Parse(typeof(TileMapObject.TileType), tileType);
        this.tileType = parsed;
        itemPrefab = Instantiate(Resources.Load<GameObject>("Inventory/Materials/" + tileType), cursor.transform);
    }

    public void setInstalledObject(string objectName)
    {
        Destroy(buildPrefab);
        buildPrefab = Instantiate(Resources.Load<GameObject>("Inventory/Interactables/" + objectName), cursor.transform);
    }
}
