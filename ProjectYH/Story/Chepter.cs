using UnityEngine;
using UnityEngine.AddressableAssets;

[System.Serializable]
[CreateAssetMenu(fileName = "Chepter Data", menuName = "Scriptable Object/Chepter Data")]
public class Chepter : ScriptableObject
{
    public string chepterName;
    public StoryStage[] stages;
}