using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ProjectYH_Server.Models;
using DG.Tweening;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Collections;
using FMODUnity;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class MultiRoomUI : MonoBehaviour
{
    [Header("Multi Create Room UI")]
    public GameObject createRoomUI;
    public Button[] maxPlayerButtons;
    public TMP_InputField roomNameField;
    public TMP_InputField roomPasswordField;

    private int selectedMaxPlayers;

    [Header("Multi Room Default UI")]
    public GameObject multiRoomDefaultPannel;
    public TMP_Text roomName;
    public TMP_Text playerCount;
    public GameObject[] playerList;

    
    [Header("Music Selection UI")]
    [Header("Layout")]
    public Image difficultSlider;
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

    [Header("Difficult Controller")]
    public int difficultValue;
    public RectTransform[] difficultButtons;
    public float dificultMoveAnimtime;

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

    public float maxDifficultValue;


    void Start()
    {
        NetworkManager.Instance.OnJoinedRoom += EnableCurrentRoomInfo;
        NetworkManager.Instance.OnLeftRoom += DisableCurrentRoomInfo;
        NetworkManager.Instance.OnRoomUpdated += UpdateCurrentRoomInfo;

        StartCoroutine(LoadAssetsCoroutine());
        SelectMaxPlayers(9);     
    }

    void Update()
    {
        ChangeDifficult();
        ControllButton();

    }

    #region  Room Create UI Methods
    public void OpenCreateRoomUI()
    {
        createRoomUI.SetActive(true);
    }
    public void CloseCreateRoomUI()
    {
        SelectMaxPlayers(9);
        roomNameField.text = "";
        roomPasswordField.text = "";
        createRoomUI.SetActive(false);
    }

    private void EnableCurrentRoomInfo(RoomData room)
    {
        multiRoomDefaultPannel.SetActive(true);

        roomName.text = room.RoomName;
        playerCount.text = $"{room.Players?.Count ?? 0} / {room.MaxPlayers}";

        for(int i = 0; i < playerList.Length; i++)
        {
            if(i < room.Players?.Count)
            {
                playerList[i].SetActive(true);
            }
            else
            {
                playerList[i].SetActive(false);
            }
        }
    }

    public void DisableCurrentRoomInfo()
    {
        multiRoomDefaultPannel.SetActive(false);
    }

    public void UpdateCurrentRoomInfo(RoomData room)
    {
        playerCount.text = $"{room.Players?.Count ?? 0} / {room.MaxPlayers}";

        for (int i = 0; i < playerList.Length; i++)
        {
            if (i < room.Players?.Count)
            {
                playerList[i].SetActive(true);
            }
            else
            {
                playerList[i].SetActive(false);
            }
        }
    }

    public void EnableDisablePasswordField(bool isPasswordProtected)
    {
        roomPasswordField.gameObject.SetActive(isPasswordProtected);
    }

    public void SelectMaxPlayers(int maxPlayers)
    {
        selectedMaxPlayers = maxPlayers;

        for(int i = 1; i < maxPlayerButtons.Length + 1; i++)
        {
            if(i == maxPlayers)
            {
                maxPlayerButtons[i - 1].colors = new ColorBlock()
                {
                    normalColor = Color.green,
                    highlightedColor = Color.green,
                    pressedColor = Color.green,
                    selectedColor = Color.green,
                    disabledColor = Color.gray,
                    colorMultiplier = 1,
                    fadeDuration = 0.1f
                };
            }
            else
            {
                maxPlayerButtons[i - 1].colors = new ColorBlock()
                {
                    normalColor = Color.white,
                    highlightedColor = Color.white,
                    pressedColor = Color.white,
                    selectedColor = Color.white,
                    disabledColor = Color.gray,
                    colorMultiplier = 1,
                    fadeDuration = 0.1f
                };
            }
        }
    }

    public async void CreateRoom()
    {
        await NetworkManager.Instance.CreateRoom(roomNameField.text, selectedMaxPlayers, roomPasswordField.text);
        CloseCreateRoomUI();
    }

    public async void LeaveRoom()
    {
        await NetworkManager.Instance.LeaveRoom();
    }

    public void QuickJoinRoom()
    {
        MultiRoomListManager.Instance.QuickJoinRoom();
    }

    public async void RefreshRoomList()
    {
        await NetworkManager.Instance.UpdateRoomList();
    }

    public void BackToMainMenu()
    {
        MultiRoomListManager.Instance.Quit();
    }
    #endregion

    #region  Music Selection Methods
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
                    difficultSlider.fillAmount = pathManager.pathList[mainSelectMusic].DifficultNormal / maxDifficultValue;
                    break;
                }
            case 1:
                {
                    difficultSlider.fillAmount = pathManager.pathList[mainSelectMusic].DifficultHard / maxDifficultValue;
                    break;
                }
            case 2:
                {
                    difficultSlider.fillAmount = pathManager.pathList[mainSelectMusic].DifficultMax / maxDifficultValue ;
                    break;
                }
            case 3:
                {
                    difficultSlider.fillAmount = pathManager.pathList[mainSelectMusic].DifficultYH / maxDifficultValue  ;
                    break;
                }
        }
    }

    private void ControllButton()
    {
        if(!NetworkManager.Instance.isRoomOwner) return;
        
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
        //playButtonAnim.SetBool("isEnable", false);
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
        //if (buttonIndex == 4) playButtonAnim.SetBool("isEnable", true);
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
            //musicInfoAlbumCover.texture = albumCovers[musicIndex];
            //musicInfoTitle.text = pathManager.pathList[musicIndex].MusicName;
            //musicInfoProducer.text = pathManager.pathList[musicIndex].MainComposer;
            ChangeVideoTimer();
        }
    }
    #endregion

    #region System Methods
    int CompareByMusicName(MusicPathObj x, MusicPathObj y)
    {
        return string.Compare(x.MusicName, y.MusicName, System.StringComparison.Ordinal);
    }
    IEnumerator Co_IngameSceneLoad(string SceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(SceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
    #endregion

    #region Datas Load
    private IEnumerator<AsyncOperationHandle> LoadAssetsCoroutine()
    {
        pathManager.pathList.Sort(CompareByMusicName);
        for (int i = 0; i < pathManager.pathList.Count; i++)
        {
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
    }
    #endregion

}