using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class SelectMenu : MonoBehaviour
{
    [Header("Audio Source Header")]
    public EventReference mainSceneBGM;
    public EventInstance mainBGM;
    public EventReference tickSound;
    public EventInstance tickSoundInstance;

    public EventReference cantMusicRef;
    public EventInstance cantMusicInstance;

    [Header("Motion & System")]
    public Tweener mainTwenner;
    public int nowMenuNum;
    public int saveNumber;

    [Header("Transition Video")]
    public bool isPlayingTransition = false;
    public bool isAlredyLoadScene = false;
    public string nextScene;
    public VideoPlayer transitionVP;
    public VideoClip[] clip;
    public RawImage fadeIO;

    [Header("Character Text")]
    public TMP_Text charText;
    public string[] menuTextList;

    [Header("GameObject")]
    public GameObject menuGroup;
    public GameObject turnCircle;
    public GameObject niddle;

    private void Awake()
    {
        isPlayingTransition = false;
    }

    void Start()
    {
        Debug.Log("Start!");
        nowMenuNum = -9;
        mainBGM.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        mainBGM = RuntimeManager.CreateInstance(mainSceneBGM);
        cantMusicInstance = RuntimeManager.CreateInstance(cantMusicRef);
        Debug.Log($"main BGM Start {gameObject.name}");
        mainBGM.start();
        Application.runInBackground = true;
    }

    private void Update()
    {
        if (isPlayingTransition)
        {
            mainBGM.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

            if (!transitionVP.isPlaying && !isAlredyLoadScene)
            {
                Debug.Log("Load NextScene");
                isAlredyLoadScene = true;
                fadeIO.DOFade(1, 0.1f).OnComplete(() => SceneManager.LoadScene(nextScene));
            }
        }
        else
        {
            EnterScene();
            TurnKey();
            NumberingEven();
        }
    }

    private void EnterScene()
    {
        if (nowMenuNum != -9 && Input.GetKeyDown(KeyCode.Return))
        {
            switch (nowMenuNum)
            {
                case 4: Application.Quit(); Debug.Log("Exit"); break;
                case 5: cantMusicInstance.start(); Debug.Log("Credits"); break;
                case 6: nextScene = "Editor"; transitionScene(1); Debug.Log("Editor"); break;
                case 7: nextScene = "StorySelect"; transitionScene(2); Debug.Log("Story"); break;
                case 0: nextScene = "Playlist"; transitionScene(3); Debug.Log("Playlist"); break;
                case 1: nextScene = "OffsetSetting"; transitionScene(4); Debug.Log("Offset Settings"); break;
                case 2: cantMusicInstance.start(); Debug.Log("Oneline Play"); break;
                case 3: cantMusicInstance.start(); Debug.Log("Inventory"); break;
            }
            
        }
    }

    private void NumberingEven() {
        if (nowMenuNum > 7 && nowMenuNum != -9)
        {
            nowMenuNum = 0;
        }
        else if (nowMenuNum < 0 && nowMenuNum != -9)
        {
            nowMenuNum = 7;
        }
    }

    private void TurnKey()
    {
        float wheelInput = Input.GetAxis("Mouse ScrollWheel");

        if (nowMenuNum == -9 && Input.anyKeyDown && !Input.GetMouseButtonDown(0) && !Input.GetMouseButtonDown(1) && !Input.GetKeyDown(KeyCode.Tab) && !Input.GetKeyDown(KeyCode.LeftAlt) && !Input.GetKeyDown(KeyCode.Escape) && !Input.GetKeyDown(KeyCode.LeftControl) && !Input.GetKeyDown(KeyCode.LeftWindows) && !Input.GetKeyDown(KeyCode.LeftAlt) && !Input.GetKeyDown(KeyCode.RightWindows) && !Input.GetKeyDown(KeyCode.RightControl) && !Input.GetKeyDown(KeyCode.RightAlt)) 
        {
            mainTwenner = DOTween.To(() => new Vector2(0, 0), v => menuGroup.GetComponent<RectTransform>().anchoredPosition = v, new Vector2(0, 1080), 0.5f).OnComplete(() => mainTwenner = null);
            niddle.transform.DOLocalRotate(new Vector3(0, 0, 0), 0.3f);
            nowMenuNum = saveNumber;
        }
        if (nowMenuNum != -9 && Input.GetKeyDown(KeyCode.Escape))
        {
            mainTwenner = DOTween.To(() => new Vector2(0, 1080), v => menuGroup.GetComponent<RectTransform>().anchoredPosition = v, new Vector2(0, 0), 0.5f).OnComplete(() => mainTwenner = null);
            saveNumber = nowMenuNum;
            nowMenuNum = -9;
        }

        if (nowMenuNum != -9 && (Input.GetKeyDown(KeyCode.DownArrow) || wheelInput < 0))
        {
            nowMenuNum++;
            turnCircle.transform.DORotate(new Vector3(0, 0, 45 * nowMenuNum), 0.5f).SetEase(Ease.OutQuint);
            niddle.transform.DOLocalRotate(new Vector3(0, 0, 3), 0.1f).OnComplete(() => niddle.transform.DOLocalRotate(new Vector3(0, 0, 0), 0.15f).SetEase(Ease.Linear));
            tickSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            if (tickSoundInstance.isValid())
            {
                tickSoundInstance.release();
                tickSoundInstance.clearHandle();
            } 
            tickSoundInstance = AudioManager.instance.PlayOneShot(tickSound, transform.position);
            WrapMenuIndex();
            UpdateMenuText();
        }
        if (nowMenuNum != -9 && (Input.GetKeyDown(KeyCode.UpArrow) || wheelInput > 0))
        {
            nowMenuNum--;
            turnCircle.transform.DORotate(new Vector3(0, 0, 45 * nowMenuNum), 0.5f).SetEase(Ease.OutQuint);
            niddle.transform.DOLocalRotate(new Vector3(0, 0, -3), 0.1f).OnComplete(() => niddle.transform.DOLocalRotate(new Vector3(0, 0, 0), 0.15f).SetEase(Ease.Linear));
            tickSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            if (tickSoundInstance.isValid())
            {
                tickSoundInstance.release();
                tickSoundInstance.clearHandle();
            }
            tickSoundInstance = AudioManager.instance.PlayOneShot(tickSound, transform.position);
            WrapMenuIndex();
            UpdateMenuText();
        }
    }

    public void transitionScene(int val)
    {
        mainBGM.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        mainBGM = new EventInstance();

        transitionVP.clip = clip[val];
        transitionVP.Play();
        mainTwenner = DOTween.To(() => new Vector2(0, 1080), v => menuGroup.GetComponent<RectTransform>().anchoredPosition = v, new Vector2(-1920, 1080), 0.3f).OnComplete(() => mainTwenner = null);
        isPlayingTransition = true;
    }

    private void WrapMenuIndex()
    {
        if (menuTextList == null || menuTextList.Length == 0)
            return;

        if (nowMenuNum >= menuTextList.Length)
            nowMenuNum = 0;
        else if (nowMenuNum < 0)
            nowMenuNum = menuTextList.Length - 1;
    }

    private void UpdateMenuText()
    {
        if (charText == null || menuTextList == null || menuTextList.Length == 0)
            return;

        if (nowMenuNum >= 0 && nowMenuNum < menuTextList.Length)
            charText.text = menuTextList[nowMenuNum];
        else
            Debug.LogError($"nowMenuNum not in array : {nowMenuNum}");
    }
}