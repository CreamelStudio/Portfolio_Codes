using FMOD.Studio;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class OffsetNote : MonoBehaviour
{
    public int expectedHitTimeMs;
}

public class OffsetSetManager : MonoBehaviour
{
    public EventReference offsetRef;
    public EventInstance offsetInstance;

    public TMP_Text offsetText;

    public GameObject notePrefab;
    public Transform noteSpawnPosition;

    public List<GameObject> noteObjs = new List<GameObject>();
    public List<GameObject> visualObj = new List<GameObject>();
    public List<int> positionList = new List<int>();

    public float offsetValue = 0f;
    public float totalOffset = 0f;
    public float totalOffsetCount = 0f;

    public bool isStart;
    public int noteCount;

    public NoteClick inputActions;

    [Header("Note Move")]
    public float noteSpeed = 15f;
    public float missX = 12f;

    [Header("Timing")]
    public float spawnInterval = 1f;

    // 중요: 노트 생성 위치 -> 판정선까지 걸리는 시간(ms)
    // 네 게임에 맞게 반드시 측정해서 넣어야 함
    public int travelTimeMs = 1000;

    private Coroutine startRoutine;
    private Coroutine spawnRoutine;

    private void Start()
    {
        inputActions = new NoteClick();
        inputActions.Enable();

        InputSystem.pollingFrequency = 1000f;

        inputActions.NoteClickMap.NoteClick1.performed += _ => DeleteNote(true);
        inputActions.NoteClickMap.NoteClick2.performed += _ => DeleteNote(true);
        inputActions.NoteClickMap.NoteClick3.performed += _ => DeleteNote(true);
        inputActions.NoteClickMap.NoteClick4.performed += _ => DeleteNote(true);

        BeginSession();
    }

    private void OnDestroy()
    {
        if (inputActions != null)
            inputActions.Disable();

        StopCurrentSession();
    }

    private void BeginSession()
    {
        StopCurrentSession();
        ClearAllNotes();
        ResetValuesOnly();

        startRoutine = StartCoroutine(Co_StartOffset());
    }

    private void StopCurrentSession()
    {
        isStart = false;

        if (startRoutine != null)
        {
            StopCoroutine(startRoutine);
            startRoutine = null;
        }

        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }

        offsetInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        offsetInstance.release();
    }

    private void ResetValuesOnly()
    {
        offsetValue = 0f;
        totalOffset = 0f;
        totalOffsetCount = 0f;
        noteCount = 0;
        positionList.Clear();
        offsetText.text = "Offset : 0";
    }

    private void ClearAllNotes()
    {
        foreach (var note in noteObjs)
        {
            if (note != null)
                Destroy(note);
        }

        foreach (var note in visualObj)
        {
            if (note != null)
                Destroy(note);
        }

        noteObjs.Clear();
        visualObj.Clear();
    }

    IEnumerator Co_StartOffset()
    {
        offsetInstance = RuntimeManager.CreateInstance(offsetRef);

        offsetText.text = "3";
        yield return new WaitForSeconds(1f);
        offsetText.text = "2";
        yield return new WaitForSeconds(1f);
        offsetText.text = "1";
        yield return new WaitForSeconds(1f);
        offsetText.text = "Start!";
        yield return new WaitForSeconds(1f);

        offsetInstance.start();
        isStart = true;
        offsetText.text = "Offset : 0";

        spawnRoutine = StartCoroutine(Co_SpawnNotes());
    }

    IEnumerator Co_SpawnNotes()
    {
        int spawnIndex = 0;

        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            offsetInstance.getTimelinePosition(out int currentTimelineMs);

            GameObject obj = Instantiate(notePrefab, noteSpawnPosition.position, Quaternion.identity);
            var noteData = obj.GetComponent<OffsetNote>();
            if (noteData == null)
                noteData = obj.AddComponent<OffsetNote>();

            // "지금 생성된 노트가 travelTimeMs 뒤에 판정선 도착"
            noteData.expectedHitTimeMs = currentTimelineMs + travelTimeMs;

            noteObjs.Add(obj);
            visualObj.Add(obj);

            spawnIndex++;
        }
    }

    private void Update()
    {
        for (int i = 0; i < noteObjs.Count; i++)
        {
            GameObject noteObj = noteObjs[i];
            if (noteObj == null) continue;

            noteObj.transform.position += Vector3.right * Time.deltaTime * noteSpeed;

            if (i == noteCount && noteObj.transform.position.x > missX)
            {
                DeleteNote(false);
            }
        }
    }

    public void ResetOffset()
    {
        BeginSession();
    }

    public void EndOffset()
    {
        PlayerPrefs.SetFloat("OffsetValue", offsetValue);
        GameManager.instance.soundOffset = offsetValue;
        PlayerPrefs.Save();

        StopCurrentSession();
        SceneManager.LoadScene("MenuSelect");
    }

    void DeleteNote(bool isEffect)
    {
        if (!isStart) return;
        if (noteCount < 0 || noteCount >= noteObjs.Count) return;

        GameObject currentNote = noteObjs[noteCount];
        if (currentNote == null) return;

        if (!isEffect)
        {
            Destroy(currentNote);
            noteObjs[noteCount] = null;
            noteCount++;
            return;
        }

        offsetInstance.getTimelinePosition(out int position);

        var noteData = currentNote.GetComponent<OffsetNote>();
        if (noteData == null)
        {
            Debug.LogError("OffsetNote component missing.");
            return;
        }

        int diff = position - noteData.expectedHitTimeMs;

        positionList.Add(position);
        totalOffset += diff;
        totalOffsetCount++;

        offsetValue = totalOffset / totalOffsetCount;
        offsetText.text = $"Offset : {offsetValue:0}";

        var sr = currentNote.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = new Color(1f, 1f, 1f, 0.1f);

        Destroy(currentNote);
        noteObjs[noteCount] = null;
        noteCount++;
    }
}