using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

using Debug = UnityEngine.Debug;

public class NoteSpawnManager : MonoBehaviour
{
    private static readonly Color32 GradeSColor = new Color32(255, 176, 0, 255);
    private static readonly Color32 GradeAColor = new Color32(18, 207, 0, 255);
    private static readonly Color32 GradeBColor = new Color32(0, 144, 255, 255);
    private static readonly Color32 GradeCColor = new Color32(255, 0, 69, 255);
    private static readonly Color32 GradeDColor = new Color32(197, 197, 197, 255);

    public static NoteSpawnManager instance;
    public NoteSpawner minuteNoteSpawner;

    public RawImageAnimation endGlassBreak;
    public TMP_Text gradeText;

    public float defaultFmodOffset;
    public float startOffset;
    public float dsptime;

    // EventReference = 오디오 클립 참조
    // EventInstance = 실제 재생 인스턴스
    public EventReference musicRef;
    public EventInstance musicInstance;
    public EventReference endEffect;

    [SerializeField] private GameObject nullMinuteNiddle;
    public GameObject minuteNiddle;

    [SerializeField] private int defaultNoteCount;
    [SerializeField] private int doubleNoteCount;
    [SerializeField] private int longNoteCount;
    [SerializeField] private int longNoteEndCount;

    public List<GameObject> defaultNoteObjects = new List<GameObject>();
    public List<GameObject> doubleNoteObjects = new List<GameObject>();
    public List<GameObject> longNoteObjects = new List<GameObject>();
    public List<GameObject> longNoteEndObjects = new List<GameObject>();

    [Space(10f)]
    [SerializeField] private GameObject bgaEnd;
    [SerializeField] private RawImageAnimation glassBreak;
    public int perfectCount;
    public int greatCount;
    public int goodCount;
    public int badCount;
    public int missCount;
    public float rate;
    public int maxCombo;

    [SerializeField] private TMP_Text perfectCountText;
    [SerializeField] private TMP_Text greatCountText;
    [SerializeField] private TMP_Text goodCountText;
    [SerializeField] private TMP_Text badCountText;
    [SerializeField] private TMP_Text missCountText;

    [SerializeField] private TMP_Text rateText;
    [SerializeField] private TMP_Text maxComboText;
    [SerializeField] private GameObject keyView;

    [SerializeField] private Slider musicProgressBar;

    [Header("BGA Manager")]
    [SerializeField] public VideoPlayer bgaVP;
    [SerializeField] public RawImage bgaImage;
    [SerializeField] public RawImage spaceImage;

    [Header("Music Info")]
    [SerializeField] private TMP_Text musicText;

    [Header("Pause Menu")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private TMP_Text countdownText;

    private bool isPaused;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        StartCoroutine(GameManager.instance.LoadJson());
    }

    public void LoadComplete()
    {
        GameManager.instance.dsptime = 0f;
        GameManager.instance.isPlaying = false;
        GameManager.instance.isStart = true;

        startOffset = 180f / GameManager.instance.currentLevel.defaultMinuteSpeed;

        if (GameManager.instance.currentLevel.managers.Count > 0)
        {
            musicText.text = $"{GameManager.instance.currentLevel.managers[0].Name} - {GameManager.instance.currentLevel.musicName}";
        }

        TransitionCanvas.instance.InitProgress(1);
        TransitionCanvas.instance.OnOffTransition(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !GameManager.instance.isEnding)
        {
            if (!pauseMenu.activeSelf)
            {
                Pause();
            }
            else
            {
                Resume();
            }
        }

        MusicTimer();
        NiddleTurn();

        if (GameManager.instance.isStart)
        {
            SpawnNoteManage();
        }
    }

    public void Pause()
    {
        if (isPaused)
        {
            Debug.Log("이미 일시정지 상태입니다.");
            return;
        }

        isPaused = true;
        musicInstance.setPaused(true);

        if (bgaVP.isPlaying)
        {
            bgaVP.Pause();
        }

        Time.timeScale = 0f;
        pauseMenu.SetActive(true);
    }

    public void Resume()
    {
        pauseMenu.SetActive(false);
        countdownText.gameObject.SetActive(true);
        StartCoroutine(Co_ResumeAfterSeconds(3));
    }

