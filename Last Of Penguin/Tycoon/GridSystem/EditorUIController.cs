using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum StructFilter
{
    All = 0,
    Wall = 1,
    Supply = 2,
    Production = 3,
    Other = 4,
    Floor = 5,
}

public class EditorUIController : MonoBehaviour
{
    [Header("Settings")]
    public GameObject settingPanel;
    public Slider cameraSensSlider;
    public Slider moveSpeedSlider;
    public Slider runSpeedSlider;
    public Sprite runSliderImg;
    public Sprite normalSliderImg;

    [Header("Save")]
    public GameObject fileNamePanel;
    public InputField fileInput;
    [Header("System")]
    public DataManager dataManager;
    public ViewerController viewerController;
    public TerrainController terrainController;

    public GameObject buttonContentField;
    public GameObject buttonPrefab;

    public StructPrefabList structPrefabList;
    [Header("In Setting")]
    public Slider widthSlider;
    public Slider heightSlider;

    public TextMeshProUGUI sizeText;

    public StructFilter wb;

    public GameObject[] filterButtons;
    public GameObject terrainSet;
    public bool isTerrainUp;

    public Texture2D brushMouse;
    public Texture2D eraserMouse;
    public Texture2D defaultMouse;

    RaycastHit hit;
    bool isE;

    private void Awake()
    {
        LoadEditorSettings();
    }

    private void Start()
    {
        LoadButtonList(StructFilter.All); // if -1? 다불러오기
        Cursor.SetCursor(brushMouse, new Vector2(0, 0), CursorMode.Auto);
    }

    private void LoadEditorSettings()
    {
        if (PlayerPrefs.GetInt("EditorSettingSaving") == 1)
        {
            cameraSensSlider.value = PlayerPrefs.GetFloat("EditorSetCameraSens");
            moveSpeedSlider.value = PlayerPrefs.GetFloat("EditorSetMoveSpeed");
            runSpeedSlider.value = PlayerPrefs.GetFloat("EditorSetRunSpeed");
        }
        else
        {
            cameraSensSlider.value = 1.5f;
            moveSpeedSlider.value = 0.3f;
            runSpeedSlider.value = 0.5f;

            PlayerPrefs.SetFloat("EditorSetCameraSens", 1.5f);
            PlayerPrefs.SetFloat("EditorSetMoveSpeed", 0.3f);
            PlayerPrefs.SetFloat("EditorSetRunSpeed", 0.5f);

            PlayerPrefs.SetInt("EditorSettingSaving", 1);
        }
    }

    private void SaveEditorSettings()
    {
        PlayerPrefs.SetFloat("EditorSetCameraSens", cameraSensSlider.value);
        PlayerPrefs.SetFloat("EditorSetMoveSpeed", moveSpeedSlider.value);
        PlayerPrefs.SetFloat("EditorSetRunSpeed", runSpeedSlider.value);
    }

    public void EditSlider(int val)
    {
        switch (val)
        {
            case 0: cameraSensSlider.targetGraphic.gameObject.GetComponent<Image>().sprite = runSliderImg; break;
            case 1: moveSpeedSlider.targetGraphic.gameObject.GetComponent<Image>().sprite = runSliderImg; break;
            case 2: runSpeedSlider.targetGraphic.gameObject.GetComponent<Image>().sprite = runSliderImg; break;
        }
    }

    public void EndEditSlider(int val)
    {
        switch (val)
        {
            case 0: cameraSensSlider.targetGraphic.gameObject.GetComponent<Image>().sprite = normalSliderImg; break;
            case 1: moveSpeedSlider.targetGraphic.gameObject.GetComponent<Image>().sprite = normalSliderImg; break;
            case 2: runSpeedSlider.targetGraphic.gameObject.GetComponent<Image>().sprite = normalSliderImg; break;
        }
    }

    public void ShowSetting()
    {
        LoadEditorSettings();
        settingPanel.SetActive(true);
    }

    public void HideSetting()
    {
        SaveEditorSettings();
        settingPanel.SetActive(false);
    }

    private void Update()
    {
        UpdateCursorOnUI();
    }

    public void enablePanel()
    {
        SetTerrainWH();
        sizeText.text = widthSlider.value + " X " + heightSlider.value;
        fileNamePanel.SetActive(true);
    }

    public void panelLoad(int dev)
    {
        fileNamePanel.SetActive(false);
        dataManager.LoadStructsJson(fileInput.text, dev);
        fileInput.text = "";
    }

    public void panelSave()
    {
        fileNamePanel.SetActive(false);
        dataManager.SaveJsonData(fileInput.text);
        fileInput.text = "";
    }

