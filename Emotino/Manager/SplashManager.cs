using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class SplitManager : MonoBehaviour
{
    public float delay;
    public RawImage title;

    private void Start()
    {
        title.DOFade(1, 1f).OnComplete(() =>
        {
            title.DOFade(0, 1f).OnComplete(() =>
            {
                SceneManager.LoadScene(Scenes.Title);
            });
        });
    }
}