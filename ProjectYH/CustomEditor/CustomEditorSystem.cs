using FMOD.Studio;
using FMODUnity;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.Rendering.VolumeComponent;

public class CustomEditorSystem : MonoBehaviour
{

    public EventReference FmodmusicData;
    public EventInstance FmodmusicInstance;

    public LevelData editLevel;
    public int Enablepannel;

    [Header("Func & Effect")]
    public GameObject downBar;
    public GameObject sideBar;
    public GameObject circleViewPrefab;
    public GameObject[] sideFunc;

    [Header("System Value")]
    public string filePath;
    [Space(4f)]
    public bool isPlaying;
    public float verticalPosition;
    [Space(3f)]
    public int nowSelectFunc;
    public int nowSelectButton;
    public int nowSelectOffset;
    public int bitCount;
    public double bitOffset;
    [Space(3f)]
    [Header("Scroll View Object")]
    public ScrollRect objectViewRect;
    public GameObject buttonPrefab;
    public List<GameObject> bitObjects;
    public List<Button> bitButtons;
    public Transform buttonSpawnPosition;
    public Transform scrollViewDownBar;

    [Space(10f)]
    [Header("SideBar - musicData")]
    public GameObject musicData;
    public TMP_InputField musicGuid_Input;
    public TMP_InputField bpm_Input;
    public Slider bitDetail_Input;
    public TMP_InputField musicLengthMinute_Input;
    public TMP_InputField musicLengthSecond_Input;
    public TMP_InputField jsonLoadName_Input;
    [Space(3f)]
    public TMP_Text musicGuid_Output;
    public TMP_Text bpm_Output;
    public TMP_Text bitDetail_Output;
    public TMP_Text musicLength_Output;

    [Space(10f)]
    [Header("SideBar - editingData")]
    public GameObject editingData;
    public Slider playSpeed_Input;
    public Slider buttonScale_Input;
    public TMP_InputField minuteDefaultTurnSpeed_Input;
    public TMP_InputField jsonSaveName1_Input;
    [Space(3f)]
    public TMP_Text playSpeed_Output;
    public TMP_Text buttonScale_Output;
    public TMP_Text minuteDefaultTurnSpeed_Output;

    [Space(10f)]
    [Header("SideBar - saveData")]
    public GameObject saveData;
    public TMP_Text musicName_Output;
    [Space(3f)]
    public GameObject producerTempletePrefab;
    public GameObject producerSpawnPosition;
    public List<GameObject> producerManagerList;
    public TMP_InputField musicName_Input;
    public List<TMP_InputField> producerRole_Inputs;
    public List<TMP_InputField> producerName_Inputs;
    public TMP_InputField jsonSaveName2_Input;

    [Space(10f)]
    public NoteWSpeedUI noteAndspeedUI;
    public PostProcessUI postprocessUI;
    public ColorAdjustmentUI colorAdjUI;
    public CameraMoveUI cameraMoveUI;

    public NoteClick inputActions;
    public bool isScroll = false;

    public bool isDownBar;
    public bool isSideBar;

    private void Awake()
    {
        inputActions = new NoteClick();
        inputActions.Enable();

        InputSystem.pollingFrequency = 1000f;

        inputActions.NoteClickMap.NoteClick1.performed += ctx => {
            OnKeyInput();
        };
        inputActions.NoteClickMap.NoteClick2.performed += ctx => {
            OnKeyInput();
        };
        inputActions.NoteClickMap.NoteClick3.performed += ctx => {
            OnKeyInput();
        };
        inputActions.NoteClickMap.NoteClick4.performed += ctx => {
            OnKeyInput();
        };
    }
    void Start()
    {
        editLevel = new LevelData();
        musicData.SetActive(true);
        editingData.SetActive(false);
        saveData.SetActive(false);
        Debug.Log(typeof(DefaultNote).Assembly.FullName);
    }

    private void Update()
    {
        InitScrollRect();
        ExitKeyInput();
        ControllKey();
        KeyInputFunc();
    }

    private void LateUpdate()
    {
        InitBitSlider();
    }

    #region JsonData
    public void SaveLevelData(string FileName)
    {
        var settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            Formatting = Formatting.Indented
        };