    public void panelReturn()
    {
        fileNamePanel.SetActive(false);
        fileInput.text = "";
    }

    public void OnClickBlockSelect(int BlockID)
    {
        dataManager.SetBlockID(BlockID);
        Debug.Log(BlockID + " is Select");
        viewerController.SetisCanDraw(true);
        if(!viewerController.returnEraser()) viewerController.GetComponent<MeshRenderer>().material = viewerController.LoadMaterial("Draw"); 
    }

    public void SetDrawEraser(bool isEraser) {
        
        if(isEraser) Cursor.SetCursor(eraserMouse, new Vector2(0, 0), CursorMode.Auto);
        if(!isEraser) Cursor.SetCursor(brushMouse, new Vector2(30, 100), CursorMode.Auto);
        viewerController.SetisEraser(isEraser);
        isE = isEraser;
    }

    public void LoadButtonList(StructFilter wb)
    {
        for (int i = 0; i < buttonContentField.transform.childCount; i++) Destroy(buttonContentField.transform.GetChild(i).gameObject);
        for (int i = 0; i < structPrefabList.structList.Length; i++)
        {
            if(wb == structPrefabList.structList[i].wb || wb == StructFilter.All)
            {
                if (!structPrefabList.structList[i].disableButton)
                {
                    int index = i; 
                    Button TempObj = Instantiate(buttonPrefab, buttonContentField.transform).GetComponent<Button>();
                    TempObj.GetComponentInChildren<TextMeshProUGUI>().text = structPrefabList.structList[i].name;
                    TempObj.onClick.AddListener(() => OnClickBlockSelect(index));
                    if (structPrefabList.structList[i].image == null) TempObj.GetComponentInChildren<RawImage>().gameObject.SetActive(false);
                    else TempObj.GetComponentInChildren<RawImage>().texture = structPrefabList.structList[i].image;
                    Debug.Log(i + " is Ready!");
                }
            }
        }
    }

    public void selWb(int temp)
    {
        wb = (StructFilter)temp;
        LoadButtonList(wb);
        for(int i=0;i<5; i++)
        {
            if(wb != StructFilter.All)
            {
                if (Convert.ToInt32(wb) == i) filterButtons[i].GetComponent<RectTransform>().localPosition = new Vector3(filterButtons[i].GetComponent<RectTransform>().localPosition.x, -361.5f, filterButtons[i].GetComponent<RectTransform>().localPosition.z);
                else filterButtons[i].GetComponent<RectTransform>().localPosition = new Vector3(filterButtons[i].GetComponent<RectTransform>().localPosition.x, -379.7f, filterButtons[i].GetComponent<RectTransform>().localPosition.z);
            }
            else filterButtons[i].GetComponent<RectTransform>().localPosition = new Vector3(filterButtons[i].GetComponent<RectTransform>().localPosition.x, -379.7f, filterButtons[i].GetComponent<RectTransform>().localPosition.z);
        }
        if(wb == StructFilter.All) filterButtons[0].GetComponent<RectTransform>().localPosition = new Vector3(filterButtons[0].GetComponent<RectTransform>().localPosition.x, -361.5f, filterButtons[0].GetComponent<RectTransform>().localPosition.z);
    }

    public void ViewUp()
    {
        if (!isTerrainUp)
        {
            terrainSet.GetComponent<Animator>().SetBool("ToUp", true);
            isTerrainUp = true;
        }
        else
        {
            terrainSet.GetComponent<Animator>().SetBool("ToUp", false);
            isTerrainUp = false;
        }
        
    }

    public void SetTerrainWH()
    {
        widthSlider.value = Mathf.Round(widthSlider.value);
        heightSlider.value = Mathf.Round(heightSlider.value);
        terrainController.SetTerrainSizeWH((int)widthSlider.value, (int)heightSlider.value);
        sizeText.text = widthSlider.value + " X " + heightSlider.value;
    }

    public void UpdateCursorOnUI()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); // 마우스 좌표에서 쏘는 ray

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Floor")))
        {
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) Cursor.SetCursor(defaultMouse, Vector2.zero, CursorMode.Auto);
            else
            {
                if(isE) Cursor.SetCursor(eraserMouse, new Vector2(0,0), CursorMode.Auto);
                if(!isE) Cursor.SetCursor(brushMouse, new Vector2(30, 100), CursorMode.Auto);
            }

        }
        else Cursor.SetCursor(defaultMouse, Vector2.zero, CursorMode.Auto);
    }
}