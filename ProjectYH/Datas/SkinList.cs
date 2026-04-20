using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class skinList
{
    public string skinName;
    public GameObject skinPrefab;
    public Sprite skinTexture;
}

[CreateAssetMenu(fileName = "Skin List Manager", menuName = "Scriptable Object/Skin List Manager")]
public class SkinList : ScriptableObject
{
    public skinList[] skin;
}