    public void Settings()
    {
        Debug.Log("설정 메뉴를 엽니다.");
    }

    public void ExitGame()
    {
        Time.timeScale = 1f;

        if (musicInstance.isValid())
        {
            musicInstance.release();
            musicInstance.clearHandle();
        }

        GameManager.instance.ResetManager();
        SceneManager.LoadSceneAsync("Playlist");
    }

    private IEnumerator Co_ResumeAfterSeconds(int seconds)
    {
        countdownText.text = seconds.ToString();

        for (int i = seconds - 1; i >= 0; i--)
        {
            yield return new WaitForSecondsRealtime(1f);
            countdownText.text = i.ToString();
        }

        musicInstance.setPaused(false);

        if (bgaVP.isPrepared)
        {
            bgaVP.Play();
        }

        Time.timeScale = 1f;
        countdownText.gameObject.SetActive(false);
        isPaused = false;
    }

    private void NiddleTurn()
    {
        if (GameManager.instance.isPlaying)
        {
            minuteNiddle.transform.Rotate(0f, 0f, GameManager.instance.currentLevel.defaultMinuteSpeed * Time.deltaTime);
        }

        if (GameManager.instance.isStart)
        {
            nullMinuteNiddle.transform.Rotate(0f, 0f, GameManager.instance.currentLevel.defaultMinuteSpeed * Time.deltaTime);
        }
    }

    private void MusicTimer()
    {
        musicProgressBar.value = dsptime / (GameManager.instance.currentLevel.musicLength + 3.5f);

        if (GameManager.instance.isStart)
        {
            GameManager.instance.dsptime += Time.deltaTime;
        }

        if (GameManager.instance.currentLevel.bgaPath != "NULL" && GameManager.instance.isStart)
        {
            bgaImage.gameObject.SetActive(true);
        }

        musicInstance.getPlaybackState(out PLAYBACK_STATE playState);

        if (playState != PLAYBACK_STATE.PLAYING
            && GameManager.instance.dsptime >= startOffset - (GameManager.instance.soundOffset / 1000f) + (defaultFmodOffset / 1000f)
            && !GameManager.instance.isPlaying)
        {
            Debug.Log("[FMOD] Music Prepare");
            musicInstance.start();

            if (bgaVP.isPrepared)
            {
                bgaVP.Play();
            }

            Debug.Log("[FMOD] Music Start Complete");
        }

        if (!GameManager.instance.isPlaying
            && GameManager.instance.dsptime >= startOffset - (GameManager.instance.keyOffset / 1000f)
            && GameManager.instance.isStart)
        {
            GameManager.instance.isPlaying = true;
        }

        if (GameManager.instance.dsptime > GameManager.instance.currentLevel.musicLength + 1f && !GameManager.instance.isEnding)
        {
            musicInstance.stop(STOP_MODE.ALLOWFADEOUT);
        }

        if (GameManager.instance.dsptime > GameManager.instance.currentLevel.musicLength + 3.5f && !GameManager.instance.isEnding)
        {
            GameManager.instance.isEnding = true;
            StartCoroutine(Co_EndAnimation());
        }

        if (GameManager.instance.isEnding && Input.GetKeyDown(KeyCode.Return))
        {
            GameManager.instance.ResetManager();

            if (SceneManager.GetActiveScene().name == "Tutorial")
            {
                SceneManager.LoadSceneAsync("MenuSelect");
            }
            else
            {
                SceneManager.LoadSceneAsync("Playlist");
            }
        }

        if (GameManager.instance.isEnding && Input.GetKeyDown(KeyCode.F5))
        {
            musicInstance.release();
            musicInstance.clearHandle();

            AssetReference map = GameManager.instance.levelReference;
            GameManager.instance.ResetManager();
            GameManager.instance.levelReference = map;
            TransitionCanvas.instance.OnOffTransition(true);
            SceneManager.LoadSceneAsync("Ingame");
        }

        if (GameManager.instance.isEnding && endGlassBreak.imageCount >= 3)
        {
            UpdateGradeDisplay(IngameInput.instance.rate);
            gradeText.transform.DORotate(Vector3.zero, 0.2f);
        }
    }

