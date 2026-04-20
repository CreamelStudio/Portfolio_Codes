using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;

    [Header("GameOverSocreBored")]
    [SerializeField] TMP_Text orderCompleteText;
    [SerializeField] TMP_Text orderMissText;

    [Header("MissionSocreBored")]
    [SerializeField] private TextMeshProUGUI missionStageName;
    [SerializeField] TMP_Text missionOrderCompleteText;
    [SerializeField] TMP_Text missionOrderMissText;
    [SerializeField] private List<GameObject> missionStar;

    [Header("Score")]
    [SerializeField] private TextMeshProUGUI stageName;
    [SerializeField] public float orderCompleteScore;
    [SerializeField] public float orderMissScore;
    [SerializeField] private List<float> starRequiredScore;


    void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        starRequiredScore[0] = GameManager.instance.currentStageData.starRequiredScore_1;
        starRequiredScore[1] = GameManager.instance.currentStageData.starRequiredScore_2;
        starRequiredScore[2] = GameManager.instance.currentStageData.starRequiredScore_3;

        stageName.text = GameManager.instance.currentStageData.stageName.ToUpper();
        missionStageName.text = GameManager.instance.currentStageData.stageName.ToUpper();
    }

    void Update()
    {
        orderCompleteText.text = orderCompleteScore.ToString();
        orderMissText.text = orderMissScore.ToString();
        missionOrderCompleteText.text = orderCompleteScore.ToString();
        missionOrderMissText.text = orderMissScore.ToString();

        UpdateMissionStars();
    }

    public IEnumerator Co_ScoreStart()
    {
        for (int i = 0; i < TimeManager.instance.starUI.Length; i++)
        {
            if (MoneyManager.instance.currentMoney >= starRequiredScore[i])
            {
                TimeManager.instance.starUI[i].SetActive(true);
                AudioEffectManager.instance.PlaySound(transform.position, 9, Random.Range(1f, 1.1f));
            }
            yield return new WaitForSecondsRealtime(0.5f);
        }

        Time.timeScale = 0;
    }

    public void UpdateMissionStars()
    {
        for (int i = 0; i < missionStar.Count; i++)
        {
            if (MoneyManager.instance.currentMoney >= starRequiredScore[i])
            {
                missionStar[i].SetActive(true);
            }
            else
            {
                missionStar[i].SetActive(false);
            }
        }
    }

}
