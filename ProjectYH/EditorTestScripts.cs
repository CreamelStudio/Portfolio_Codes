/*using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FMODUnity;
using System.Collections;
using System.Linq;
using System.IO;
using FMOD.Studio;
using BackEnd.BackndNewtonsoft.Json;
using Unity.VisualScripting;


public class EditorTestScripts : MonoBehaviour
{
    public EventReference music;
    private EventInstance musicInstance;
    public float defaultTurnSpeed;
    public int selBlock;
    public bool isPlay;
    [Header("Play")]
    public UnityEngine.UI.Slider playSpeed;
    public TextMeshProUGUI playSpeedText;
    public TMP_InputField turnSpeed;
    public TextMeshProUGUI turnSpeedText;
    [Header("Fuction")]
    public GameObject[] functionImage;

    [Header("Other UI")]
    public UnityEngine.UI.Slider SpawnProg;
    public ScrollRect viewScroll;

    public GameObject lineButtonPrefab;
    public GameObject buttonSpawnTransform;
    public List<GameObject> buttonList;

    public GameObject downBarButtonM;
    public GameObject sideBarPannel;
    public GameObject[] sideBarButtonM;

    [Header("Button Scale")]
    public UnityEngine.UI.Slider buttonScaleSlider;

    [Header("Bit detail")]
    public UnityEngine.UI.Slider bitDetailSlider;
    public TextMeshProUGUI bitDetailText;

    [Header("BPM")]
    public TMP_InputField bpmInputField;
    public TextMeshProUGUI bpmInputText;

    [Header("MusicGUID")]
    public TMP_InputField musicGUIDField;
    public TextMeshProUGUI MusicGUIDText;

    [Header("Music Name")]
    public TMP_InputField musicNameField;
    public TMP_InputField producerField;
    public TMP_InputField editorNameField;

    [Header("Music Length")]
    public TMP_InputField minuteField;
    public TMP_InputField secondField;

    public int bitCount;
    public int secondTime;
    public float bitOffset;

    public StageData saveJsonData;

    [Header("TempData")]
    public List<float> NoteSpawnMinute;
    public List<float> NoteSpawnDouble;


    public List<Vector4Serial> CameraMoveVec; //MoveDelayTime, CameraX, CameraY, MoveProgTime

    public List<Vector2Serial> SpeedChange; //Delay, Speed
    public List<Vector2Serial> Bloom; //Delay, Power, Duration
    public List<Vector2Serial> Flash; //Delay, Power, Duration

    [Header("SideBar")]
    public TMP_InputField cameraXInput;
    public TMP_InputField cameraYInput;
    public TMP_InputField cameraDurationInput;
    [Space(10f)]
    public TMP_InputField scSpeedInput;
    [Space(10f)]
    public TMP_InputField bloomPowerInput;
    [Space(10f)]
    public TMP_InputField flashDurationInput;

    [Space(10f)]
    public TMP_InputField fileName;

    public int startOffset;

    public GameObject playtester;
    public GameObject EditorObj;

    [Header("PlayTest")]
    public GameObject mainNiddle;
    public GameObject nullNiddle;

    public List<GameObject> noteMinute;

    public GameObject niddlePosition;
    public GameObject noteM;
    public GameObject notePrefab;
    public TextMeshProUGUI startCount;

    public float time;
    public bool isStart;
    public bool isEndMSpawn;
    public int minuteSpawn;

    public bool isPlayMode;

    private void Update()
    {
        if (isPlayMode)
        {
            time += Time.deltaTime;

            SpawnNote();
            TurnNiddles();
        }

        for (int i = 0; i < buttonList.Count; i++)
        {
            buttonList[i].GetComponent<RectTransform>().sizeDelta = new Vector2(303.6624f, 50 * buttonScaleSlider.value);
        }

        SpawnProg.value = buttonList.Count();
        float speedDetail = MathF.Floor((playSpeed.value * 10)) / 10;
        playSpeed.value = speedDetail;
        playSpeedText.text = $"Speed : {playSpeed.value}";

        musicInstance.getTimelinePosition(out int position);
        if (isPlay && musicInstance.isValid() && position >= startOffset)
        {
            musicInstance.setPitch(playSpeed.value);

            viewScroll.verticalNormalizedPosition = (float)((float)position / (float)1000) / (float)secondTime;
            Debug.Log($"{position}");
            Debug.Log($"{((float)(float)position / (float)1000) / (float)secondTime} | Is Audio Playing!");
            if (Input.anyKeyDown && !Input.GetMouseButtonDown(1) && !Input.GetMouseButtonDown(0) && !Input.GetMouseButtonDown(2))
            {
                AddF(3);
                selBlock = Mathf.FloorToInt(S.verticalNormalizedPosition * bitCount);
                Instantiate(functionImage[3], buttonList[bitCount - Mathf.FloorToInt(viewScroll.verticalNormalizedPosition * bitCount)].transform);
                Debug.Log("dmddo");
            }
        }
    }

    public void ToTester()
    {
        NoteSpawnMinute.Sort();
        NoteSpawnDouble.Sort();
        CameraMoveVec.Sort();
        SpeedChange.Sort();
        Bloom.Sort();
        Flash.Sort();
        isPlayMode = true;
        StartCoroutine(Co_StartRoutine());
        EditorObj.SetActive(false);
        playtester.SetActive(true);
    }

    public void ToEditor()
    {
        isPlayMode = false;
        musicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        EditorObj.SetActive(true);
        playtester.SetActive(false);
        time = 0;
        isStart = false;
        isEndMSpawn = false;
        minuteSpawn = 0;
        nullNiddle.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
        mainNiddle.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));

        for (int i = 0; i < noteMinute.Count; i++)
        {
            Destroy(noteMinute[i].gameObject);
        }
    }

    public void MusicGUIDEdit()
    {
        music = new EventReference { Guid = new FMOD.GUID(new Guid(musicGUIDField.text)) };
        MusicGUIDText.text = music.Guid.ToString();
        musicInstance = RuntimeManager.CreateInstance(music);
        Debug.Log("Audio is Init!");
    }

    public void bitDetail()
    {
        bitDetailSlider.value = Mathf.FloorToInt(bitDetailSlider.value);
        bitDetailText.text = bitDetailSlider.value.ToString();
    }

    public void EditBPM()
    {
        bpmInputText.text = float.Parse(bpmInputField.text).ToString() + " BPM";
    }

    public void ButtonSpawn()
    {
        NoteSpawnMinute = new List<float>();
        CameraMoveVec = new List<Vector4Serial>();
        SpeedChange = new List<Vector2Serial>();
        Bloom = new List<Vector2Serial>();
        Flash = new List<Vector2Serial>();

        for (int i = 0; i < buttonList.Count; i++) Destroy(buttonList[i].gameObject);
        buttonList.Clear();

        secondTime = (int.Parse(minuteField.text) * 60) + int.Parse(secondField.text);
        bitCount = (((secondTime / 60)) * int.Parse(bpmInputField.text)) * int.Parse(bitDetailText.text);
        bitOffset = (float)secondTime / (float)bitCount;
        Debug.Log($"bitCOunt : {bitCount}");
        SpawnProg.gameObject.SetActive(true);
        SpawnProg.maxValue = bitCount;

        StartCoroutine(Co_ButtonSpawn());
    }

    public void ClickListButton(int count)
    {

        if (!sideBarPannel.activeSelf)
        {
            downBarButtonM.SetActive(false);
            sideBarPannel.SetActive(false);
            for (int i = 0; i < sideBarButtonM.Length; i++) if (sideBarButtonM[i] != null) sideBarButtonM[i].SetActive(false);
            Debug.Log(count);
            downBarButtonM.SetActive(true);

            selBlock = count;
            Debug.Log("Click!");
        }
    }

    public void ClickDownBar(int val)
    {
        switch (val)
        {
            case 0: Debug.Log("Delete"); AddF(val); break;
            case 1: Debug.Log("Camera"); sideBarButtonM[val].SetActive(true); sideBarPannel.SetActive(true); Instantiate(functionImage[val], buttonList[bitCount - selBlock].transform); break;
            case 2: Debug.Log("SpeedChange"); sideBarButtonM[val].SetActive(true); sideBarPannel.SetActive(true); Instantiate(functionImage[val], buttonList[bitCount - selBlock].transform); break;
            case 3: Debug.Log("Note"); AddF(val); Instantiate(functionImage[val], buttonList[bitCount - selBlock].transform); break;
            case 4: Debug.Log("DoubleNote"); AddF(val); Instantiate(functionImage[val], buttonList[bitCount - selBlock].transform); break;
            case 5: Debug.Log("Bloom"); sideBarButtonM[val].SetActive(true); sideBarPannel.SetActive(true); Instantiate(functionImage[val], buttonList[bitCount - selBlock].transform); break;
            case 6: Debug.Log("Flash"); sideBarButtonM[val].SetActive(true); sideBarPannel.SetActive(true); Instantiate(functionImage[val], buttonList[bitCount - selBlock].transform); break;
        }

        downBarButtonM.SetActive(false);

    }

    public void AddF(int val)
    {
        if (val == 0) //Delete
        {
            for (int i = 0; i < buttonList[bitCount - selBlock].transform.childCount; i++)
            {
                Destroy(buttonList[bitCount - selBlock].transform.GetChild(i).gameObject);
                for (int j = 0; j < CameraMoveVec.Count; j++) if (CameraMoveVec[i].x == bitCount * selBlock) CameraMoveVec.RemoveAt(i);
                for (int j = 0; j < SpeedChange.Count; j++) if (SpeedChange[i].x == bitCount * selBlock) SpeedChange.RemoveAt(i);
                for (int j = 0; j < NoteSpawnMinute.Count; j++) if (NoteSpawnMinute[i] == bitCount * selBlock) NoteSpawnMinute.RemoveAt(i);
                for (int j = 0; j < NoteSpawnDouble.Count; j++) if (NoteSpawnDouble[i] == bitCount * selBlock) NoteSpawnDouble.RemoveAt(i);
                for (int j = 0; j < Bloom.Count; j++) if (Bloom[i].x == bitCount * selBlock) Bloom.RemoveAt(i);
                for (int j = 0; j < Flash.Count; j++) if (Flash[i].x == bitCount * selBlock) Flash.RemoveAt(i);
            }
        }
        else if (val == 1) //Camera
        {
            CameraMoveVec.Add(new Vector4Serial((selBlock * bitOffset) - bitOffset, float.Parse(cameraXInput.text), float.Parse(cameraYInput.text), float.Parse(cameraDurationInput.text)));
        }
        else if (val == 2) //SpeedChange
        {
            SpeedChange.Add(new Vector2Serial((selBlock * bitOffset) - bitOffset, float.Parse(scSpeedInput.text)));
        }
        else if (val == 3) //Note
        {
            NoteSpawnMinute.Add((selBlock * bitOffset) - bitOffset);
        }
        else if (val == 4) //DoubleNote
        {
            NoteSpawnDouble.Add((selBlock * bitOffset) - bitOffset);
        }
        else if (val == 5) //Bloom
        {
            Bloom.Add(new Vector2Serial((selBlock * bitOffset) - bitOffset, float.Parse(bloomPowerInput.text)));
        }
        else if (val == 6) //Flash
        {
            Flash.Add(new Vector2Serial((selBlock * bitOffset) - bitOffset, float.Parse(flashDurationInput.text)));
        }
    }

    public void ClickSideBar(int val)
    {
        sideBarButtonM[val].SetActive(false);
        sideBarPannel.SetActive(false);
        AddF(val);
    }

    IEnumerator Co_ButtonSpawn()
    {

        for (int i = bitCount; i > 0; i--)
        {

            GameObject button = Instantiate(lineButtonPrefab, buttonSpawnTransform.transform);
            buttonList.Add(button);

            if ((i) % bitDetailSlider.value == 0)
            {
                button.GetComponent<Outline>().effectColor = Color.red;
                button.GetComponent<Outline>().effectDistance = new Vector2(0, -5);
            }
            else if ((i) % 4 == 0 && bitDetailSlider.value != 4) button.GetComponent<Outline>().effectColor = Color.blue;

            int cap = i;
            button.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => ClickListButton(cap));
        }
        SpawnProg.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.1f);
        viewScroll.verticalNormalizedPosition = 0;
    }

    public void TurnSpeedEdit()
    {
        defaultTurnSpeed = float.Parse(turnSpeed.text);
        turnSpeedText.text = $"Default : {defaultTurnSpeed}";
    }

    public void AudioPlay()
    {
        startOffset = Mathf.FloorToInt((secondTime * viewScroll.verticalNormalizedPosition) * 1000);
        musicInstance.start();
        musicInstance.setPitch(12f);

        isPlay = true;
    }

    public void AudioStop()
    {
        musicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        Debug.Log("Audio Stop!");
        isPlay = false;
    }

    public void SaveData()
    {
        Debug.Log("Saving Start");
        saveJsonData.MusicName = musicNameField.text;
        saveJsonData.Producer = producerField.text;
        saveJsonData.NoteEditor = editorNameField.text;
        Debug.Log("Music Text Data Save");
        saveJsonData.Bpm = float.Parse(bpmInputField.text);
        saveJsonData.MusicLength = secondTime;
        saveJsonData.musicLink = musicGUIDField.text;
        Debug.Log("Music Data Save");
        saveJsonData.MinuteTurnSpeed = defaultTurnSpeed;
        NoteSpawnMinute.Sort();
        NoteSpawnDouble.Sort();
        saveJsonData.NoteSpawnMinute = NoteSpawnMinute.ToArray();
        saveJsonData.NoteSpawnDouble = NoteSpawnDouble.ToArray();
        Debug.Log("Note Speed Data Save");
        CameraMoveVec.Sort((a, b) => a.x.CompareTo(b.x));
        SpeedChange.Sort((a, b) => a.x.CompareTo(b.x));
        Bloom.Sort((a, b) => a.x.CompareTo(b.x));
        Flash.Sort((a, b) => a.x.CompareTo(b.x));
        saveJsonData.CameraMoveVec = CameraMoveVec.ToArray();
        saveJsonData.SpeedChange = SpeedChange.ToArray();
        saveJsonData.Bloom = Bloom.ToArray();
        saveJsonData.Flash = Flash.ToArray();
        Debug.Log("Fuction Data Save");
        string json = JsonConvert.SerializeObject(saveJsonData, Formatting.Indented);
        string path = Path.Combine(Application.dataPath + "/MainProject/Resources/MapJson", fileName.text + ".json");
        Debug.Log("Json Path Save");
        System.IO.File.WriteAllText(path, json);
        Debug.Log("Save Success");
    }















    private void SpawnNote()
    {
        if (minuteSpawn < NoteSpawnMinute.Count && NoteSpawnMinute[minuteSpawn] < (time) && !isEndMSpawn)
        {
            noteM = Instantiate(notePrefab, niddlePosition.transform.position, niddlePosition.transform.rotation);
            noteMinute.Add(noteM);
            Destroy(noteM, 3.5f);

            if (NoteSpawnMinute.Count - 1 == minuteSpawn)
            {
                isEndMSpawn = true;
            }
            else
            {
                minuteSpawn++;
            }
        }
    }

    private void TurnNiddles()
    {
        nullNiddle.transform.Rotate(0, 0, -int.Parse(turnSpeed.text) * Time.deltaTime);
        if (isStart) mainNiddle.transform.Rotate(0, 0, -int.Parse(turnSpeed.text) * Time.deltaTime);
    }

    IEnumerator Co_StartRoutine()
    {
        startCount.text = "3";
        yield return new WaitForSeconds(1);
        startCount.text = "2";
        yield return new WaitForSeconds(1);
        startCount.text = "1";
        yield return new WaitForSeconds(1);
        startCount.text = "";
        isStart = true;
        musicInstance.start();
    }
}
*/