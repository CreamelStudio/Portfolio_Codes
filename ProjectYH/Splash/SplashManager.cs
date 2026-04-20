using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class SplitManager : MonoBehaviour
{
    public float delay;
    public GameObject[] Logos;

    public int splash;

    [Header("Video")]
    public RawImage fade;
    public VideoPlayer vp;
    private bool isPlaying = false;

    private int isFirstBoot;
    private string sceneName;

    void Start()
    {
        isFirstBoot = PlayerPrefs.GetInt("isFirstBoot", 0);
        sceneName = (isFirstBoot == 0) ? "Tutorial" : "MenuSelect";

        StartCoroutine(Co_Splash());
    }

    private void Update()
    {
        if (splash != Logos.Length - 1 && Input.anyKeyDown && Input.anyKeyDown && !Input.GetMouseButtonDown(0) && !Input.GetMouseButtonDown(1) && !Input.GetKeyDown(KeyCode.Tab) && !Input.GetKeyDown(KeyCode.LeftAlt) && !Input.GetKeyDown(KeyCode.Escape) && splash != 0)
        {
            if(splash == Logos.Length - 1) SceneManager.LoadScene(sceneName);
            Debug.Log("ToNext");
            Logos[splash].SetActive(false);
            StopAllCoroutines();
            splash++;
            StartCoroutine(Co_Splash());
        }

        if(isPlaying && vp.isPlaying && Input.anyKeyDown && Input.anyKeyDown && !Input.GetMouseButtonDown(0) && !Input.GetMouseButtonDown(1) && !Input.GetKeyDown(KeyCode.Tab) && !Input.GetKeyDown(KeyCode.LeftAlt) && !Input.GetKeyDown(KeyCode.Escape))
        {
            fade.DOFade(1, 0.2f).OnComplete(() => SceneManager.LoadScene(sceneName));
        }

        if(isPlaying && !vp.isPlaying)
        {
            Debug.Log($"{isPlaying} | {vp.isPlaying}");
            fade.DOFade(1, 0.2f).OnComplete(()=>SceneManager.LoadScene(sceneName));
        }
    }

    IEnumerator Co_Splash()
    {
        
        if (splash == Logos.Length - 1)
        {
            Logos[splash].SetActive(true);
            vp.Play();
            Debug.Log("Video Sound Start");
            yield return new WaitForSeconds(0.1f);
            isPlaying = true;

            Debug.Log("isPlay!");
            StopAllCoroutines();
        }
        else if(splash == Logos.Length)
        {
            StopAllCoroutines();
        }
        else
        {
            Logos[splash].SetActive(true);
        }
        
        yield return new WaitForSeconds(delay);
        Logos[splash].SetActive(false);
        splash++;
        StartCoroutine(Co_Splash());
    }
}