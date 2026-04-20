using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class ViewerController : MonoBehaviour
{
    [SerializeField]private float turnSpeed;
    //Struct들의 정리를 위한 부모 오브젝트
    [SerializeField]private GameObject structsObj;

    [SerializeField]private MeshRenderer meshRenderer;
    private bool isDrawFloor;
    [SerializeField]private GameObject floorStructs;

    //Viewer움직임, 높이, 위치조정을 위한 Offset 변수들
    #region ViewerStructOffset
    private int nowNum;
    private Vector3 gridSize;
    [SerializeField]private new Vector3 viewerSize;
    [SerializeField]private Vector3 gridMove;
    [SerializeField]private Vector3 trueVec;

    [SerializeField]private float viewerOffsetX = 0;
    [SerializeField]private float viewerOffsetY = 0;
    [SerializeField]private float viewerOffsetZ = 0;

    private float yPos = 0.5f;
    #endregion

    //Eraser Draw Disable 상태의 구분을 위한 material
    #region ViewerMat
    [SerializeField]private Material eraserMat;
    [SerializeField]private Material drawMat;
    [SerializeField]private Material disableMat;
    #endregion

    //기타 Offset 수정가능 여, 지우, 그리기 가능 여부 확인용 bool
    #region CanDrawEraserEditOffset
    private bool isEraser = false;
    private bool isCanDraw = true;
    private bool isOffsetXZEdit;
    private RaycastHit hit;
    #endregion

    #region System
    [SerializeField]private DataManager dataManager;
    [SerializeField]private StructPrefabList structDataList;
    [SerializeField]private EditorUIController uiController;
    [SerializeField]private TerrainController terrainController;
    private bool overlapBox;
    #endregion

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = drawMat;
    }

    void Update()
    {
        FloorCheck();
        BuildBlock();

        TurnStruct();
        ChangeGridSize(dataManager.LoadBlockID());
        ChangeHeight();
    }

    #region TriggerCheck
    private void OnTriggerEnter(Collider other)
    {
        if (!isEraser && other.gameObject.CompareTag("Struct"))
        {
            isCanDraw = false;
            meshRenderer.material = disableMat;
            Debug.Log("disable!");
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (isEraser)
        {
            Debug.Log("Eraser?");
            meshRenderer.material = eraserMat;
        }
        if (isEraser && Input.GetMouseButton(0) && other.CompareTag("Struct"))
        {
            Destroy(other.gameObject);
            dataManager.RemoveStructList(other.transform.position);
            Debug.Log("Eraser!");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!isEraser && other.gameObject.CompareTag("Struct"))
        {
            meshRenderer.material = drawMat;
            isCanDraw = true;
            Debug.Log("Enable!");
        }
    }
    #endregion

    #region DefaultCheck&Build

    public void ChangeHeight()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            isDrawFloor = true;
            uiController.LoadButtonList(StructFilter.Floor);
            ChangeGridSize(25);
            yPos = -1f;
            
        }
        else if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            isDrawFloor = false;
            yPos = 0.5f;
            
        }
    }
    
    public void TurnStruct()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            transform.Rotate(0, 90, 0);
        }

        float rotationY = transform.eulerAngles.y;
        if (rotationY < 0) rotationY += 360;

        if (gridSize.x != gridSize.z)
        {
            isOffsetXZEdit = false;

            if (Mathf.Approximately(rotationY, 0) || Mathf.Approximately(rotationY, 180))
            {
                // 0도 및 180도에서의 오프셋
                viewerOffsetX = gridSize.x % 2 == 0 ? 0.5f : 0;
                viewerOffsetZ = gridSize.z % 2 == 0 ? 0.5f : 0;
            }
            else if (Mathf.Approximately(rotationY, 90) || Mathf.Approximately(rotationY, 270))
            {
                // 90도 및 270도에서의 오프셋
                viewerOffsetX = gridSize.z % 2 == 0 ? 0.5f : 0;
                viewerOffsetZ = gridSize.x % 2 == 0 ? 0.5f : 0;
            }
        }
        else
        {
            if (gridSize.x % 2 == 0)
            {
                viewerOffsetX = 0.5f;
            }
            else
            {
                viewerOffsetX = 0;
            }

            if (gridSize.z % 2 == 0)
            {
                viewerOffsetZ = 0.5f;
            }
            else
            {
                viewerOffsetZ = 0;
            }
        }
    }

    public void ChangeGridSize(int a)
    {
        Debug.Log($"NowGrid : {a}");
        if (!isDrawFloor && !isEraser) {

            nowNum = a;
            if (structDataList.structList[nowNum].mesh != null) GetComponent<MeshFilter>().mesh = structDataList.structList[nowNum].mesh; // 만약 Mesh가 비어있을
            else GetComponent<MeshFilter>().mesh = structDataList.defaultMesh;//디폴트로 변경!!
            viewerSize = structDataList.structList[nowNum].viewerSize;
            transform.localScale = viewerSize - new Vector3(0.2f, 0.2f, 0.2f);
            gridSize = structDataList.structList[nowNum].modelSize;

        }
        else if(isEraser && !isDrawFloor)
        {
            nowNum = a;
            GetComponent<MeshFilter>().mesh = structDataList.defaultMesh;
            viewerSize = new Vector3(1, 1, 1);
            transform.localScale = viewerSize - new Vector3(0.1f, 0.1f, 0.1f);
            gridSize = new Vector3(1, 1, 1);
        }
        else
        {
            nowNum = a;
            GetComponent<MeshFilter>().mesh = structDataList.defaultMesh;
            viewerSize = new Vector3(1, 1.3f, 1);
            transform.localScale = viewerSize - new Vector3(0.1f, 0.1f, 0.1f);
            gridSize = new Vector3(1, 1, 1);
        }
    }

    //지우개, 그리기 기능 선택 시 Material 변경 및 변수 조정
    public void DrawEraserChange(int isE)
    {
        if (isE == 0)
        {
            Debug.Log("is Edit");
            meshRenderer.material = drawMat;
            isCanDraw = true;
            isEraser = false;
        }
        else
        {
            Debug.Log("is Eraser");
            meshRenderer.material = eraserMat;
            isEraser = true;
        }
    }

    //Raycast로 마우스가 바라보고 있는 위치 확인
    public void FloorCheck()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); // 마우스 좌표에서 쏘는 ray

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Floor")))
        {
            Vector3 nextPos = hit.point + new Vector3(+0.5f, 0, +0.5f);

            if (new Vector3(Mathf.Round(nextPos.x / gridMove.x) * gridMove.x + viewerOffsetX, Mathf.Round(nextPos.y / gridMove.y) * gridMove.y + viewerOffsetY, Mathf.Round(nextPos.z / gridMove.z) * gridMove.z + viewerOffsetZ) != transform.position)
            {
                nextPos.y = yPos;
                transform.position = nextPos;
            }

        }
    }

    //Viewer 기본적인 Offset 조정 및 움직임, Struct 생성
    public void BuildBlock()
    {

        if (isOffsetXZEdit)
        {
            if (gridSize.x % 2 == 0 && transform.rotation.y % 90 == 0) viewerOffsetX = 0.5f;
            else viewerOffsetX = 0;
            if (gridSize.z % 2 == 0) viewerOffsetZ = 0.5f;
            else viewerOffsetZ = 0;
        }

        if (structDataList.structList[nowNum].isYOffset && !isEraser && !isDrawFloor) trueVec = new Vector3(Mathf.Floor(transform.position.x / gridMove.x) * gridMove.x + viewerOffsetX, Mathf.Floor(transform.position.y / gridMove.y) * gridMove.y - 0.5f, Mathf.Floor(transform.position.z / gridMove.z) * gridMove.z + viewerOffsetZ);
        else trueVec = new Vector3(Mathf.Floor(transform.position.x / gridMove.x) * gridMove.x + viewerOffsetX, Mathf.Floor(transform.position.y / gridMove.y) * gridMove.y, Mathf.Floor(transform.position.z / gridMove.z) * gridMove.z + viewerOffsetZ);

        transform.position = trueVec;

        Collider[] colliders = Physics.OverlapBox(trueVec, Vector3.one * 0.4f, Quaternion.identity);
        foreach(Collider collider in colliders)
        {
            if (collider.CompareTag("Struct"))
            {
                overlapBox = true;
                break;
            }
            else
            {
                overlapBox = false;
            }
        }
        if (!overlapBox)
        {
            if (Input.GetMouseButton(0) && !isDrawFloor)
            {
                if (!isEraser && isCanDraw)
                {
                    Debug.Log(nowNum + " is Build");

                    GameObject tempObj = Instantiate(structDataList.structList[nowNum].structPrefab, new Vector3(transform.position.x, transform.position.y, transform.position.z) + structDataList.structList[nowNum].modelPositionOffset, Quaternion.Euler(transform.rotation.eulerAngles + structDataList.structList[nowNum].modelRotateOffset), structsObj.transform);
                    Debug.Log(new Vector3(transform.rotation.x, transform.rotation.y, transform.rotation.z));
                    if(tempObj != null)
                    {
                        Debug.Log("Temp Obj가 비어있지 않습니다!");
                    }
                    tempObj.tag = "Struct";
                    dataManager.AddStructList(new Vector3(transform.position.x, transform.position.y, transform.position.z) + structDataList.structList[nowNum].modelPositionOffset, transform.rotation.eulerAngles + structDataList.structList[nowNum].modelRotateOffset, nowNum);

                };
            }
            if (Input.GetMouseButton(0) && isDrawFloor)
            {
                if (!isEraser && isCanDraw)
                {
                    Debug.Log(nowNum + " is Paint");

                    GameObject tempObj = Instantiate(structDataList.structList[nowNum].structPrefab, new Vector3(transform.position.x, transform.position.y, transform.position.z) + structDataList.structList[nowNum].modelPositionOffset, Quaternion.Euler(transform.rotation.eulerAngles + structDataList.structList[nowNum].modelRotateOffset), floorStructs.transform);
                    Debug.Log(new Vector3(transform.rotation.x, transform.rotation.y, transform.rotation.z));
                    terrainController.DelFloorObj((int)transform.position.x + 11, (int)transform.position.z + 4);
                    dataManager.AddFloorPaint(new Vector3(transform.position.x, transform.position.y, transform.position.z) + structDataList.structList[nowNum].modelPositionOffset, (int)transform.position.x + 11, (int)transform.position.z + 4, nowNum);

                };
            }
            if (Input.GetMouseButton(0) && isDrawFloor && isEraser)
            {
                Debug.Log(nowNum + " is Paint Eraser");
                Debug.Log(new Vector3(transform.rotation.x, transform.rotation.y, transform.rotation.z));
                RemoveAllPaint();
                dataManager.RemovePaintData((int)transform.position.x + 11, (int)transform.position.z + 4);
                terrainController.InitTerrainSizeWH();
            }
        }
        
    }
    #endregion

    #region SystemSet
    public void RemoveAllPaint()
    {
        for(int i = 0; i < floorStructs.transform.childCount; i++)
        {
            Destroy(floorStructs.transform.GetChild(i).gameObject);
        }
    }

    public void SetisEraser(bool IsEraser)
    {
        isEraser = IsEraser;
    }

    public void SetisCanDraw(bool isCD)
    {
        isCanDraw = isCD;
    }
    
    public bool returnEraser()
    {
        return isEraser;
    }

    public Material LoadMaterial(string mat)
    {
        if(mat == "Disable")
        {
            return disableMat;
        }
        else if(mat == "Draw")
        {
            return drawMat;
        }
        else
        {
            return eraserMat;
        }
    }
    #endregion

}
