using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Video;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using System;
using FMODUnity;
using UnityEngine.SceneManagement;
using System.Collections;
public class LevelLayout : MonoBehaviour
{
    [Header("Layout")]
    public Slider difficultSlider;
    public GameObject[] musicButtonObj;
    public CanvasGroup[] musicButtonCG;
    public RectTransform[] musicButtonRect;
    public Tweener[] musicPositionBTween = new Tweener[9];
    public Tweener[] musicRotationBTween = new Tweener[9];
    public Tweener[] musicScaleBTween = new Tweener[9];
    public int[] buttonIndex = new int[9];
    public Vector3[] buttonPosition;
    public Vector3[] buttonRotation;
    public Vector3[] buttonScale;
    public float duringAnimTime;

    [Header("Video Play")]
    public float changeVPDuration;
    public int mainSelectMusic;
    public float isChangeVS;
    public bool isChange;
    public bool isNowVP1;
    public VideoPlayer backgroundVP1;
    public VideoPlayer backgroundVP2;
    public RenderTexture bgaTexture;

    [Header("System")]
    public EventReference scrollTickSound;
    public EventReference cantPlayRef;
    public bool isLoadingComplete;
    public int downCount;
    public Texture2D missingTexture;
    public AssetReference pathListLink;
    public MusicPathList pathManager;
    public List<VideoClip> videoClips;
    public List<Texture2D> albumCovers;

    [Header("Key Input Hold")]
    public float holdTimeFlag;
    public float autoScrollFlag;

    public bool isPressDown;
    public float downPressTime;
    public float autoScrollDownTime;
    public bool isDown;

    public bool isPressUp;
    public float upPressTime;
    public float autoScrolUpTime;
    public bool isUp;

    public Animator playButtonAnim;

    [Header("Loading")]
    public GameObject loadingObj;
    public TextMeshProUGUI loadingPercentText;

    [Header("Difficult Controller")]
    public int difficultValue;
    public RectTransform[] difficultButtons;
    public float dificultMoveAnimtime;

    [Header("MusicInfo")]
    public RawImage musicInfoAlbumCover;
    public TextMeshProUGUI musicInfoTitle;
    public TextMeshProUGUI musicInfoProducer;

    void Start()
    {
        Application.runInBackground = true;
        StartCoroutine(LoadAssetsCoroutine());
        playButtonAnim.SetBool("isEnable", true);
        DifficultInit();
    }



    private void Update()
    {
        if (isLoadingComplete)
        {
            ControllButton();
            ChangeDifficult();
            InputExitKey();
            PlayButtonClick();
        }
    }

    private void PlayButtonClick()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            var musicData = pathManager.pathList[mainSelectMusic];
            AssetReference levelRef = null;
            switch (difficultValue)
            {
                case 0: levelRef = musicData.MusicNormalPath; break;
                case 1: levelRef = musicData.MusicHardPath; break;
                case 2: levelRef = musicData.MusicMaxPath; break;
                case 3: levelRef = musicData.MusicYHPath; break;
            }

            if (levelRef == null || string.IsNullOrEmpty(levelRef.AssetGUID) || !levelRef.RuntimeKeyIsValid())
            {
                PopUpManager.instance.PopupShow("채보 미작업", "해당 곡은 플레이할 수 없습니다.", () => { }, () => { });
                RuntimeManager.CreateInstance(cantPlayRef).start();
                return;
            }

