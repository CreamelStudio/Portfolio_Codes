using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public enum JudgeEnum
{
    Perfect,
    Great,
    Good,
    Bad,
    Miss
}

public enum DetectNoteEnum
{
    Default,
    Double,
    LongStart,
    LongIng,
    LongEnd,
}

public class IngameInput : MonoBehaviour
{
    public static IngameInput instance;

    [SerializeField] private EventReference keyBombRef;
    [SerializeField] private EventInstance keyBombInstance;
    private NoteClick inputActions;

    private int noteClickCount;
    private int longNoteClickCount;
    private int longNoteEndClickCount;
    private int doubleNoteClickCount;
    private int defaultNoteClickCount;

    private bool isDouble;

    public GameObject[] keyviews;
    public RawImage colorViewer;
    public bool[] isPressKey = new bool[4];
    public bool[] isLongNoteCheck = new bool[4];

    [SerializeField] private float perfectDiff;
    [SerializeField] private float greatDiff;
    [SerializeField] private float goodDiff;
    [SerializeField] private float badDiff;
    [SerializeField] private float missDiff;

    [Space(10f)]
    private GameObject judgeObject;
    [SerializeField] private Transform judgeSpawnTrans;
    private GameObject comboObject;
    [SerializeField] private Transform comboSpawnTrans;

    [SerializeField] private GameObject perfectJudgePrefab;
    [SerializeField] private GameObject greatJudgePrefab;
    [SerializeField] private GameObject goodJudgePrefab;
    [SerializeField] private GameObject badJudgePrefab;
    [SerializeField] private GameObject missJudgePrefab;

    [SerializeField] private GameObject comboTitlePrefab;

    [SerializeField] private TMP_Text rateText;

    private int longNoteCount;

    [SerializeField] private SpriteRenderer particleNote;
    [SerializeField] private Transform mainObletPosition;
    
    [Space(10f)]
    public int combo;
    public int score;

    public float judgeCount;
    public float judgeCal;
    public float rate;

    public TMP_Text scoreText;

    private System.Action<InputAction.CallbackContext> note1In;
    private System.Action<InputAction.CallbackContext> note1Out;
    private System.Action<InputAction.CallbackContext> note2In;
    private System.Action<InputAction.CallbackContext> note2Out;
    private System.Action<InputAction.CallbackContext> note3In;
    private System.Action<InputAction.CallbackContext> note3Out;
    private System.Action<InputAction.CallbackContext> note4In;
    private System.Action<InputAction.CallbackContext> note4Out;

    public int noteSplinkleCount = 0;
    public bool isLongnoteSplinkle = false;

