
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using TMPro;

public class EndingCamera : MonoBehaviour
{
    public Image fadeImage;
    public Image whiteFadeImage; // For white fade out
    public VideoPlayer videoPlayer; // Video player
    public TextMeshProUGUI thanksText;
    public float fadeDuration = 2f;
    public Transform character;
    public float moveSpeed = 1f;
    public float delayBeforeText = 5f;
    public float delayAfterText = 2f;

    private bool shouldMove = false;

    void Start()
    {
        if (thanksText != null)
        {
            thanksText.alpha = 0f;
        }
        if (whiteFadeImage != null)
        {
            whiteFadeImage.color = new Color(1f, 1f, 1f, 0f); // Start transparent
            whiteFadeImage.gameObject.SetActive(false);
        }
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoFinished;
            videoPlayer.gameObject.SetActive(false);
        }
        StartCoroutine(EndingSequence());
    }

    void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }

    void Update()
    {
        if (shouldMove && character != null)
        {
            character.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
        }
    }

    IEnumerator EndingSequence()
    {
        shouldMove = true;

        yield return StartCoroutine(Fade(1f, 0f, fadeImage));

        yield return new WaitForSeconds(delayBeforeText);

        if (thanksText != null)
        {
            yield return StartCoroutine(FadeInText());
        }

        yield return new WaitForSeconds(delayAfterText);

        if (whiteFadeImage != null)
        {
            whiteFadeImage.gameObject.SetActive(true);
            yield return StartCoroutine(Fade(0f, 1f, whiteFadeImage));
        }

        if (videoPlayer != null)
        {
            videoPlayer.gameObject.SetActive(true);
            videoPlayer.Play();
        }
        else
        {
            // If no video player, load lobby directly
            LoadLobbyScene();
        }
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        LoadLobbyScene();
    }

    void LoadLobbyScene()
    {
        SceneManager.LoadScene("_01_Lobby");
    }



    IEnumerator Fade(float startAlpha, float endAlpha, Image image)
    {
        float counter = 0f;
        Color fadeColor = image.color;

        while (counter < fadeDuration)
        {
            counter += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, counter / fadeDuration);
            image.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            yield return null;
        }
        image.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, endAlpha);
    }

    IEnumerator FadeInText()
    {
        float counter = 0f;
        while (counter < fadeDuration)
        {
            counter += Time.deltaTime;
            thanksText.alpha = Mathf.Lerp(0f, 1f, counter / fadeDuration);
            yield return null;
        }
        thanksText.alpha = 1f;
    }
}
