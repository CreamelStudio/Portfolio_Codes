using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[System.Serializable]
public class MusicPathObj
{
    public string MusicName;
    public string MainComposer;

    [Header("Resource")]
    public AssetReference MusicVideoPath;
    public AssetReference MusicAlbumCoverPath;
    public float MusicHighlight;
    public AssetReference MusicNormalPath;
    public AssetReference MusicHardPath;
    public AssetReference MusicMaxPath;
    public AssetReference MusicYHPath;

    public float DifficultNormal;
    public float DifficultHard;
    public float DifficultMax;
    public float DifficultYH;
}

[CreateAssetMenu(fileName = "MusicPathList", menuName = "Scriptable Object/MusicPathList")]
public class MusicPathList : ScriptableObject
{
    public List<MusicPathObj> pathList;
}