    private IEnumerator Co_EndAnimation()
    {
        bgaEnd.SetActive(true);

        if (musicInstance.isValid())
        {
            musicInstance.release();
            musicInstance.clearHandle();
        }

        musicInstance = RuntimeManager.CreateInstance(endEffect);
        musicInstance.start();

        perfectCountText.text = $"Perfect {perfectCount}";
        greatCountText.text = $"Great {greatCount}";
        goodCountText.text = $"Good {goodCount}";
        badCountText.text = $"Bad {badCount}";
        missCountText.text = $"Miss {missCount}";

        rateText.text = $"Rate {rate}%";
        maxComboText.text = $"{maxCombo} Combo";

        yield return new WaitForSeconds(0.3f);
        keyView.SetActive(true);
        yield return new WaitForSeconds(2.6f);
        glassBreak.isPlay = true;
    }

    public float getDspTime()
    {
        musicInstance.getPlaybackState(out PLAYBACK_STATE playState);

        if (playState == PLAYBACK_STATE.PLAYING || playState == PLAYBACK_STATE.STARTING)
        {
            // 음악이 재생 중이거나 시작 중이면 FMOD의 정확한 시간을 사용한다.
            musicInstance.getTimelinePosition(out int position);
            dsptime = position / 1000f;
        }
        else
        {
            // 프리롤 구간에서는 -startOffset ~ 0 범위로 흐르게 만든다.
            dsptime = GameManager.instance.dsptime - startOffset;
        }

        // 유저 사운드 오프셋은 한 번만 적용한다.
        dsptime += GameManager.instance.soundOffset / 1000f;
        return dsptime;
    }

    private void SpawnNoteManage()
    {
        float chartTime = getDspTime();

        if (defaultNoteCount < PlayDataManager.instance.defaultNote.Count
            && chartTime + startOffset >= PlayDataManager.instance.defaultNote[defaultNoteCount].StartDelay)
        {
            Debug.Log("Spawn Default Note");
            defaultNoteObjects.Add(minuteNoteSpawner.SpawnDefaultNote());
            defaultNoteCount++;
        }

        if (doubleNoteCount < PlayDataManager.instance.doubleNote.Count
            && chartTime + startOffset >= PlayDataManager.instance.doubleNote[doubleNoteCount].StartDelay)
        {
            Debug.Log("Spawn Double Note");
            doubleNoteObjects.Add(minuteNoteSpawner.SpawnDoubleNote());
            doubleNoteCount++;
        }

        if (longNoteCount < PlayDataManager.instance.longNote.Count
            && chartTime + startOffset >= PlayDataManager.instance.longNote[longNoteCount].StartDelay)
        {
            Debug.Log("Spawn Long Note");
            longNoteObjects.Add(minuteNoteSpawner.SpawnLongNote(startOffset));
            longNoteCount++;
        }

        if (longNoteEndCount < PlayDataManager.instance.longNote.Count
            && longNoteEndCount < minuteNoteSpawner.longNoteRenderer.Count)
        {
            LongNote longNote = PlayDataManager.instance.longNote[longNoteEndCount];
            float longNoteEndTime = (float)longNote.StartDelay + longNote.EndDelay;

            if (chartTime + startOffset >= longNoteEndTime)
            {
                Debug.Log("Long Note End Rendering");
                longNoteEndObjects.Add(minuteNoteSpawner.StopLongNoteRenderer(longNoteEndCount));
                longNoteEndCount++;
            }
        }
    }

    private void UpdateGradeDisplay(float currentRate)
    {
        if (currentRate >= 97f)
        {
            gradeText.text = "S+";
            gradeText.color = GradeSColor;
        }
        else if (currentRate >= 93f)
        {
            gradeText.text = "S";
            gradeText.color = GradeSColor;
        }
        else if (currentRate >= 89f)
        {
            gradeText.text = "A";
            gradeText.color = GradeAColor;
        }
        else if (currentRate >= 73f)
        {
            gradeText.text = "B";
            gradeText.color = GradeBColor;
        }
        else if (currentRate >= 60f)
        {
            gradeText.text = "C";
            gradeText.color = GradeCColor;
        }
        else
        {
            gradeText.text = "D";
            gradeText.color = GradeDColor;
        }
    }
}