        editLevel.MinuteNotes.Sort((a, b) => a.StartDelay.CompareTo(b.StartDelay));
        editLevel.Effects.Sort((a, b) => a.StartDelay.CompareTo(b.StartDelay));
        string json = JsonConvert.SerializeObject(editLevel, settings);
        File.WriteAllText(Path.Combine(Application.dataPath, $"{FileName}.yh"), json);
    }

    public void LoadLevelData(string FileName)
    {
        if (!File.Exists(Path.Combine(Application.dataPath, $"{FileName}.yh")))
        {
            Debug.Log("파일이 없습니다");
            return;
        }

        string json = File.ReadAllText(Path.Combine(Application.dataPath, $"{FileName}.yh"));

        var settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        };

        editLevel = JsonConvert.DeserializeObject<LevelData>(json, settings);

        musicGuid_Input.text = editLevel.guidPath;
        bpm_Input.text = editLevel.bpm.ToString();
        bitDetail_Input.value = editLevel.bitDetail;
        musicLengthMinute_Input.text = Mathf.RoundToInt(editLevel.musicLength / 60).ToString();
        musicLengthSecond_Input.text = Mathf.RoundToInt(editLevel.musicLength % 60).ToString();

        Do_SpawnButton(false);

        minuteDefaultTurnSpeed_Input.text = editLevel.defaultMinuteSpeed.ToString();

        for(int i = 0; i < 4; i++) InitMusicDataUI(i);
        for(int i = 0; i < 3; i++) InitEditDataUI(i);

        foreach (BaseNote note in editLevel.MinuteNotes)
        {
            int selBlock = Mathf.RoundToInt((float)note.StartDelay / (float)bitOffset);
            Instantiate(circleViewPrefab, bitObjects[selBlock].transform).GetComponent<EditorCircleView>().InitText(note.NoteType.ToString(), 8.5f);
        }
        foreach (BaseEffect effect in editLevel.Effects)
        {
            int selBlock = Mathf.RoundToInt((float)effect.StartDelay / (float)bitOffset);
            Instantiate(circleViewPrefab, bitObjects[selBlock].transform).GetComponent<EditorCircleView>().InitText(effect.EffectType.ToString(), 8.5f);
        }

        for (int i = 0; i < editLevel.managers.Count; i++)
        {
            DrawProducerUI(i);
            SetProducerInputValues(producerManagerList[i], editLevel.managers[i]);
        }
    }
    #endregion

    #region Music
    public void OnKeyInput()
    {
        if (isScroll)
        {
            FmodmusicInstance.getTimelinePosition(out int position);
            float playdsp = ((float)position / 1000f);
            int beatIndex = Mathf.RoundToInt(playdsp / (float)bitOffset);
            float snappedTime = beatIndex * (float)bitOffset;

            editLevel.MinuteNotes.Add(new DefaultNote(snappedTime));
            StartCoroutine(Co_SpawnNote(beatIndex));
        }
    }

    IEnumerator Co_SpawnNote(int selBlock)
    {
        yield return null;
        Instantiate(circleViewPrefab, bitObjects[selBlock].transform).GetComponent<EditorCircleView>().InitText("Note", 16.5f);
    }

    public void InitScrollRect()
    {
        FmodmusicInstance.getTimelinePosition(out int position);

        if (isPlaying && FmodmusicInstance.isValid() && verticalPosition < (float)position / editLevel.musicLength / 1000f)
        {
            FmodmusicInstance.setPitch(playSpeed_Input.value);
            float currentTime = position/1000f;
            float latencyCompensation = 0; // 실험적으로 조정 가능
            float normalizedPos = (currentTime + latencyCompensation) / editLevel.musicLength;
            objectViewRect.verticalNormalizedPosition = Mathf.Clamp01(normalizedPos);
            isScroll = true;
        }
        else
        {
            isScroll = false;
        }
    }

    public void StartMusic()
    {
        verticalPosition = objectViewRect.verticalNormalizedPosition;
        FmodmusicInstance.setPitch(12f);
        isPlaying = true;
        FmodmusicInstance.start();
    }

    public void StopMusic()
    {
        isPlaying = false;
        FmodmusicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
    }
    #endregion

    #region InitUI
    public void InitBitSlider()
    {
        bitDetail_Input.value = MathF.Round(bitDetail_Input.value);
        playSpeed_Input.value = MathF.Round(playSpeed_Input.value, 1);
        buttonScale_Input.value = MathF.Round(buttonScale_Input.value, 1);
    }

    public void InitMusicDataUI(int val)
    {
        switch (val)
        {
            case 0:
                {
                    FmodmusicData.Guid = new FMOD.GUID(new System.Guid(musicGuid_Input.text));
                    if (FmodmusicInstance.isValid())
                    {
                        FmodmusicInstance.release();
                        FmodmusicInstance.clearHandle();
                    }
                    FmodmusicInstance = RuntimeManager.CreateInstance(FmodmusicData);
                    musicGuid_Output.text = FmodmusicData.ToString();
                    editLevel.guidPath = musicGuid_Input.text;
                    break;
                }
            case 1:
                {
                    bpm_Output.text = float.Parse(bpm_Input.text).ToString() + "BPM";
                    editLevel.bpm = float.Parse(bpm_Input.text);
                    break;
                }
            case 2:
                {
                    bitDetail_Output.text = bitDetail_Input.value.ToString() + "Bit";
                    editLevel.bitDetail = bitDetail_Input.value;
                    break;
                }
            case 3:
                {
                    if (!string.IsNullOrEmpty(musicLengthMinute_Input.text) && !string.IsNullOrEmpty(musicLengthSecond_Input.text))
                    {
                        musicLength_Output.text = ((float.Parse(musicLengthMinute_Input.text) * 60) + float.Parse(musicLengthSecond_Input.text)).ToString() + "Sec";
                        editLevel.musicLength = (float.Parse(musicLengthMinute_Input.text) * 60f) + float.Parse(musicLengthSecond_Input.text);
                    }
                    break;
                }
        }
    }

    public void InitEditDataUI(int val)
    {
        switch (val)
        {
            case 0:
                {
                    playSpeed_Output.text = playSpeed_Input.value.ToString() + "X";
                    break;
                }
            case 1:
                {
                    foreach (GameObject obj in bitObjects)
                    {
                        obj.GetComponent<RectTransform>().sizeDelta = new Vector2(430, 100 * buttonScale_Input.value);
                        obj.transform.localScale = new Vector3(1, buttonScale_Input.value, 1);
                    }
                    buttonScale_Output.text = buttonScale_Input.value.ToString() + "X";
                    break;
                }
            case 2:
                {
                    editLevel.defaultMinuteSpeed = float.Parse(minuteDefaultTurnSpeed_Input.text);
                    minuteDefaultTurnSpeed_Output.text = minuteDefaultTurnSpeed_Input.text;
                    break;
                }
        }
    }

    public void InitSaveData()
    {
        musicName_Output.text = musicName_Input.text;
    }

    public void JsonFileLoadSave(int val)
    {
        switch (val)
        {
            case 0:
                {
                    LoadLevelData(jsonLoadName_Input.text);
                    break;
                }
            case 1:
                {
                    SaveLevelData(jsonSaveName1_Input.text);
                    break;
                }
            case 2:
                {
                    SaveLevelData(jsonSaveName2_Input.text);
                    break;
                }
        }
    }
    #endregion

    #region InitButton
    public void ResetList()
    {
        editLevel.Effects.Clear();
        editLevel.MinuteNotes.Clear();

        foreach (GameObject button in bitObjects)
        {
            Destroy(button);
        }
    }

    public void CheckResetButton()
    {
        PopUpManager.instance.PopupShow("경고!", "모두 초기화 하시겠습니까?", OnClickConfirmButton: () => Do_SpawnButton(true), OnClickCancelButton: () => Do_CancelRespawn());
    }

    public void Do_CancelRespawn()
    {
        Debug.Log("Cancel Reset");
    }

    public void Do_SpawnButton(bool isReset)
    {
        if(isReset) ResetList();

        float musicMinutes = (float)editLevel.musicLength / 60.0f;
        bitCount = (int)(musicMinutes * editLevel.bpm * editLevel.bitDetail);
        bitOffset = (float)editLevel.musicLength / (float)bitCount;

        for (int i = bitCount - 2; i >= 1; i--)
        {
            GameObject temp = Instantiate(buttonPrefab, buttonSpawnPosition);
            bitObjects.Add(temp);
            bitButtons.Add(temp.GetComponent<Button>());

            if (i % 4 == 0) temp.GetComponent<Shadow>().effectColor = new Color(0, 1, 0);
            if (i % editLevel.bitDetail == 0) temp.GetComponent<Shadow>().effectColor = new Color(1, 0, 0);
            
            temp.name = $"Button {i}";

            int cap = i;
            temp.GetComponent<Button>().onClick.AddListener(() =>
            {
                ClickBitButton(cap - 1);
            });
        }
        bitButtons.Reverse();
        bitObjects.Reverse();
        scrollViewDownBar.SetAsLastSibling();
        objectViewRect.verticalNormalizedPosition = 0;
    }
    #endregion

    #region ButtonEvent
    public void ClickBitButton(int val)
    {
        Debug.Log($"click node :{val}");
        nowSelectOffset = val;
        nowSelectButton = val;
        downBar.SetActive(true);
        isDownBar = true;
    }

    public void KeyInputFunc()
    {
        if (isDownBar)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) ClickDownBar(0);
            if (Input.GetKeyDown(KeyCode.Alpha2)) ClickDownBar(1);
            if (Input.GetKeyDown(KeyCode.Alpha3)) ClickDownBar(2);
            if (Input.GetKeyDown(KeyCode.Alpha4)) ClickDownBar(3);
            if (Input.GetKeyDown(KeyCode.Alpha5)) ClickDownBar(4);
            if (Input.GetKeyDown(KeyCode.Alpha6)) ClickDownBar(5);
            if (Input.GetKeyDown(KeyCode.Alpha7)) ClickDownBar(6);
            if (Input.GetKeyDown(KeyCode.Delete)) ClickDownBar(5);
        }

        if (isSideBar)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) AddFunc(0);
            if (Input.GetKeyDown(KeyCode.Alpha2)) AddFunc(1);
            if (Input.GetKeyDown(KeyCode.Alpha3)) AddFunc(2);
            if (Input.GetKeyDown(KeyCode.Alpha4)) AddFunc(3);
            if (Input.GetKeyDown(KeyCode.Alpha5)) AddFunc(4);
            if (Input.GetKeyDown(KeyCode.Alpha6)) AddFunc(5);
            if (Input.GetKeyDown(KeyCode.Alpha7)) AddFunc(6);
            if (Input.GetKeyDown(KeyCode.Alpha8)) AddFunc(7);
            if (Input.GetKeyDown(KeyCode.Alpha9)) AddFunc(8);
        }
    }

    public void InitFuncText(int val)
    {
        switch (val)
        {
            case 0:
                if (!string.IsNullOrEmpty(noteAndspeedUI.longNote_Input.text))
                    noteAndspeedUI.longNote_Output.text = int.Parse(noteAndspeedUI.longNote_Input.text).ToString();
                break;
            case 1:
                if (!string.IsNullOrEmpty(noteAndspeedUI.speedChange_Input.text))
                    noteAndspeedUI.speedChange_Output.text = float.Parse(noteAndspeedUI.speedChange_Input.text).ToString();
                break;
            case 2:
                if (!string.IsNullOrEmpty(postprocessUI.vignettingInten_Input.text))
                    postprocessUI.vignettingInten_Output.text = float.Parse(postprocessUI.vignettingInten_Input.text).ToString();
                if (!string.IsNullOrEmpty(postprocessUI.vignettingDuration_Input.text))
                    postprocessUI.vignettingDuration_Output.text = float.Parse(postprocessUI.vignettingDuration_Input.text).ToString();
                break;
            case 3:
                if (!string.IsNullOrEmpty(postprocessUI.bloomInten_Input.text))
                    postprocessUI.bloomInten_Output.text = float.Parse(postprocessUI.bloomInten_Input.text).ToString();
                if (!string.IsNullOrEmpty(postprocessUI.bloomDuration_Input.text))
                    postprocessUI.bloomDuration_Output.text = float.Parse(postprocessUI.bloomDuration_Input.text).ToString();
                break;
            case 4:
                if (!string.IsNullOrEmpty(postprocessUI.flashInten_Input.text))
                    postprocessUI.flashInten_Output.text = float.Parse(postprocessUI.flashInten_Input.text).ToString();
                if (!string.IsNullOrEmpty(postprocessUI.flashInDuration_Input.text))
                    postprocessUI.flashInDuration_Output.text = float.Parse(postprocessUI.flashInDuration_Input.text).ToString();
                if (!string.IsNullOrEmpty(postprocessUI.flashOutDuration_Input.text))
                    postprocessUI.flashOutDuration_Output.text = float.Parse(postprocessUI.flashOutDuration_Input.text).ToString();
                break;
            case 5:
                if (!string.IsNullOrEmpty(postprocessUI.glassBreakOption_Input.text))
                    postprocessUI.glassBreakOption_Output.text = int.Parse(postprocessUI.glassBreakOption_Input.text).ToString();
                break;
            case 6:
                if (!string.IsNullOrEmpty(postprocessUI.paniniLength_Input.text))
                    postprocessUI.paniniLength_Output.text = float.Parse(postprocessUI.paniniLength_Input.text).ToString();
                if (!string.IsNullOrEmpty(postprocessUI.paniniDuration_Input.text))
                    postprocessUI.paniniDuration_Output.text = float.Parse(postprocessUI.paniniDuration_Input.text).ToString();
                break;
            case 7:
                if (!string.IsNullOrEmpty(colorAdjUI.colorAdjPostEx_Input.text))
                    colorAdjUI.colorAdjPostEx_Output.text = float.Parse(colorAdjUI.colorAdjPostEx_Input.text).ToString();
                if (!string.IsNullOrEmpty(colorAdjUI.colorAdjContrast_Input.text))
                    colorAdjUI.colorAdjContrast_Output.text = float.Parse(colorAdjUI.colorAdjContrast_Input.text).ToString();
                if (!string.IsNullOrEmpty(colorAdjUI.colorAdjHueShift_Input.text))
                    colorAdjUI.colorAdjHueShift_Output.text = float.Parse(colorAdjUI.colorAdjHueShift_Input.text).ToString();
                if (!string.IsNullOrEmpty(colorAdjUI.colorAdjSaturation_Input.text))
                    colorAdjUI.colorAdjSaturation_Output.text = float.Parse(colorAdjUI.colorAdjSaturation_Input.text).ToString();
                if (!string.IsNullOrEmpty(colorAdjUI.colorAdjDuration_Input.text))
                    colorAdjUI.colorAdjDuration_Output.text = float.Parse(colorAdjUI.colorAdjDuration_Input.text).ToString();
                break;
            case 8:
                if (!string.IsNullOrEmpty(cameraMoveUI.cameraMovePX_Input.text) &&
                    !string.IsNullOrEmpty(cameraMoveUI.cameraMovePY_Input.text) &&
                    !string.IsNullOrEmpty(cameraMoveUI.cameraMovePZ_Input.text))
                    cameraMoveUI.cameraMoveP_Output.text = new Vector3(
                        float.Parse(cameraMoveUI.cameraMovePX_Input.text),
                        float.Parse(cameraMoveUI.cameraMovePY_Input.text),
                        float.Parse(cameraMoveUI.cameraMovePZ_Input.text)
                    ).ToString();
                break;
            case 9:
                if (!string.IsNullOrEmpty(cameraMoveUI.cameraMoveRX_Input.text) &&
                    !string.IsNullOrEmpty(cameraMoveUI.cameraMoveRY_Input.text) &&
                    !string.IsNullOrEmpty(cameraMoveUI.cameraMoveRZ_Input.text))
                    cameraMoveUI.cameraMoveR_Output.text = new Vector3(
                        float.Parse(cameraMoveUI.cameraMoveRX_Input.text),
                        float.Parse(cameraMoveUI.cameraMoveRY_Input.text),
                        float.Parse(cameraMoveUI.cameraMoveRZ_Input.text)
                    ).ToString();
                break;
        }
    }

    public void AddFunc(int val)
    {
        Debug.Log($"Add Start Func val {val}"); 
        switch (val)
        {
            case 0:
                {
                    editLevel.MinuteNotes.Add(new LongNote(bitOffset * nowSelectOffset, int.Parse(noteAndspeedUI.longNote_Input.text) * (float)bitOffset));
                    noteAndspeedUI.longNote_Input.text = "";
                    noteAndspeedUI.longNote_Output.text = "";
                    Instantiate(circleViewPrefab, bitObjects[nowSelectButton].transform).GetComponent<EditorCircleView>().InitText("Long\nNote", 16.5f);
                    sideBar.SetActive(false);
                    sideFunc[3].SetActive(false);
                    break;
                }
            case 1:
                {
                    editLevel.Effects.Add(new NiddleSpeed(bitOffset * nowSelectOffset, float.Parse(noteAndspeedUI.speedChange_Input.text)));
                    noteAndspeedUI.speedChange_Input.text = "";
                    noteAndspeedUI.speedChange_Output.text = "";
                    Instantiate(circleViewPrefab, bitObjects[nowSelectButton].transform).GetComponent<EditorCircleView>().InitText("Speed\nChange", 16.5f);
                    sideBar.SetActive(false);
                    sideFunc[0].SetActive(false);
                    break;
                }
            case 2:
                {
                    editLevel.Effects.Add(new Vignetting(bitOffset * nowSelectOffset, float.Parse(postprocessUI.vignettingInten_Input.text), float.Parse(postprocessUI.vignettingDuration_Input.text)));
                    postprocessUI.vignettingInten_Input.text = "";
                    postprocessUI.vignettingInten_Output.text = "";
                    postprocessUI.vignettingDuration_Input.text = "";
                    postprocessUI.vignettingDuration_Output.text = "";
                    Instantiate(circleViewPrefab, bitObjects[nowSelectButton].transform).GetComponent<EditorCircleView>().InitText("Vignetting", 14f);
                    sideBar.SetActive(false);
                    sideFunc[val + 5].SetActive(false);
                    break;
                }
            case 3:
                {
                    editLevel.Effects.Add(new Bloom(bitOffset * nowSelectOffset, float.Parse(postprocessUI.bloomInten_Input.text), float.Parse(postprocessUI.bloomDuration_Input.text)));
                    postprocessUI.bloomInten_Input.text = "";
                    postprocessUI.bloomInten_Output.text = "";
                    postprocessUI.bloomDuration_Input.text = "";
                    postprocessUI.bloomDuration_Output.text = "";
                    Instantiate(circleViewPrefab, bitObjects[nowSelectButton].transform).GetComponent<EditorCircleView>().InitText("Bloom", 15.51f);
                    sideBar.SetActive(false);
                    sideFunc[val + 5].SetActive(false);
                    break;
                }
            case 4:
                {
                    editLevel.Effects.Add(new Flash(bitOffset * nowSelectOffset, float.Parse(postprocessUI.flashInDuration_Input.text), float.Parse(postprocessUI.flashOutDuration_Input.text), float.Parse(postprocessUI.flashInten_Input.text)));
                    postprocessUI.flashInten_Input.text = "";
                    postprocessUI.flashInten_Output.text = "";
                    postprocessUI.flashInDuration_Input.text = "";
                    postprocessUI.flashInDuration_Output.text = "";
                    postprocessUI.flashOutDuration_Input.text = "";
                    postprocessUI.flashOutDuration_Output.text = "";
                    Instantiate(circleViewPrefab, bitObjects[nowSelectButton].transform).GetComponent<EditorCircleView>().InitText("Flash", 16.5f);
                    sideBar.SetActive(false);
                    sideFunc[val + 5].SetActive(false);
                    break;
                }
            case 5:
                {
                    editLevel.Effects.Add(new GlassBreak(bitOffset * nowSelectOffset, int.Parse(postprocessUI.glassBreakOption_Input.text)));
                    postprocessUI.glassBreakOption_Input.text = "";
                    postprocessUI.glassBreakOption_Output.text = "";
                    Instantiate(circleViewPrefab, bitObjects[nowSelectButton].transform).GetComponent<EditorCircleView>().InitText("Glass\nBreak", 15f);
                    sideBar.SetActive(false);
                    sideFunc[val + 5].SetActive(false);
                    break;
                }
            case 6:
                {
                    editLevel.Effects.Add(new PaniniProjection(bitOffset * nowSelectOffset, float.Parse(postprocessUI.paniniLength_Input.text), float.Parse(postprocessUI.paniniDuration_Input.text)));
                    postprocessUI.paniniLength_Input.text = "";
                    postprocessUI.paniniLength_Output.text = "";
                    postprocessUI.paniniDuration_Input.text = "";
                    postprocessUI.paniniDuration_Output.text = "";
                    Instantiate(circleViewPrefab, bitObjects[nowSelectButton].transform).GetComponent<EditorCircleView>().InitText("Panini", 16.5f);
                    sideBar.SetActive(false);
                    sideFunc[val + 5].SetActive(false);
                    break;
                }
            case 7:
                {
                    editLevel.Effects.Add(new ColorAdjustments(bitOffset * nowSelectOffset, float.Parse(colorAdjUI.colorAdjPostEx_Input.text), float.Parse(colorAdjUI.colorAdjContrast_Input.text), float.Parse(colorAdjUI.colorAdjHueShift_Input.text), float.Parse(colorAdjUI.colorAdjSaturation_Input.text), float.Parse(colorAdjUI.colorAdjDuration_Input.text)));
                    colorAdjUI.colorAdjPostEx_Input.text = "";
                    colorAdjUI.colorAdjPostEx_Output.text = "";
                    colorAdjUI.colorAdjContrast_Input.text = "";
                    colorAdjUI.colorAdjContrast_Output.text = "";
                    colorAdjUI.colorAdjHueShift_Input.text = "";
                    colorAdjUI.colorAdjHueShift_Output.text = "";
                    colorAdjUI.colorAdjSaturation_Input.text = "";
                    colorAdjUI.colorAdjSaturation_Output.text = "";
                    colorAdjUI.colorAdjDuration_Input.text = "";
                    colorAdjUI.colorAdjDuration_Output.text = "";
                    Instantiate(circleViewPrefab, bitObjects[nowSelectButton].transform).GetComponent<EditorCircleView>().InitText("Color\nAdjust", 14.5f);
                    sideBar.SetActive(false);
                    sideFunc[val + 5].SetActive(false);
                    break;
                }
            case 8:
                {
                    Vector3Serial pos = new Vector3Serial { x = float.Parse(cameraMoveUI.cameraMovePX_Input.text), y = float.Parse(cameraMoveUI.cameraMovePY_Input.text), z = float.Parse(cameraMoveUI.cameraMovePZ_Input.text) };
                    Vector3Serial rot = new Vector3Serial { x = float.Parse(cameraMoveUI.cameraMoveRX_Input.text), y = float.Parse(cameraMoveUI.cameraMoveRY_Input.text), z = float.Parse(cameraMoveUI.cameraMoveRZ_Input.text) };
                    editLevel.Effects.Add(new CameraMove(bitOffset * nowSelectOffset, pos, rot, float.Parse(cameraMoveUI.cameraMoveDuration_Input.text)));
                    cameraMoveUI.cameraMovePX_Input.text = "";
                    cameraMoveUI.cameraMovePY_Input.text = "";
                    cameraMoveUI.cameraMovePZ_Input.text = "";
                    cameraMoveUI.cameraMoveRX_Input.text = "";
                    cameraMoveUI.cameraMoveRY_Input.text = "";
                    cameraMoveUI.cameraMoveRZ_Input.text = "";
                    cameraMoveUI.cameraMoveP_Output.text = "";
                    cameraMoveUI.cameraMoveR_Output.text = "";
                    cameraMoveUI.cameraMoveDuration_Input.text = "";
                    cameraMoveUI.cameraMoveDuration_Output.text = "";
                    Instantiate(circleViewPrefab, bitObjects[nowSelectButton].transform).GetComponent<EditorCircleView>().InitText("Camera\nMove", 14.5f);
                    sideBar.SetActive(false);
                    sideFunc[val + 5].SetActive(false);
                    break;
                }
        }
        Debug.Log($"Add Complete Func val {val}");
    }

    public void ClickEffect(int val)
    {
        isSideBar = false;
        sideFunc[val].SetActive(true);
    }

    public void ClickDownBar(int val)
    {
        nowSelectFunc = val;
        if(sideFunc[val] != null) sideFunc[val].SetActive(true);
        isDownBar = false;
        switch (val)
        {
            case 0:
                {
                    sideBar.SetActive(true);
                    break;
                }
            case 1:
                {
                    sideBar.SetActive(true);
                    isSideBar = true;
                    break;
                }
            case 2:
                {
                    sideBar.SetActive(false);
                    GameObject temp = Instantiate(circleViewPrefab, bitObjects[nowSelectButton].transform);
                    temp.GetComponent<EditorCircleView>().InitText("Note", 16.5f);
                    editLevel.MinuteNotes.Add(new DefaultNote(nowSelectOffset * bitOffset));
                    break;
                }
            case 3:
                {
                    sideBar.SetActive(true);
                    break;
                }
            case 4:
                {
                    sideBar.SetActive(false);
                    GameObject temp = Instantiate(circleViewPrefab, bitObjects[nowSelectButton].transform);
                    temp.GetComponent<EditorCircleView>().InitText("Double\nNote", 16.5f);
                    editLevel.MinuteNotes.Add(new DoubleNote(nowSelectOffset * bitOffset));
                    break;
                }
            case 5:
                {
                    sideBar.SetActive(false);

                    for (int i = 0; i< bitObjects[nowSelectButton].transform.childCount; i++)
                    {
                        Destroy(bitObjects[nowSelectButton].transform.GetChild(i).gameObject);

                        editLevel.Effects.RemoveAll(e => e.StartDelay == nowSelectOffset * bitOffset);
                        editLevel.MinuteNotes.RemoveAll(e => e.StartDelay == nowSelectOffset * bitOffset);
                    }
                    
                    break;
                }
            case 6:
                {
                    sideBar.SetActive(false);
                    break;
                }
        }
        downBar.SetActive(false);
    }
    #endregion

    #region Producer Manage
    private TMP_InputField[] GetProducerInputFields(GameObject producerObject)
    {
        TMP_InputField[] inputFields = producerObject.GetComponentsInChildren<TMP_InputField>(true);

        if (inputFields.Length < 2)
        {
            Debug.LogWarning("프로듀서 UI에 필요한 입력 필드가 부족합니다.");
        }

        return inputFields;
    }

    private void BindProducerInputEvents(GameObject producerObject, int index)
    {
        TMP_InputField[] inputFields = GetProducerInputFields(producerObject);
        if (inputFields.Length < 2)
        {
            return;
        }

        inputFields[0].onValueChanged.AddListener((text) => EditProducer(index, ManagerNameRole.Role, text));
        inputFields[1].onValueChanged.AddListener((text) => EditProducer(index, ManagerNameRole.Name, text));
    }

    private void SetProducerInputValues(GameObject producerObject, ManagerList manager)
    {
        TMP_InputField[] inputFields = GetProducerInputFields(producerObject);
        if (inputFields.Length < 2)
        {
            return;
        }

        inputFields[0].text = manager.Role;
        inputFields[1].text = manager.Name;
    }

    public void AddProducer()
    {
        editLevel.managers.Add(new ManagerList("Null","Null"));
        GameObject tempObj = Instantiate(producerTempletePrefab, producerSpawnPosition.transform);
        producerManagerList.Add(tempObj);
        tempObj.GetComponentInChildren<TextMeshProUGUI>().text = editLevel.managers.Count.ToString();
        int cap = editLevel.managers.Count - 1;
        tempObj.GetComponentInChildren<Button>().onClick.AddListener(() => DeleteManager(cap));
        BindProducerInputEvents(tempObj, cap);
    }

    public void DrawProducerUI(int index)
    {
        GameObject tempObj = Instantiate(producerTempletePrefab, producerSpawnPosition.transform);
        producerManagerList.Add(tempObj);
        int cap = index;
        tempObj.GetComponentInChildren<TextMeshProUGUI>().text = (cap + 1).ToString();
        tempObj.GetComponentInChildren<Button>().onClick.AddListener(() => DeleteManager(cap));
        BindProducerInputEvents(tempObj, cap);
    }

    public void DeleteManager(int index)
    {
        for (int i = 0; i < editLevel.managers.Count; i++) Destroy(producerManagerList[i]);
        producerManagerList.Clear();
        editLevel.managers.RemoveAt(index);
        for (int i = 0; i < editLevel.managers.Count; i++)
        {
            int cap = i;
            DrawProducerUI(cap);
            SetProducerInputValues(producerManagerList[cap], editLevel.managers[i]);
        }
    }

    public void EditProducer(int Index, ManagerNameRole nf, string text)
    {
        if (nf == ManagerNameRole.Role) editLevel.managers[Index].Role = text;
        else if (nf == ManagerNameRole.Name) editLevel.managers[Index].Name = text;
    }
    #endregion

    #region Pannel Controll
    public void ControllKey()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && Input.GetKey(KeyCode.LeftControl)) PannelLeftShift();
        else if (Input.GetKeyDown(KeyCode.RightShift) && Input.GetKey(KeyCode.LeftControl)) PannelRightShift();
    }

    public void PannelRightShift()
    {
        if(Enablepannel != 2)
        {
            Enablepannel++;
            switch (Enablepannel)
            {
                case 0:
                    musicData.SetActive(true);
                    editingData.SetActive(false);
                    saveData.SetActive(false);
                    break;
                case 1:
                    musicData.SetActive(false);
                    editingData.SetActive(true);
                    saveData.SetActive(false);
                    break;
                case 2:
                    musicData.SetActive(false);
                    editingData.SetActive(false);
                    saveData.SetActive(true);
                    break;
            }
        }
        
    }

    public void PannelLeftShift()
    {
        if (Enablepannel != 0)
        {
            Enablepannel--;
            switch (Enablepannel)
            {
                case 0:
                    musicData.SetActive(true);
                    editingData.SetActive(false);
                    saveData.SetActive(false);
                    break;
                case 1:
                    musicData.SetActive(false);
                    editingData.SetActive(true);
                    saveData.SetActive(false);
                    break;
                case 2:
                    musicData.SetActive(false);
                    editingData.SetActive(false);
                    saveData.SetActive(true);
                    break;
            }
        }
    }
    #endregion

    #region EXIT
    private void ExitKeyInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PopUpManager.instance.PopupShow("EXIT", "메인화면으로 나가시겠습니까?", () =>
            {
                SceneManager.LoadScene("MenuSelect");
            }, () =>
            {
                Debug.Log("나가기 취소");
            });
        }
    }
    #endregion

    #region PlayTest
    public void EnablePlayTest()
    {
        editLevel.MinuteNotes.Sort((a, b) => a.StartDelay.CompareTo(b.StartDelay));
        editLevel.Effects.Sort((a, b) => a.StartDelay.CompareTo(b.StartDelay));

        PlayTest.instance.playTestLevel = editLevel;
        PlayTest.instance.Enable();
    }
    #endregion
}
