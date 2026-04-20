using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor;
using TMPro;
public class CustomLoader : MonoBehaviour
{
    //�ӽ� Ŀ���� ��������
    StageData customStage;

    //���� ����Ʈ
    public List<Food> FoodList;

    //�뷡 ����
    public AudioClip ac;
    
    //��۷� ���̽�ũ�� ����
    public Toggle choco;
    public Toggle straw;
    public Toggle vanila;

    //InputText
    public TMP_InputField oneStar;
    public TMP_InputField twoStar;
    public TMP_InputField threeStar;

    //�÷���Ÿ��
    public Dropdown time;

    //Ŀ���� �÷��� �г�
    public GameObject onPannel;
    public GameObject OnlyUseInEditorPannel;
    //���� �������� String
    public string nowStage;

    private void Start()
    {
        DontDestroyOnLoad(this);
    }

    public void OnGamePannel()
    {
        onPannel.SetActive(true);
    }

    public void OffGamePannel()
    {
        onPannel.SetActive(false);
    }

    public void SavePlay()
    {
        //Stage Data Instance ����

        DataManager.instance.editorData.stageInfo.stageName = nowStage;
        DataManager.instance.editorData.stageInfo.stageDescription = nowStage + "��������";
        if (choco.isOn) DataManager.instance.editorData.stageInfo.FoodList.Add((int)FoodEnum.choco);
        if (straw.isOn) DataManager.instance.editorData.stageInfo.FoodList.Add((int)FoodEnum.Strawberry);
        if (vanila.isOn) DataManager.instance.editorData.stageInfo.FoodList.Add((int)FoodEnum.vanila);

        int randomBGM = Random.Range(0, 1);
        if (randomBGM == 0) DataManager.instance.editorData.stageInfo.backGroundMusicSourcePath = "Music/Tycoon Groove";
        if (randomBGM == 1) DataManager.instance.editorData.stageInfo.backGroundMusicSourcePath = "Music/Tycoon Groove 2";

        DataManager.instance.editorData.stageInfo.starRequiredScore_3 = int.Parse(threeStar.text);
        Debug.Log(threeStar.text);
        DataManager.instance.editorData.stageInfo.starRequiredScore_2 = int.Parse(twoStar.text);
        DataManager.instance.editorData.stageInfo.starRequiredScore_1 = int.Parse(oneStar.text);

        if (time.value == 0) DataManager.instance.editorData.stageInfo.stageTime = 180;
        if (time.value == 1) DataManager.instance.editorData.stageInfo.stageTime = 120;
        if (time.value == 2) DataManager.instance.editorData.stageInfo.stageTime = 60;

        DataManager.instance.SaveJsonData(nowStage.Split('.')[0]);
    }

    public void OffPannel()
    {
        OnlyUseInEditorPannel.SetActive(false);
    }

}
