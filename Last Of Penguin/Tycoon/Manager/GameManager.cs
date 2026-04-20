using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class PlayerPenuinData
{
    public string penguinName = "�⺻";
    private string myPenguinName = "�⺻";
    public string penguinColor = "�⺻";
}

public enum GameMode
{
    normal = 0,
    icelink = 1
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("CurrentPlayerPenguin")]
    //public string penguinName = "�⺻";
    public CharacterProfile penguinData = null;
    private string myPenguinName = "�⺻";
    private CharacterProfile myPenguin;
    public string penguinColor = "�⺻";

    [Header("CurrentPlayerPenguin_Multi")]
    public List<PlayerPenuinData> multiPlayerPenguinData = new List<PlayerPenuinData>();

    [Header("CurrentStage")]
    //public string stageName;
    [SerializeField] public GameMode gameMode;
    [SerializeField] private List<StageData> stageDatas;
    public StageData currentStageData;

    [Header("CurrentMode")]
    public bool isMuilt;

    public SubmissionGacility submissiongacility;

    public List<Food> tempFood;
    public List<StageData> stageDataList = new List<StageData>();

    public int foodIndex;

    [Header("JoystickType")]
    public bool isKeyBord;
    public bool isXbox;
    public bool isPs;

    public Transform penguinSpawnPos;

    public bool isCustomMap;

    private void InitStage()
    {
        string[] jsonFiles = Directory.GetFiles(Application.dataPath + "/DevJson/", "*.json");

        foreach (string jsonFile in jsonFiles)
        {
            string json = File.ReadAllText(jsonFile);
            EditorData tempEditorData = JsonUtility.FromJson<EditorData>(json);

            if (tempEditorData.stageInfo.backGroundMusicSourcePath != null)
            {
                StageData tempStage = ScriptableObject.CreateInstance<StageData>();
                tempStage.stageName = tempEditorData.stageInfo.stageName.Split('.')[0];
                tempStage.stageDescription = tempEditorData.stageInfo.stageDescription;
                tempStage.stageTime = tempEditorData.stageInfo.stageTime;
                tempStage.starRequiredScore_1 = tempEditorData.stageInfo.starRequiredScore_1;
                tempStage.starRequiredScore_2 = tempEditorData.stageInfo.starRequiredScore_2;
                tempStage.starRequiredScore_3 = tempEditorData.stageInfo.starRequiredScore_3;

                tempStage.backGroundMusicSource = Resources.Load<AudioClip>(tempEditorData.stageInfo.backGroundMusicSourcePath);
                tempStage.stageImage = Resources.Load<Sprite>(tempEditorData.stageInfo.stageImagePath);

                tempStage.FoodList = new List<Food>();

                foreach (int foodList in tempEditorData.stageInfo.FoodList)
                {
                    tempStage.FoodList.Add(tempFood[foodList]);
                    Debug.Log(foodList);
                }

                stageDataList.Add(tempStage);
            }
        }
    }

    private void Awake()
    {
        gameMode = GameMode.normal;
        InitStage();
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ChangePenguin(CharacterProfile _penguinName) // �� �κп��� ���װ� ���ٸ� .dataPenguinName�� �߰�
    {
        if (myPenguin != penguinData) myPenguin = penguinData;
        penguinData = _penguinName;
    }

    public void UnDoPenguin()
    {
        penguinData = myPenguin;
    }

    public void StageSelcet(StageData stageData)
    {
        currentStageData = stageData;
    }

    public void Photon_StageSelcet(string stageData)
    {
        foreach (var s in stageDataList)
        {
            if (s.stageName == stageData)
            {
                currentStageData = s;
            }
        }
    }
}
