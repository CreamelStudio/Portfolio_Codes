using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "StageData", menuName = "StageDatas/StageData")]
public class StageData : ScriptableObject
{
    public Sprite sumnaThumbnailImage;
    public string stageName;
    public string stageDescription;
    public float stageTime;
    public float starRequiredScore_1;
    public float starRequiredScore_2;
    public float starRequiredScore_3;
    public AudioClip backGroundMusicSource;
    public List<Food> FoodList; // 음식 정보를 담는 리스트
    public Sprite stageImage;
}
