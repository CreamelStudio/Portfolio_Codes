using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ManagerNameRole
{
    Role,
    Name
}

[System.Serializable]
public class LevelData
{
    public string musicName;
    public List<ManagerList> managers = new List<ManagerList>();

    [Header("Music Data")]
    public string guidPath;
    public string bgaPath;
    public float musicLength;
    public int difficult;

    [Header("Detail Music Data")]
    public float bpm;
    public float bitDetail;
    
    [Header("Default")]
    public bool isEnableMinute;
    public float defaultMinuteSpeed;

    [Header("Effect")]
    public List<BaseEffect> Effects = new List<BaseEffect>();

    [Header("Note Minute")]
    public List<BaseNote> MinuteNotes = new List<BaseNote>();
}

[System.Serializable]
public class ManagerList
{
    public ManagerList(string Role, string Name)
    {
        this.Role = Role;
        this.Name = Name;
    }

    public string Role;
    public string Name;
}