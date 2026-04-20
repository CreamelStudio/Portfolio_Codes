using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class StageSelectManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject stageInfoBox;
    [SerializeField] private Image stageThumbnailImage;
    [SerializeField] private TextMeshProUGUI stageName;
    [SerializeField] private TextMeshProUGUI stageDescription;
    [SerializeField] private GameObject stageStar;
    [SerializeField] private TextMeshProUGUI scoreText_1;
    [SerializeField] private TextMeshProUGUI scoreText_2;
    [SerializeField] private TextMeshProUGUI scoreText_3;
    [SerializeField] private GameObject startBtn_Defualt;
    [SerializeField] private GameObject startBtn_Tutorial;

    private Animator stageBoxAnimator;
    private readonly int hashShow = Animator.StringToHash("Show");

    [SerializeField] Image currentStageImage;
    [SerializeField] TMP_Text currentStageText;

    [SerializeField] private Transform spawnPosition;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private Sprite buttonImage;

    private void Awake()
    {
        if (stageInfoBox != null)
        {
            stageBoxAnimator = stageInfoBox.GetComponent<Animator>();
        }


    }
    private void Start()
    {
        GameManager.instance.Photon_StageSelcet("MultiStage1");
        InitStageButton();
    }

    private void InitStageButton()
    {
        foreach(StageData stage in GameManager.instance.stageDataList)
        {
            GameObject temp = Instantiate(buttonPrefab, spawnPosition);
            ConfigureStageButton(temp, stage);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseStageInfo();
        }
    }

    public void SetStageInfo(StageData stageData)
    {
        GameManager.instance.isCustomMap = false;
        if (stageData == null)
        {
            return;
        }

        if (stageThumbnailImage != null)
        {
            stageThumbnailImage.sprite = stageData.sumnaThumbnailImage;
        }

        if (currentStageImage != null)
        {
            currentStageImage.sprite = stageData.stageImage;
        }

        if (stageName != null)
        {
            stageName.text = stageData.stageName;
        }

        if (currentStageText != null)
        {
            currentStageText.text = stageData.stageName;
        }

        if (stageDescription != null)
        {
            stageDescription.text = stageData.stageDescription;
        }

        GameManager.instance.StageSelcet(stageData);

        if (stageInfoBox != null && stageBoxAnimator != null)
        {
            stageInfoBox.SetActive(true);
            stageBoxAnimator.SetBool(hashShow, true);
        }

        if (scoreText_1 != null && scoreText_2 != null && scoreText_3 != null)
        {
            scoreText_1.text = stageData.starRequiredScore_1.ToString();
            scoreText_2.text = stageData.starRequiredScore_2.ToString();
            scoreText_3.text = stageData.starRequiredScore_3.ToString();
        }

        if(startBtn_Defualt != null && startBtn_Tutorial != null)
        {
            if(stageData.stageName == "Tutorial")
            {
                startBtn_Defualt.SetActive(false);
                startBtn_Tutorial.SetActive(true);
            }
            else
            {
                startBtn_Defualt.SetActive(true);
                startBtn_Tutorial.SetActive(false);
            }
        }
    }


    public void RPC_SetStageInfo(string stageName)
    {
        GameManager.instance.isCustomMap = false;
        GameManager.instance.Photon_StageSelcet(stageName);
    }
    public void CloseStageInfo()
    {
        if (stageBoxAnimator != null)
        {
            stageBoxAnimator.SetBool(hashShow, false);
        }
    }

    private void ConfigureStageButton(GameObject buttonObject, StageData stage)
    {
        Image[] images = buttonObject.GetComponentsInChildren<Image>(true);
        if (images.Length > 0)
        {
            images[0].sprite = buttonImage;
        }

        if (images.Length > 2)
        {
            images[2].sprite = stage.stageImage;
        }

        TextMeshProUGUI label = buttonObject.GetComponentInChildren<TextMeshProUGUI>(true);
        if (label != null)
        {
            label.text = stage.stageName;
        }

        Button button = buttonObject.GetComponent<Button>();
        if (button == null)
        {
            Debug.LogWarning($"스테이지 버튼에 Button 컴포넌트가 없습니다: {buttonObject.name}");
            return;
        }

        button.onClick.AddListener(() =>
        {
            SetStageInfo(stage);
            UIManager.Instance.ButtonSfx();
        });
    }


  
}