    private TMP_Text ComboValueText
    {
        get
        {
            if (comboObject == null)
            {
                return null;
            }

            TMP_Text[] texts = comboObject.GetComponentsInChildren<TMP_Text>(true);
            if (texts.Length == 0)
            {
                return null;
            }

            return texts.Length > 1 ? texts[1] : texts[0];
        }
    }

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this.gameObject);

        inputActions = new NoteClick();
        inputActions.Enable();

        keyBombInstance = RuntimeManager.CreateInstance(keyBombRef);

        InputSystem.pollingFrequency = 1000f;

        note1In = ctx =>  PerformedKey(0);
        note1Out = ctx => CancelKey(0);

        note2In = ctx =>  PerformedKey(1);
        note2Out = ctx => CancelKey(1);

        note3In = ctx =>  PerformedKey(2);
        note3Out = ctx => CancelKey(2);

        note4In = ctx =>  PerformedKey(3);
        note4Out = ctx => CancelKey(3);

        inputActions.NoteClickMap.NoteClick1.performed += note1In;
        inputActions.NoteClickMap.NoteClick1.canceled += note1Out;

        inputActions.NoteClickMap.NoteClick2.performed += note2In;
        inputActions.NoteClickMap.NoteClick2.canceled += note2Out;

        inputActions.NoteClickMap.NoteClick3.performed += note3In;
        inputActions.NoteClickMap.NoteClick3.canceled += note3Out;

        inputActions.NoteClickMap.NoteClick4.performed += note4In;
        inputActions.NoteClickMap.NoteClick4.canceled += note4Out;

    }

    private void Update()
    {
        CheckMissNote();
    }

    /// <summary>
    /// 키를 눌렀을 때 판정 처리를 진행합니다
    /// </summary>
    /// <param name="index">눌린 키의 인덱스</param>
    private void PerformedKey(int index)
    {
        keyBombInstance.start(); // 키 입력 시 키밤 사운드 재생
        
        particleNote.DOFade(0.7f, 0.2f); // 노트 주변 파티클을 서서히 나타나게 함

        // 현재 입력 시점을 ms 단위로 변환
        float position = (float)NoteSpawnManager.instance.getDspTime() * 1000;

        // 노트 판정 처리
        // 1. 기본 노트 판정 시도 (만약 일반노트가 아니면 실패처리)
        // 2. 실패하면 롱노트 시작 판정 시도 (롱노트도 아니면 실패처리)
        // 3. 그것도 실패하면 더블노트 판정 시도 (더블노트가 아니면 실패처리)
        if (!InputDefaultNote(position))
            if (!InputLongNote(index, position))
                InputDoubleNote(position, true);

        isPressKey[index] = true; // 해당 인덱스의 키가 눌린 상태로 변경
        keyviews[index].SetActive(true); // 키 눌림 시각 효과 활성화
    }

    /// <summary>
    /// 키를 뗐을 때 판정 처리를 진행합니다
    /// </summary>
    /// <param name="index">뗀 키의 인덱스</param>
    private void CancelKey(int index)
    {
        particleNote.DOFade(0f, 0.2f); // 파티클을 서서히 사라지게 함

        // 현재 입력 해제 시점을 ms 단위로 변환
        float position = (float)NoteSpawnManager.instance.getDspTime() * 1000;

        InputLongNoteEnd(index, position); // 롱노트 종료(떼기) 판정 처리
        InputDoubleNote(position, false); // 더블노트 떼기 판정 처리

        isPressKey[index] = false; // 해당 키가 눌리지 않은 상태로 변경
        keyviews[index].SetActive(false); // 키 눌림 시각 효과 비활성화
    }

    private void FixedUpdate()
    {
        int totalHandled = defaultNoteClickCount + longNoteClickCount + doubleNoteClickCount;
        if (noteClickCount != totalHandled)
        {
            Debug.LogWarning($"[Mismatch] noteClickCount({noteClickCount}) != sum({totalHandled})");
            noteClickCount = totalHandled;
        }

        CheckLongNote();
    }

    private bool InputDoubleNote(float position, bool isDown)
    {
        if (doubleNoteClickCount >= PlayDataManager.instance.doubleNote.Count) return false;
        if (noteClickCount >= GameManager.instance.currentLevel.MinuteNotes.Count) return false;

        if (!isDown)
        {
            isDouble = false;
            return false;
        }

        if (isDown && !isDouble)
        {
            isDouble = true;
            return false; // <- 여기서 true로 리턴하지 말고 그냥 대기
        }

        BaseNote allNote = GameManager.instance.currentLevel.MinuteNotes[noteClickCount];
        if (allNote.NoteType != NoteEnum.Double) return false;

        BaseNote note = PlayDataManager.instance.doubleNote[doubleNoteClickCount];
        float diffTime = Mathf.Abs((position / 1000f) - (float)note.StartDelay);

        JudgeEnum? judge = null;
        if (diffTime < perfectDiff / 1000f) judge = JudgeEnum.Perfect;
        else if (diffTime < greatDiff / 1000f) judge = JudgeEnum.Great;
        else if (diffTime < goodDiff / 1000f) judge = JudgeEnum.Good;
        else if (diffTime < badDiff / 1000f) judge = JudgeEnum.Bad;
        else if (diffTime < missDiff / 1000f) judge = JudgeEnum.Miss;

        if (judge.HasValue)
        {
            particleNote.transform.position = NoteSpawnManager.instance.doubleNoteObjects[doubleNoteClickCount].transform.position;
            particleNote.transform.rotation = NoteSpawnManager.instance.doubleNoteObjects[doubleNoteClickCount].transform.rotation;
            Destroy(NoteSpawnManager.instance.doubleNoteObjects[doubleNoteClickCount].gameObject);


            doubleNoteClickCount++;
            noteClickCount++;
            CheckJudge(judge.Value, DetectNoteEnum.Double, 1.4f);
            isDouble = false;
            return true;
        }

        return false;
    }









    private bool InputDefaultNote(float position)
    {
        if (noteClickCount >= GameManager.instance.currentLevel.MinuteNotes.Count) return false;
        if (defaultNoteClickCount >= PlayDataManager.instance.defaultNote.Count) return false;
        if (noteClickCount >= GameManager.instance.currentLevel.MinuteNotes.Count) return false;

        BaseNote allNote = GameManager.instance.currentLevel.MinuteNotes[noteClickCount];
        if (allNote.NoteType != NoteEnum.Default) return false;

        BaseNote note = PlayDataManager.instance.defaultNote[defaultNoteClickCount];
        float diffTime = Mathf.Abs(((float)position / 1000f) - (float)note.StartDelay);

        JudgeEnum? judge = null;

        if (diffTime < perfectDiff / 1000f) judge = JudgeEnum.Perfect;
        else if (diffTime < greatDiff / 1000f) judge = JudgeEnum.Great;
        else if (diffTime < goodDiff / 1000f) judge = JudgeEnum.Good;
        else if (diffTime < badDiff / 1000f) judge = JudgeEnum.Bad;
        else if (diffTime < missDiff / 1000f) judge = JudgeEnum.Miss;

        if (judge.HasValue)
        {
            particleNote.transform.position = NoteSpawnManager.instance.defaultNoteObjects[defaultNoteClickCount].transform.position;
            particleNote.transform.rotation = NoteSpawnManager.instance.defaultNoteObjects[defaultNoteClickCount].transform.rotation;
            Destroy(NoteSpawnManager.instance.defaultNoteObjects[defaultNoteClickCount].gameObject);
            defaultNoteClickCount++;
            noteClickCount++;

            CheckJudge(judge.Value, DetectNoteEnum.Default, 1);
            return true;
        }

        return false;
    }










    private bool InputLongNote(int keyCode, float position)
    {
        if (longNoteClickCount >= PlayDataManager.instance.longNote.Count || longNoteClickCount >= NoteSpawnManager.instance.minuteNoteSpawner.longNoteRenderer.Count ||longNoteClickCount >= NoteSpawnManager.instance.longNoteObjects.Count) return false;

        BaseNote note = PlayDataManager.instance.longNote[longNoteClickCount];

        if (note.NoteType != NoteEnum.Long)
            return false;

        float diffTime = Mathf.Abs(((float)position / 1000f) - (float)note.StartDelay);

        JudgeEnum? judge = null;

        if (diffTime < perfectDiff / 1000f) judge = JudgeEnum.Perfect;
        else if (diffTime < greatDiff / 1000f) judge = JudgeEnum.Great;
        else if (diffTime < goodDiff / 1000f) judge = JudgeEnum.Good;
        else if (diffTime < badDiff / 1000f) judge = JudgeEnum.Bad;
        else if (diffTime < missDiff / 1000f) judge = JudgeEnum.Miss;

        if (judge.HasValue)
        {
            particleNote.transform.position = NoteSpawnManager.instance.longNoteObjects[longNoteClickCount].transform.position;
            particleNote.transform.rotation = NoteSpawnManager.instance.longNoteObjects[longNoteClickCount].transform.rotation;
            
            NoteSpawnManager.instance.minuteNoteSpawner.longNoteRenderer[longNoteClickCount].trail.time = NoteSpawnManager.instance.startOffset;
            Destroy(NoteSpawnManager.instance.longNoteObjects[longNoteClickCount].gameObject);
            
            longNoteClickCount++;
            noteClickCount++;
            
            isLongNoteCheck[keyCode] = true;
            CheckJudge(judge.Value, DetectNoteEnum.LongStart, 1);

            return true;
        }
        return false;
    }








    private void InputLongNoteEnd(int keyCode, float position)
    {
        if (longNoteClickCount <= 0 || longNoteClickCount - 1 >= PlayDataManager.instance.longNote.Count) return;
        if (!isLongNoteCheck[keyCode]) return;
        if (longNoteClickCount - 1 >= NoteSpawnManager.instance.minuteNoteSpawner.longNoteRenderer.Count) return;
        if (longNoteEndClickCount >= NoteSpawnManager.instance.longNoteEndObjects.Count) return;
        LongNote note = PlayDataManager.instance.longNote[longNoteClickCount - 1];
        if (note.NoteType != NoteEnum.Long) return;

        Debug.Log("Click Long Note End");

        float diffTime = Mathf.Abs((float)position / 1000f - ((float)note.StartDelay + (float)note.EndDelay));

        JudgeEnum? judge = null;
        if (diffTime < (perfectDiff + 15f) / 1000f) judge = JudgeEnum.Perfect;
        else if (diffTime < (greatDiff + 15f) / 1000f) judge = JudgeEnum.Great;
        else if (diffTime < (goodDiff + 15f) / 1000f) judge = JudgeEnum.Good;
        else if (diffTime < (badDiff + 15f) / 1000f) judge = JudgeEnum.Bad;
        else if (diffTime < (missDiff + 15f) / 1000f) judge = JudgeEnum.Miss;

        if (judge.HasValue)
        {
            for (int i = 0; i < 4; i++) isLongNoteCheck[i] = false;
            int clickCount = longNoteEndClickCount;
            longNoteEndClickCount++;
            particleNote.transform.position = NoteSpawnManager.instance.longNoteEndObjects[clickCount].transform.position;
            particleNote.transform.rotation = NoteSpawnManager.instance.longNoteEndObjects[clickCount].transform.rotation;
            Destroy(NoteSpawnManager.instance.minuteNoteSpawner.longNoteRenderer[clickCount].gameObject);
            Destroy(NoteSpawnManager.instance.longNoteEndObjects[clickCount]);
            CheckJudge(judge.Value, DetectNoteEnum.LongEnd, 1);
        }
    }










    private void CheckMissNote()
    {
        float clickTime = (float)NoteSpawnManager.instance.getDspTime();

        if (noteClickCount < GameManager.instance.currentLevel.MinuteNotes.Count)
        {
            float diffTime = clickTime - (float)GameManager.instance.currentLevel.MinuteNotes[noteClickCount].StartDelay;

            if (diffTime > missDiff / 1000)
            {
                if (GameManager.instance.currentLevel.MinuteNotes[noteClickCount].NoteType == NoteEnum.Long)
                {
                    if (longNoteClickCount < NoteSpawnManager.instance.longNoteObjects.Count)
                    {
                        particleNote.transform.position = NoteSpawnManager.instance.longNoteObjects[longNoteClickCount].transform.position;
                        particleNote.transform.rotation = NoteSpawnManager.instance.longNoteObjects[longNoteClickCount].transform.rotation;
                        Destroy(NoteSpawnManager.instance.longNoteObjects[longNoteClickCount].gameObject);
                        CheckJudge(JudgeEnum.Miss, DetectNoteEnum.LongStart, 1);
                        longNoteClickCount++;
                        noteClickCount++;
                    }
                    else
                    {
                        Debug.LogWarning($"[MissCheck] 롱노트 index {longNoteClickCount} 접근 실패. 리스트 Count: {NoteSpawnManager.instance.longNoteObjects.Count}");
                    }
                }

                if (noteClickCount < GameManager.instance.currentLevel.MinuteNotes.Count && defaultNoteClickCount < NoteSpawnManager.instance.defaultNoteObjects.Count)
                {
                    if (GameManager.instance.currentLevel.MinuteNotes[noteClickCount].NoteType == NoteEnum.Default)
                    {
                        particleNote.transform.position = NoteSpawnManager.instance.defaultNoteObjects[defaultNoteClickCount].transform.position;
                        particleNote.transform.rotation = NoteSpawnManager.instance.defaultNoteObjects[defaultNoteClickCount].transform.rotation;
                        Destroy(NoteSpawnManager.instance.defaultNoteObjects[defaultNoteClickCount].gameObject);
                        CheckJudge(JudgeEnum.Miss, DetectNoteEnum.Default, 1);
                        defaultNoteClickCount++;
                        noteClickCount++;
                    }
                }
                
                
            }
        }

        if (longNoteEndClickCount < longNoteClickCount && longNoteEndClickCount < PlayDataManager.instance.longNote.Count && longNoteEndClickCount < NoteSpawnManager.instance.longNoteEndObjects.Count && longNoteEndClickCount < NoteSpawnManager.instance.minuteNoteSpawner.longNoteRenderer.Count)
        {
            float endLongNote = (float)PlayDataManager.instance.longNote[longNoteEndClickCount].EndDelay +
                                (float)PlayDataManager.instance.longNote[longNoteEndClickCount].StartDelay;
            float diffTime = clickTime - endLongNote;
            if (diffTime > (missDiff + 20) / 1000)
            {
                Destroy(NoteSpawnManager.instance.longNoteEndObjects[longNoteEndClickCount].gameObject);
                Destroy(NoteSpawnManager.instance.minuteNoteSpawner.longNoteRenderer[longNoteEndClickCount].gameObject);
                for (int i = 0; i < 4; i++) isLongNoteCheck[i] = false;
                Debug.Log("Miss Long Note End");
                longNoteEndClickCount++;
                CheckJudge(JudgeEnum.Miss, DetectNoteEnum.LongEnd, 1);
            }
        }

        if (doubleNoteClickCount < PlayDataManager.instance.doubleNote.Count && doubleNoteClickCount < NoteSpawnManager.instance.doubleNoteObjects.Count)
        {
            float spawnTime = (float)PlayDataManager.instance.doubleNote[doubleNoteClickCount].StartDelay;
            float diffTime = clickTime - spawnTime;
            if (diffTime > missDiff / 1000f)
            {
                Destroy(NoteSpawnManager.instance.doubleNoteObjects[doubleNoteClickCount].gameObject);
                CheckJudge(JudgeEnum.Miss, DetectNoteEnum.Double, 1f);
                Debug.Log("Miss Double Note");
                doubleNoteClickCount++;
                noteClickCount++;
            }
        }
    }









    private void CheckLongNote()
    {
        for(int i = 0; i < 4; i++)
        {
            if (isLongNoteCheck[i]) //롱노트 상태에 들어가 있는가?
            {
                if (!isPressKey[i]) //키를 안눌렀는가?
                {
                    NoteSpawnManager.instance.minuteNoteSpawner.longNoteRenderer[longNoteClickCount - 1].trail.time = NoteSpawnManager.instance.startOffset;
                    colorViewer.color = Color.grey;
                    isLongNoteCheck[i] = false;
                }
                else
                {
                    longNoteCount++; 
                    if(longNoteCount % 5 == 0)
                    {
                        Debug.Log("롱노트 유지 판정");
                        colorViewer.color = Color.yellow;
                        CheckJudge(JudgeEnum.Perfect, DetectNoteEnum.LongIng, 0.4f); //롱노트 유지 판정
                    }
                    particleNote.transform.position = mainObletPosition.transform.position;
                    particleNote.transform.rotation = NoteSpawnManager.instance.minuteNiddle.transform.rotation;
                }
            }
        }
    }










    private void CheckJudge(JudgeEnum judge, DetectNoteEnum detect, float ratio)
    {
        switch (detect)
        {
            case DetectNoteEnum.Default:
                if (!isLongnoteSplinkle) noteSplinkleCount = 1;
                else noteSplinkleCount++;
                    break;
            case DetectNoteEnum.Double:
                if (!isLongnoteSplinkle) noteSplinkleCount = 2;
                else noteSplinkleCount += 2;
                break;
            case DetectNoteEnum.LongStart:
                isLongnoteSplinkle = true;
                noteSplinkleCount = 1;
            break;
            case DetectNoteEnum.LongIng:
                noteSplinkleCount++;
            break;
            case DetectNoteEnum.LongEnd:
                isLongnoteSplinkle = false;
                noteSplinkleCount = 0;
            break;
        }

        switch (judge)
        {
            case JudgeEnum.Perfect:
                colorViewer.color = new Color(1, 1, 0);
                if (judgeObject != null) Destroy(judgeObject);
                judgeObject = Instantiate(perfectJudgePrefab, judgeSpawnTrans);
                judgeObject.GetComponent<JudgeRotation>().InitSplinkle(noteSplinkleCount);
                FeverManager.instance.AddFeverValue(2 * ratio);
                NoteSpawnManager.instance.perfectCount++;
                combo++;
                if (FeverManager.instance.isFever) judgeCal += 120 * ratio;
                else judgeCal += 100 * ratio;
                break;
            case JudgeEnum.Great:
                colorViewer.color = new Color(0, 1, 0);
                if (judgeObject != null) Destroy(judgeObject);
                judgeObject = Instantiate(greatJudgePrefab, judgeSpawnTrans);
                judgeObject.GetComponent<JudgeRotation>().InitSplinkle(noteSplinkleCount);
                FeverManager.instance.AddFeverValue(1 * ratio);
                NoteSpawnManager.instance.greatCount++;
                combo++;
                if (FeverManager.instance.isFever) judgeCal += 96 * ratio;
                else judgeCal += 80 * ratio;
                break;
            case JudgeEnum.Good:
                colorViewer.color = new Color(0, 0, 1);
                if (judgeObject != null) Destroy(judgeObject);
                judgeObject = Instantiate(goodJudgePrefab, judgeSpawnTrans);
                judgeObject.GetComponent<JudgeRotation>().InitSplinkle(noteSplinkleCount);
                FeverManager.instance.AddFeverValue(0.7f * ratio);
                NoteSpawnManager.instance.goodCount++;
                combo++;
                if (FeverManager.instance.isFever) judgeCal += 72 * ratio;
                else judgeCal += 60 * ratio;
                break;
            case JudgeEnum.Bad:
                colorViewer.color = new Color(1, 0, 0);
                if (judgeObject != null) Destroy(judgeObject);
                judgeObject = Instantiate(badJudgePrefab, judgeSpawnTrans);
                judgeObject.GetComponent<JudgeRotation>().InitSplinkle(noteSplinkleCount);
                FeverManager.instance.AddFeverValue(0.2f * ratio);
                NoteSpawnManager.instance.badCount++;
                combo = 0;
                if (FeverManager.instance.isFever) judgeCal += 42 * ratio;
                else judgeCal += 35 * ratio;
                break;
            case JudgeEnum.Miss:
                colorViewer.color = new Color(0.2f, 0.2f, 0.2f);
                if (judgeObject != null) Destroy(judgeObject);
                judgeObject = Instantiate(missJudgePrefab, judgeSpawnTrans);
                FeverManager.instance.ResetFever();
                NoteSpawnManager.instance.missCount++;
                combo = 0;
                if (FeverManager.instance.isFever) judgeCal += 12 * ratio;
                else judgeCal += 0 * ratio;
                break;
        }

        float countOffset = FeverManager.instance.isFever ? ratio * 1.2f : ratio;
        judgeCount += countOffset;

        rate = Mathf.Round(judgeCal / judgeCount * 100f) / 100;
        rateText.text = $"{rate}%";
        int score = Mathf.RoundToInt(judgeCal * 1.3f);

        NoteSpawnManager.instance.rate = rate;
        scoreText.text = $"{score}";

        if (comboObject != null) Destroy(comboObject);
        if (combo != 0)
        {
            comboObject = Instantiate(comboTitlePrefab, comboSpawnTrans);
            TMP_Text comboValueText = ComboValueText;
            if (comboValueText != null)
            {
                comboValueText.text = $"{combo}";
            }
            if(NoteSpawnManager.instance.maxCombo < combo) NoteSpawnManager.instance.maxCombo = combo;
        }
    }

    private void OnDestroy()
    {
        if (inputActions != null)
        {
            inputActions.NoteClickMap.NoteClick1.performed -= note1In;
            inputActions.NoteClickMap.NoteClick1.canceled -= note1Out;
            inputActions.NoteClickMap.NoteClick2.performed -= note2In;
            inputActions.NoteClickMap.NoteClick2.canceled -= note2Out;
            inputActions.NoteClickMap.NoteClick3.performed -= note3In;
            inputActions.NoteClickMap.NoteClick3.canceled -= note3Out;
            inputActions.NoteClickMap.NoteClick4.performed -= note4In;
            inputActions.NoteClickMap.NoteClick4.canceled -= note4Out;

            inputActions.Disable();
        }
    }
}
