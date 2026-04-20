using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TerrainController : MonoBehaviour
{
    [SerializeField]
    private DataManager dataManager;
    [SerializeField]
    private StructPrefabList listSt;

    public GameObject floorCube;

    public Material[] orangeMat;
    public Material[] whiteMat;

    public Vector3 firstOffset;
    public Vector3 floorWH;
    public GameObject floorParent;
    public GameObject newFloor;

    public GameObject[][] floorObj = new GameObject[1][];

    private void Start()
    {
        floorObj[0] = new GameObject[1];
        SetTerrainSizeWH(14, 14);
    }

    public void InitTerrainSizeWH()
    {
        int width = dataManager.returnEditorData().terrainWidth;
        int height = dataManager.returnEditorData().terrainHeight;

        floorWH = new Vector3(width, 0, height);

        floorObj = new GameObject[width][];
        for (int i = 0; i < width; i++)
        {
            floorObj[i] = new GameObject[height];
        }
        Debug.Log("floor Obj Gridx Lengh : " + floorObj.Length + "floor Obj Gridy Lengh : " + floorObj[0].Length);

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                floorObj[j][i] = Instantiate(floorCube, firstOffset + new Vector3(j, -1, i), new Quaternion(0, 0, 0, 0), floorParent.transform);
                if ((i + j) % 2 == 0)
                {
                    floorObj[j][i].GetComponent<MeshRenderer>().material = orangeMat[Random.Range(0, orangeMat.Length)];
                    floorObj[j][i].name = (width * i + j).ToString();
                }
                if ((i + j) % 2 == 1)
                {
                    floorObj[j][i].GetComponent<MeshRenderer>().material = whiteMat[Random.Range(0, whiteMat.Length)];
                    floorObj[j][i].name = (width * i + j).ToString();
                }

            }
        }
        for (int k = 0; k < dataManager.returnEditorData().floorPaint.Count; k++)
        {
            DelFloorObj(dataManager.returnEditorData().floorPaint[k].gridx, dataManager.returnEditorData().floorPaint[k].gridy);
        }

        for (int k = 0; k < dataManager.returnEditorData().floorPaint.Count; k++)
        {
            Instantiate(listSt.structList[dataManager.returnEditorData().floorPaint[k].blockCode].structPrefab, dataManager.returnEditorData().floorPaint[k].position + listSt.structList[dataManager.returnEditorData().floorPaint[k].blockCode].modelPositionOffset, Quaternion.Euler(transform.rotation.eulerAngles + listSt.structList[dataManager.returnEditorData().floorPaint[k].blockCode].modelRotateOffset), newFloor.transform);
        }
    }

    public void SetTerrainSizeWH(int width, int height)
    {
        floorWH = new Vector3(width, 0, height);
        dataManager.SetTerrainWHData(width, height);
        for (int i = 0; i < floorObj.Length; i++)
        {
            for (int j = 0; j < floorObj[i].Length; j++)
            {
                Destroy(floorObj[i][j]);
            }
        }
        //2차원배열초기화
        floorObj = new GameObject[width][];
        for (int i = 0; i < width; i++)
        {
            floorObj[i] = new GameObject[height];
        }


        for (int i = 0; i < height; i++)
        {
            for(int j = 0; j < width; j++)
            {
                floorObj[j][i] = Instantiate(floorCube, firstOffset + new Vector3(j, -1, i), new Quaternion(0, 0, 0, 0), floorParent.transform);
                if ((i + j) % 2 == 0)
                {
                    floorObj[j][i].GetComponent<MeshRenderer>().material = orangeMat[Random.Range(0, orangeMat.Length)];
                    floorObj[j][i].name = (width * i + j).ToString();
                }
                if ((i + j) % 2 == 1)
                {
                    floorObj[j][i].GetComponent<MeshRenderer>().material = whiteMat[Random.Range(0, whiteMat.Length)];
                    floorObj[j][i].name = (width * i + j).ToString();
                }
                
            }
        }
        for (int k = 0; k < dataManager.returnEditorData().floorPaint.Count; k++)
        {
            DelFloorObj(dataManager.returnEditorData().floorPaint[k].gridx, dataManager.returnEditorData().floorPaint[k].gridy);
        }
    }

    public void DelFloorObj(int gridx, int gridy)
    {
        Debug.Log("Grid x : " + gridx + " | " + "Grid y : " + gridy);
        Debug.Log("floor Obj Gridx Lengh : " + floorObj.Length + "floor Obj Gridy Lengh : " + floorObj[0].Length);
        Destroy(floorObj[gridx][gridy].gameObject);
    }

    public Vector3 ReturnTerrainSize()
    {
        return floorWH;
    }
}
