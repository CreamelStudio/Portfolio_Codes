using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;
using UnityEngine.SceneManagement;
public class ChepterUIManager : MonoBehaviour
{
    public static ChepterUIManager instance;

    public Chepter chepter;
    public int page;
    public TMP_Text[] stageTexts;
    public Book book;

    public int nowClearStageIndex = 0;

    public EventTrigger[] buttonTriggers;

    public GameObject chepterObj;

    IEnumerator enableCoroutine;
    IEnumerator disableCoroutine;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this.gameObject);

            nowClearStageIndex = PlayerPrefs.GetInt(chepter.chepterName + "ClearCount", 1);

        for (int i = 0; i < buttonTriggers.Length; i++)
        {
            int index = i; // Capture the current index
            EventTrigger.Entry entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerClick
            };
            entry.callback.AddListener((data) => OnButtonClick(index));
            buttonTriggers[i].triggers.Add(entry);
        }
    }

    private void Update()
    {
        if(book.pageDragging)
        {
            disableCoroutine = Co_DisableObj(0f);
            StartCoroutine(disableCoroutine);
        }
    }

    public void OnButtonClick(int index)
    {
        Debug.Log("Click Stage : " + index);

        if(chepter.stages[index] == null) return;

        StoryManager.instance.nowStage = chepter.stages[index];
        StoryManager.instance.nowDialogIndex = 0;
        SceneManager.LoadScene("StoryNormalStage");
    }

    public void StopCoroutines(IEnumerator corutine)
    {
        StopCoroutine(corutine);
    }

    public void OnPageDragEnd()
    {
        enableCoroutine = Co_EnableObj(0.05f);
        disableCoroutine = Co_DisableObj(0.01f);

        if (book.currentPage == page) StartCoroutine(enableCoroutine);
        else StartCoroutine(disableCoroutine);
    }

    IEnumerator Co_EnableObj(float time)
    {
        yield return new WaitForSeconds(time);
        if (!book.pageDragging && book.currentPage == page) StopCoroutines(enableCoroutine);

        chepterObj.SetActive(true);

        for(int i = 0; i < stageTexts.Length; i++)
        {
            stageTexts[i].transform.localPosition = new Vector3(0, -70, 0);
            buttonTriggers[i].gameObject.SetActive(false);
        }
        for (int i = 0; i < nowClearStageIndex; i++)
        {
            buttonTriggers[i].gameObject.SetActive(true);
            stageTexts[i].transform.DOLocalMove(new Vector3(0, 0, 0), 0.3f);
        }
    }

    IEnumerator Co_DisableObj(float time)
    {
        yield return new WaitForSeconds(time);
        chepterObj.SetActive(false);

        for (int i = 0; i < stageTexts.Length; i++)
        {
            stageTexts[i].transform.localPosition = new Vector3(0, -70, 0);
            buttonTriggers[i].gameObject.SetActive(false);
        }
    }
}

