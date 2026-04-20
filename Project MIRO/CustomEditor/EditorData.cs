using UnityEngine;

[System.Serializable]
public class BlockData
{
    public int __BlockID;
    public Vector3 __BlockPosition;
    public Vector3 __BlockRotation;

    public BlockData(int blockID, Vector3 blockPosition, Vector3 blockRotation)
    {
        __BlockID = blockID;
        __BlockPosition = blockPosition;
        __BlockRotation = blockRotation;
    }
    public BlockData(int blockID, Vector3 blockPosition, Quaternion blockRotation)
    {
        __BlockID = blockID;
        __BlockPosition = blockPosition;
        __BlockRotation = blockRotation.eulerAngles;
    }
}

[System.Serializable]
public class BlockInfo
{
    public string blockName;
    public string blockDesc;
    public Texture2D blockImage;
    public GameObject prefab;

    public bool isEnable;
}

[System.Serializable]
public class EditorData
{
    public string _MapGuid;
    public string _UserGuid;
    public string _MapName;
    public string _MapDesc;
    public string _MapEditor;
    public float _Difficult;

    public bool isCleared;
    public Vector2 GridSize;

    public BlockData[] _Blocks;
}