            // 에셋 실제 내용 검사
            Addressables.LoadAssetAsync<TextAsset>(levelRef).Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded && !string.IsNullOrWhiteSpace(handle.Result.text))
                {
                    // 정상 플레이 진행
                    GameManager.instance.levelReference = levelRef;
                    DontDestroyOnLoad(TransitionCanvas.instance.gameObject);
                    TransitionCanvas.instance.OnOffTransition(true);
                    TransitionCanvas.instance.InitText(musicData.MusicName, musicData.MainComposer);
                    StartCoroutine(Co_IngameSceneLoad("Ingame"));
                }
                else
                {
                    PopUpManager.instance.PopupShow("채보 미작업", "해당 곡은 플레이할 수 없습니다.", () => { }, () => { });
                    RuntimeManager.CreateInstance(cantPlayRef).start();
                }
            };
        }
    }

    IEnumerator Co_IngameSceneLoad(string SceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(SceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    private void ChangeVideoTimer()
    {
        Debug.Log($"OnPrepareCompleted : {mainSelectMusic}");
        if (isNowVP1)
        {
            backgroundVP2.clip = videoClips[mainSelectMusic];
            backgroundVP2.time = pathManager.pathList[mainSelectMusic].MusicHighlight;
            backgroundVP2.prepareCompleted += OnPrepareCompleted;
            backgroundVP2.Prepare();
            DOTween.To(() => (float)1, x => backgroundVP2.SetDirectAudioVolume(0, x), (float)0, changeVPDuration / 2).OnComplete(() => { DOTween.To(() => (float)0, x => backgroundVP2.SetDirectAudioVolume(0, x), (float)1, changeVPDuration / 2); });
        }
        else
        {
            backgroundVP1.clip = videoClips[mainSelectMusic];
            backgroundVP1.time = pathManager.pathList[mainSelectMusic].MusicHighlight;
            backgroundVP1.prepareCompleted += OnPrepareCompleted;
            backgroundVP1.Prepare();
            DOTween.To(() => (float)1, x => backgroundVP1.SetDirectAudioVolume(0, x), (float)0, changeVPDuration / 2).OnComplete(() => { DOTween.To(() => (float)0, x => backgroundVP1.SetDirectAudioVolume(0, x), (float)1, changeVPDuration / 2); });
        }
        
    }

    public void OnPrepareCompleted(VideoPlayer vp)
    {
        if (vp.name == "BGAVP1")
        {
            isNowVP1 = true;
            vp.targetTexture = bgaTexture;
            backgroundVP2.targetTexture = null;
            vp.Play();
            backgroundVP2.Stop();
        }
        else
        {
            isNowVP1 = false;
            vp.targetTexture = bgaTexture;
            backgroundVP1.targetTexture = null;
            vp.Play();
            backgroundVP1.Stop();
        }
        
    }

    private void DifficultInit()
    {
        for (int i = 0; i < difficultButtons.Length; i++)
        {
            if (difficultValue == i)
            {
                difficultButtons[i].localPosition = new Vector3(-120, 0, dificultMoveAnimtime);
                difficultButtons[i].DOLocalMove(new Vector3(0, 0, 0), dificultMoveAnimtime);
            }
            else difficultButtons[i].DOLocalMove(new Vector3(120, 0, 0), dificultMoveAnimtime);
        }
        InitDifficultSlider();
    }

    private void ChangeDifficult()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (difficultValue != 3)
            {
                difficultValue++;
                DifficultInit();
            }
        }
        else if(Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (difficultValue != 0)
            {
                difficultValue--;
                DifficultInit();
            }
        }
    }

    private void InitDifficultSlider()
    {
        switch (difficultValue)
        {
            case 0:
                {
                    difficultSlider.value = pathManager.pathList[mainSelectMusic].DifficultNormal;
                    break;
                }
            case 1:
                {
                    difficultSlider.value = pathManager.pathList[mainSelectMusic].DifficultHard;
                    break;
                }
            case 2:
                {
                    difficultSlider.value = pathManager.pathList[mainSelectMusic].DifficultMax;
                    break;
                }
            case 3:
                {
                    difficultSlider.value = pathManager.pathList[mainSelectMusic].DifficultYH;
                    break;
                }
        }
    }

    private void ControllButton()
    {
        float wheelInput = Input.GetAxis("Mouse ScrollWheel");
        if (Input.GetKeyDown(KeyCode.UpArrow) || wheelInput > 0)
        {
            ScrollButton(false);
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            isPressDown = true;
        }
        if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            isPressDown = false;
        }

        if (isPressDown)
        {
            downPressTime += Time.deltaTime;
            if (downPressTime >= holdTimeFlag)
            {
                autoScrollDownTime += Time.deltaTime;
                if (autoScrollFlag <= autoScrollDownTime)
                {
                    autoScrollDownTime = 0;
                    ScrollButton(false);
                }
            }
        }
        else
        {
            downPressTime = 0;
        }



        if (Input.GetKeyDown(KeyCode.DownArrow) || wheelInput < 0)
        {
            ScrollButton(true);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            isPressUp = true;
        }
        if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            isPressUp = false;
        }

        if (isPressUp)
        {
            upPressTime += Time.deltaTime;
            if (upPressTime >= holdTimeFlag)
            {
                autoScrolUpTime += Time.deltaTime;
                if (autoScrollFlag <= autoScrolUpTime)
                {
                    autoScrolUpTime = 0;
                    ScrollButton(true);
                }
            }
        }
        else
        {
            upPressTime = 0;
        }
    }

    private void ScrollButton(bool isUp)
    {
        playButtonAnim.SetBool("isEnable", false);
        RuntimeManager.CreateInstance(scrollTickSound).start();
        if (!isUp && !this.isUp)
        {
            for (int i = 0; i < musicButtonRect.Length; i++)
            {
                musicButtonRect[i].localPosition = buttonPosition[i];
                musicButtonRect[i].localScale = buttonScale[i];
                musicButtonRect[i].rotation = Quaternion.Euler(buttonRotation[i]);
            }
            for (int i = 0; i < musicButtonRect.Length; i++)
            {
                musicButtonRect[i].localPosition = buttonPosition[i];
                musicButtonRect[i].localScale = buttonScale[i];
                musicButtonRect[i].rotation = Quaternion.Euler(buttonRotation[i]);
                if (i + 1 < musicButtonRect.Length)
                {
                    this.isDown = true;
                    musicPositionBTween[i].Kill();
                    musicRotationBTween[i].Kill();
                    musicScaleBTween[i].Kill();
                    int index = i;
                    musicPositionBTween[index] = musicButtonRect[index].DOLocalMove(buttonPosition[index + 1], duringAnimTime).SetEase(Ease.OutCirc).OnKill(() => { InitButtonMove(index, false); });
                    musicRotationBTween[index] = musicButtonRect[index].DOLocalRotate(buttonRotation[index + 1], duringAnimTime).SetEase(Ease.OutCirc);
                    musicScaleBTween[index] = musicButtonRect[index].DOScale(buttonScale[index + 1], duringAnimTime).SetEase(Ease.OutCirc);
                }
            }
            InitButtonMove(musicButtonRect.Length - 1, isUp);
        }
        else if (!this.isDown && isUp)
        {
            for (int i = 0; i < musicButtonRect.Length; i++)
            {
                musicButtonRect[i].localPosition = buttonPosition[i];
                musicButtonRect[i].localScale = buttonScale[i];
                musicButtonRect[i].rotation = Quaternion.Euler(buttonRotation[i]);
            }
            for (int i = 0; i < musicButtonRect.Length; i++)
            {
                musicButtonRect[i].localPosition = buttonPosition[i];
                if (i - 1 != -1)
                {
                    this.isUp = true;
                    musicPositionBTween[i].Kill();
                    musicRotationBTween[i].Kill();
                    musicScaleBTween[i].Kill();
                    int index = i;
                    Debug.Log(buttonRotation[index - 1]);
                    musicPositionBTween[index] = musicButtonRect[index].DOLocalMove(buttonPosition[index - 1], duringAnimTime).SetEase(Ease.OutCirc).OnKill(() => { InitButtonMove(index, true); });
                    musicRotationBTween[index] = musicButtonRect[index].DOLocalRotate(buttonRotation[index - 1], duringAnimTime).SetEase(Ease.OutCirc);
                    musicScaleBTween[index] = musicButtonRect[index].DOScale(buttonScale[index - 1], duringAnimTime).SetEase(Ease.OutCirc);
                }
            }
            InitButtonMove(0, isUp);
        }
    }

    private void InitButtonMove(int buttonIndex, bool isUp)
    {
        musicButtonRect[buttonIndex].localPosition = buttonPosition[buttonIndex];
        musicButtonRect[buttonIndex].rotation = Quaternion.Euler(buttonRotation[buttonIndex]);
        musicButtonRect[buttonIndex].localScale = buttonScale[buttonIndex];
        if (buttonIndex == 4) playButtonAnim.SetBool("isEnable", true);
        if (isUp)
        {
            this.buttonIndex[buttonIndex]++;
            this.isUp = false;
        }
        else
        {
            this.isDown = false;
            this.buttonIndex[buttonIndex]--;
            if (this.buttonIndex[buttonIndex] < 0)
            {
                this.buttonIndex[buttonIndex] = albumCovers.Count - 1;
            }
        }

        int musicIndex = this.buttonIndex[buttonIndex] % albumCovers.Count;
        musicButtonObj[buttonIndex].GetComponentInChildren<RawImage>().texture = albumCovers[musicIndex];
        musicButtonObj[buttonIndex].GetComponentsInChildren<TextMeshProUGUI>()[0].text = pathManager.pathList[musicIndex].MusicName;
        musicButtonObj[buttonIndex].GetComponentsInChildren<TextMeshProUGUI>()[1].text = pathManager.pathList[musicIndex].MainComposer;

        InitDifficultSlider();

        mainSelectMusic = this.buttonIndex[4] % albumCovers.Count;
        if (buttonIndex == 4)
        {
            Debug.Log($"Select : {mainSelectMusic} | ButtonIndex : {buttonIndex}");
            musicInfoAlbumCover.texture = albumCovers[musicIndex];
            musicInfoTitle.text = pathManager.pathList[musicIndex].MusicName;
            musicInfoProducer.text = pathManager.pathList[musicIndex].MainComposer;
            ChangeVideoTimer();
        }
    }

    public void InitMusicInfo()
    {
        int musicIndex = this.buttonIndex[4] % albumCovers.Count;
        musicInfoAlbumCover.texture = albumCovers[musicIndex];
        musicInfoTitle.text = pathManager.pathList[musicIndex].MusicName;
        musicInfoProducer.text = pathManager.pathList[musicIndex].MainComposer;
    }

    #region Datas Load
    private IEnumerator<AsyncOperationHandle> LoadAssetsCoroutine()
    {
        Debug.Log("[Level Layout] Asset Load Start");
        loadingObj.SetActive(true);
        loadingPercentText.text = "Loading... 0%";
        pathManager.pathList.Sort(CompareByMusicName);
        for (int i = 0; i < pathManager.pathList.Count; i++)
        {
            loadingPercentText.text = $"Loading... {(i / pathManager.pathList.Count) * 100}%";
            // ���� �ε�
            AssetReference videoReference = pathManager.pathList[i].MusicVideoPath;

            if (videoReference == null || !videoReference.RuntimeKeyIsValid())
            {
                Debug.LogWarning($"Video reference is null or has an invalid key at index {i}. Adding null to videoClips.");
                videoClips.Add(null);
            }
            else if (!videoReference.OperationHandle.IsValid())
            {
                AsyncOperationHandle<VideoClip> videoHandle = videoReference.LoadAssetAsync<VideoClip>();
                yield return videoHandle;

                if (videoHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    videoClips.Add(videoHandle.Result);
                }
                else
                {
                    Debug.LogWarning($"Failed to load video clip at index {i}: {videoReference}. Adding null to videoClips.");
                    videoClips.Add(null);
                }
            }
            else
            {
                videoClips.Add((VideoClip)videoReference.OperationHandle.Result);
            }

            // �ٹ� Ŀ�� �ε�
            AssetReference albumCoverReference = pathManager.pathList[i].MusicAlbumCoverPath;

            if (albumCoverReference == null || !albumCoverReference.RuntimeKeyIsValid())
            {
                Debug.LogWarning($"Album cover reference is null or has an invalid key at index {i}. Adding missing texture to albumCovers.");
                albumCovers.Add(missingTexture);
            }
            else if (!albumCoverReference.OperationHandle.IsValid())
            {
                AsyncOperationHandle<Texture2D> albumCoverHandle = albumCoverReference.LoadAssetAsync<Texture2D>();
                yield return albumCoverHandle;

                if (albumCoverHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    albumCovers.Add(albumCoverHandle.Result);
                }
                else
                {
                    Debug.LogWarning($"Failed to load album cover at index {i}: {albumCoverReference}. Adding missing texture to albumCovers.");
                    albumCovers.Add(missingTexture);
                }
            }
            else
            {
                albumCovers.Add((Texture2D)albumCoverReference.OperationHandle.Result);
            }
        }




        while (buttonIndex[4] % albumCovers.Count != 0)
        {
            for (int i = 0; i < buttonIndex.Length; i++)
            {
                buttonIndex[i]++;
            }
        }

        for (int i = 0; i < buttonIndex.Length; i++)
        {
            int musicIndex = this.buttonIndex[i] % albumCovers.Count;
            musicButtonObj[i].GetComponentInChildren<RawImage>().texture = albumCovers[musicIndex];
            musicButtonObj[i].GetComponentsInChildren<TextMeshProUGUI>()[0].text = pathManager.pathList[musicIndex].MusicName;
            musicButtonObj[i].GetComponentsInChildren<TextMeshProUGUI>()[1].text = pathManager.pathList[musicIndex].MainComposer;
        }
        ChangeVideoTimer();
        InitMusicInfo();
        Debug.Log("[Level Layout] Asset Load Complete");
        loadingPercentText.text = "Loading... 100%";
        loadingObj.GetComponent<CanvasGroup>().DOFade(0, 0.1f).OnComplete(() =>
        {
            loadingObj.SetActive(false);
            isLoadingComplete = true;
        });
        Debug.Log("[Level Layout] Asset Loading End");
    }
    #endregion

    #region system
    int CompareByMusicName(MusicPathObj x, MusicPathObj y)
    {
        return string.Compare(x.MusicName, y.MusicName, StringComparison.Ordinal);
    }
    #endregion

    #region EXIT
    private void InputExitKey()
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
}
