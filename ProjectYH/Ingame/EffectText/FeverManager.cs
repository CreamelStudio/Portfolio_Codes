using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class FeverManager : MonoBehaviour
{
    public static FeverManager instance { get; private set; }

    public float feverValue;
    public int feverMax;

    public bool isFever;

    public Slider slider;
    public RawImage feverOutline;
    public Animator feverText;

    public GameObject feverPrefab;
    public Transform feverSpawnPoint;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        ResetFever();
        InitFeverUI();
    }

    public void ResetFever()
    {
        slider.gameObject.SetActive(false);
        feverValue = 0;
        InitFeverUI();
        StopCoroutine(Co_Fever());
        isFever = false;
        feverText.SetBool("isFever", false);

        feverOutline.transform.localScale = new Vector3(1.2f, 1.2f, 1);
    }

    public void AddFeverValue(float value)
    {
        slider.gameObject.SetActive(true);
        feverValue += value;
        Debug.Log(value);
        InitFeverUI();
        if (feverValue >= feverMax)
        {
            feverValue = 0;
            InitFeverUI();
            SpawnFever();
        }
    }

    public void SpawnFever()
    {
        Destroy(Instantiate(feverPrefab, feverSpawnPoint), 3f);
        StopCoroutine(Co_Fever());
        isFever = true;
        StartCoroutine(Co_Fever());
    }

    public void InitFeverUI()
    {
        slider.maxValue = feverMax;
        slider.value = feverValue;
    }

    IEnumerator Co_Fever()
    {
        feverOutline.transform.DOScale(new Vector3(1.05f, 1.07f, 1), 0.3f);
        feverText.SetBool("isFever", true);
        yield return new WaitForSeconds(13f);
        feverText.SetBool("isFever", false);
        feverOutline.transform.DOScale(new Vector3(1.2f, 1.2f, 1), 0.3f);
        isFever = false;
    }
}
