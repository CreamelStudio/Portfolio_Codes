using UnityEngine;
using UnityEngine.AddressableAssets;

[System.Serializable]
public enum CharacterEnum
{
    None,
    루미아,
    미래,
    네아
}

[System.Serializable]
public class  CharacterViewManager
{
    public Sprite charaterImage;
    public CharacterEnum character;

    [Range(0, 1)]public float positionX = 0.5f;
    [Range(0, 1)]public float positionY = 0.5f;

    [Range(0, 3)] public float Scale = 1f;
}

[System.Serializable]
public class Dialog
{
    public string Speaker;
    public string content;
    public float textOffset;

    public Texture backgroundImage;
    public CharacterViewManager[] characters;

    public bool isNotChat;

    [Header("Stage Information"), Space(10f)]
    public bool isToGame;
    public string stageName;
    public string composerName;

    public AssetReference difficult1;
    public AssetReference difficult2;
    public AssetReference difficult3;
    public AssetReference difficult4;
}

[System.Serializable]
[CreateAssetMenu(fileName = "Story Stage Data", menuName = "Scriptable Object/Story Stage Data")]
public class StoryStage : ScriptableObject
{
    public string stageName;
    public string stageDescription;

    public Dialog[] dialogs;
}