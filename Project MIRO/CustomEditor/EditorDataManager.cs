using BackEnd;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

/*public class BlockData
{
    public int __BlockID;
    public Vector3 __BlockPosition;
    public Vector3 __BlockRotation;
}

public class EditorData
{
    public string _Guid;
    public string _MapName;
    public string _MapDesc;
    public string _MapEditor;

    public float _Difficult;

    public BlockData[] _Blocks;
}*/


public class EditorDataManager : MonoBehaviour
{
    public static EditorDataManager instance;

    public BlockInfoList blockInfos;

    public EditorData currentData;
    public List<BlockData> currentTempBlockData;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this.gameObject);
    }

    public void CreateMapData(string mapName, string mapDesc, string mapEditor)
    {
        currentData = new EditorData();

        currentData._MapName = mapName;
        currentData._MapDesc = mapDesc;
        currentData._MapEditor = mapEditor;

        currentData._UserGuid = Backend.BMember.GetUserInfo().ToString();
        Debug.Log($"Map Create! GUID : {currentData._UserGuid}");
    }

    public void GridSizeChange(int width, int height)
    {
        EditorCanvasChanger.instance.ChangeCanvasSize(width, height);
        currentData.GridSize = new Vector2(width, height);
    }

    public void AddBlockData(int blockID, Vector3 position, Quaternion rotation)
    {
        currentTempBlockData.Add(new BlockData(blockID, position, rotation));
    }

    public void RemoveBlockData(Vector3 position)
    {
        currentTempBlockData.RemoveAt(currentTempBlockData.FindIndex(x => x.__BlockPosition == position));
    }

    public void SaveEditData(string mapName)
    {
        //TODO : currentData Json으로 저장하는 기능 구현
        currentData._Blocks = currentTempBlockData.ToArray();
        var match = Regex.Match(Backend.BMember.GetUserInfo().ToString(), "\"gamerId\":\"([^\"]+)\"");
        if (match.Success)
        {
            string guid = match.Groups[1].Value;
            Console.WriteLine(guid);
        }
        currentData._UserGuid = match.Groups[1].Value;
        currentData._MapName = mapName;
        if(string.IsNullOrEmpty(currentData._MapGuid)) currentData._MapGuid = CreateGuid();
        string json = JsonUtility.ToJson(currentData, true);
        string savePath = Application.dataPath + "/MapDatas/CustomMap" + "/" + currentData._MapGuid + ".json";
        System.IO.File.WriteAllText(savePath, json);

        Debug.Log($"Map saved! Path: {savePath}");
    }

    public void LoadEditData()
    {
        //TODO : CurrentData를 불러와 데이터를 정리하고, Struct Manager에서 블럭들과 그리드 사이즈를 초기화시키는 기능 구현
    }

    public static string CreateGuid()
    {
        return Guid.NewGuid().ToString();
    }

}
