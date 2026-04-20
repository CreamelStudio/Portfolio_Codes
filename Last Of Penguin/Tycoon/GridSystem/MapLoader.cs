using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MapLoader : MonoBehaviour
{
    [SerializeField]
    private StructPrefabList structData;
    private EditorData tempEditorData;

    [SerializeField]
    private GameObject floorCube;

    [SerializeField]
    private Material[] orangeMat;
    [SerializeField]
    private Material[] whiteMat;
    [SerializeField]
    private GameObject floorParent;
    private GameObject[][] floorObj = new GameObject[1][];

    [SerializeField]
    private Vector3 cameraOffset;

    [SerializeField]
    private string fileName;

    private Vector3 floorWH;

    private string path;

    private void Start()
    {
        LoadStructsJson(GameManager.instance.currentStageData.stageName);
    }

    public void LoadStructsJson(string fileName)
    {
        if (GameManager.instance.isCustomMap) path = Path.Combine(Application.dataPath, "CustomJson", fileName + ".json");
        if (!GameManager.instance.isCustomMap) path = Path.Combine("DevJson", fileName);

        if (File.Exists(path) || !GameManager.instance.isCustomMap)
        {
            string json;
            if (GameManager.instance.isCustomMap) json = File.ReadAllText(path);
            else json = Resources.Load<TextAsset>(path).ToString();

            tempEditorData = JsonUtility.FromJson<EditorData>(json);

            Debug.Log($"Load Map Path : {path} Editor Json Data : {json}");

            SetTerrainSizeWH(tempEditorData.terrainWidth, tempEditorData.terrainHeight);

            GameObject tempObj;
            for (int i = 0; i < tempEditorData.structsList.Count; i++)
            {
                tempObj = Instantiate(structData.structList[tempEditorData.structsList[i].blockCode].structPrefab, tempEditorData.structsList[i].location, Quaternion.Euler(tempEditorData.structsList[i].rotation), floorParent.transform);
            }
            for (int i = 0; i < tempEditorData.floorPaint.Count; i++)
            {
                tempObj = Instantiate(structData.structList[tempEditorData.floorPaint[i].blockCode].structPrefab, tempEditorData.floorPaint[i].position, Quaternion.Euler(0, 0, 0), floorParent.transform);
            }
        }
    }



    public void SetTerrainSizeWH(int width, int height)
    {
        floorWH = new Vector3(width, 0, height);
        //2�����迭�ʱ�ȭ
        floorObj = new GameObject[width][];
        for (int i = 0; i < width; i++)
        {
            floorObj[i] = new GameObject[height];
        }
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                if(GameManager.instance.gameMode == GameMode.normal)
                {
                    floorObj[j][i] = Instantiate(floorCube, new Vector3(j - 11, -1, i - 4), new Quaternion(0, 0, 0, 0), floorParent.transform);
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
                if(GameManager.instance.gameMode == GameMode.icelink)
                {
                    floorObj[j][i] = Instantiate(structData.structList[25].structPrefab, new Vector3(j - 11, -1, i - 4), new Quaternion(0, 0, 0, 0), floorParent.transform);
                }
                
            }
        }
        for (int k = 0; k < tempEditorData.floorPaint.Count; k++)
        {
            Destroy(floorObj[tempEditorData.floorPaint[k].gridx][tempEditorData.floorPaint[k].gridy]);
        }
        SetCameraMain();
    }

    public void SetCameraMain()
    {
        int center = Mathf.CeilToInt(floorObj.Length / 2);
        UnityEngine.Camera.main.transform.position = new Vector3(center, 0, 0) + cameraOffset;
    }
}