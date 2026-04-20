using DG.Tweening;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StoryModeChanger : MonoBehaviour
{
    public CanvasGroup titleCanvas;
    public CanvasGroup storySelectCanvas;

    public RawImage transitionImage;

    public bool isSelectMode = false;

    private void Awake()
    {
        transitionImage.gameObject.SetActive(true);
        transitionImage.DOFade(0f, 0.3f);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && isSelectMode)
        {
            FadeToMenu();
        }
    }

    public void FadeToSelect()
    {
        isSelectMode = true;
        titleCanvas.DOFade(0, 0.3f).OnComplete(() =>
        {
            titleCanvas.gameObject.SetActive(false);
            storySelectCanvas.gameObject.SetActive(true);
            storySelectCanvas.DOFade(1f, 0.3f);
        });
    }
    public void FadeToMenu()
    {
        isSelectMode = false;
        storySelectCanvas.DOFade(0, 0.3f).OnComplete(() =>
        {
            storySelectCanvas.gameObject.SetActive(false);
            titleCanvas.gameObject.SetActive(true);
            titleCanvas.DOFade(1f, 0.3f);
        });
    }

    public void ToMainMenu()
    {
        transitionImage.DOFade(1f, 0.3f).OnComplete(() =>
        {
            SceneManager.LoadSceneAsync("MenuSelect");
        });
    }
}
