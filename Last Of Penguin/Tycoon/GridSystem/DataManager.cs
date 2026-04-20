using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum FoodEnum
{
    Strawberry = 0,
    choco = 1,
    vanila = 2
}

[Serializable]
public class StageDataJson
{
    public string stageName;
    public string stageDescription;
    public float stageTime;
    public float starRequiredScore_1;
    public float starRequiredScore_2;
    public float starRequiredScore_3;
    public string backGroundMusicSourcePath;
    public List<int> FoodList;
    public string stageImagePath;
}

//���� ������ Ŭ����
[Serializable]
public class StructData
{
    public Vector3 location;
    public Vector3 rotation;
    public int blockCode;
    public StructData(Vector3 location, Vector3 rotation, int blockCode)
    {
        this.location = location;
        this.rotation = rotation;
        this.blockCode = blockCode;
    }
}

//�ٴڿ� ĥ������ Ŭ����
[Serializable]
public class FloorPaint
{
    public Vector3 position;
    public int gridx;
    public int gridy;
    public int blockCode;

    public FloorPaint(Vector3 position, int gridx, int gridy, int blockCode)
    {
        this.position = position;
        this.gridx = gridx;
        this.gridy = gridy;
        this.blockCode = blockCode;
    }
}

//���� ������ Ŭ����
[Serializable]
public class EditorData
{
    public StageDataJson stageInfo;

    public List<StructData> structsList;
    public List<FloorPaint> floorPaint;

    public int minute;
    public int second;

    public int terrainWidth;
    public int terrainHeight;


}

public class DataManager : MonoBehaviour
{
    [Header("System")]
    [SerializeField] public static DataManager instance; 

    [Header("UI")]
    [SerializeField] private GameObject loadingPannel;

    [Header("System")]
    [SerializeField] public EditorData editorData;
    [SerializeField] private StructPrefabList structDataList;
    [SerializeField] private GameObject structObj;
    [SerializeField] private TerrainController terrainController;
    private int blockID;

    [SerializeField] private fileListManager fileList;
    [SerializeField] private CustomLoader customPlayLoader;

    private void Start()
    {
        if (instance == null) instance = this;
        else Destroy(instance);

        editorData = new EditorData();
        editorData.structsList = new List<StructData>();
        editorData.floorPaint = new List<FloorPaint>();
        editorData.terrainWidth = 14;
        editorData.terrainHeight = 14;
        blockID = 4;
    }

    public int LoadBlockID()
    {
        return blockID;
    }
    
    //���� ���� ���� ����
    public void SetBlockID(int BlockID)
    {
        blockID = BlockID;
    }

    public void AddStructList(Vector3 location, Vector3 rotation, int blockCode)
    {
        editorData.structsList.Add(new StructData(location, rotation, blockCode));
    }

    public void AddFloorPaint(Vector3 location, int gridx, int gridy, int blockCode)
    {
        editorData.floorPaint.Add(new FloorPaint(location, gridx, gridy, blockCode));
    }

    public void RemoveStructList(Vector3 location)
    {
        int tempIndex = editorData.structsList.FindIndex(structLocation => structLocation.location == location);
        if (tempIndex != -1) editorData.structsList.RemoveAt(tempIndex);
    }

    public void SetTerrainWHData(int width, int height)
    {
        editorData.terrainWidth = width;
        editorData.terrainHeight = height;
    }

    public void SetTimerData(int minute, int second)
    {
        editorData.minute = minute;
        editorData.second = second;
    }

    public void RemovePaintData(int gridx, int gridy)
    {
        editorData.floorPaint.RemoveAt(editorData.floorPaint.FindIndex(f => f.gridx == gridx && f.gridy == gridy));
    }

    //�����ݷ� �����鼭 �ϸ� DevJson���� ���� �ƴϸ� �Ϲ�����
    public void SaveJsonData(string fileName)
    {
        if (Input.GetKey(KeyCode.Semicolon))
        {
            string json = JsonUtility.ToJson(editorData, true);
            File.WriteAllText(Path.Combine(Application.dataPath, "DevJson", fileName + ".json"), json);
            Debug.Log("Save On " + Path.Combine(Application.dataPath, "DevJson", fileName + ".json"));
        }
        else
        {
            customPlayLoader.nowStage = fileName;
            string json = JsonUtility.ToJson(editorData, true);
            File.WriteAllText(Path.Combine(Application.dataPath, "CustomJson", fileName + ".json"), json);
            Debug.Log("Save On " + Path.Combine(Application.dataPath, "CustomJson", fileName + ".json"));
            
        }
    }

    public EditorData returnEditorData()
    {
        return editorData;
    }

    public void LoadStructsJson(string fileName, int dev)
    {
        loadingPannel.SetActive(true);
        if (Input.GetKey(KeyCode.Semicolon) && dev == 0 || dev == 2)
        {
            string path = Path.Combine(Application.dataPath, "DevJson", fileName + ".json");
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);


                for (int i = 0; i < structObj.transform.childCount; i++)
                {
                    Destroy(structObj.transform.GetChild(i).gameObject);
                }

                editorData = JsonUtility.FromJson<EditorData>(json);
                terrainController.SetTerrainSizeWH(editorData.terrainWidth, editorData.terrainHeight);

                for (int i = 0; i < editorData.structsList.Count; i++)
                {
                    GameObject tempObj = Instantiate(structDataList.structList[editorData.structsList[i].blockCode].structPrefab, editorData.structsList[i].location, Quaternion.Euler(editorData.structsList[i].rotation), structObj.transform);
                    tempObj.transform.tag = "Struct";
                }
                for (int i = 0; i < editorData.floorPaint.Count; i++)
                {
                    GameObject tempObj = Instantiate(structDataList.structList[editorData.floorPaint[i].blockCode].structPrefab, editorData.floorPaint[i].position, Quaternion.Euler(0, 0, 0), structObj.transform);
                }
            }
        }
        else if(dev == 1 || dev == 0)
        {
            string path = Path.Combine(Application.dataPath, "CustomJson", fileName + ".json");
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);


                for (int i = 0; i < structObj.transform.childCount; i++)
                {
                    Destroy(structObj.transform.GetChild(i).gameObject);
                }

                editorData = JsonUtility.FromJson<EditorData>(json);
                terrainController.SetTerrainSizeWH(editorData.terrainWidth, editorData.terrainHeight);

                for (int i = 0; i < editorData.structsList.Count; i++)
                {
                    GameObject tempObj = Instantiate(structDataList.structList[editorData.structsList[i].blockCode].structPrefab, editorData.structsList[i].location, Quaternion.Euler(editorData.structsList[i].rotation), structObj.transform);
                    tempObj.transform.tag = "Struct";
                }
                for (int i = 0; i < editorData.floorPaint.Count; i++)
                {
                    GameObject tempObj = Instantiate(structDataList.structList[editorData.floorPaint[i].blockCode].structPrefab, editorData.floorPaint[i].position, Quaternion.Euler(0, 0, 0), structObj.transform);
                }
            }
            loadingPannel.SetActive(false);
            StartCoroutine(loadB());
        }

        

    }

    IEnumerator loadB()
    {
        yield return new WaitForSeconds(0.5f);
        fileList.initField();
    }

    public void ResetEditorMap()
    {
        for (int i = 0; i < structObj.transform.childCount; i++)
        {
            Destroy(structObj.transform.GetChild(i).gameObject);
        }
    }
}
