using DG.Tweening;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public CanvasGroup tutorialCanvas;
    public Text tutorialText;

    [TextArea]
    public string[] tutorialMessages;

    private void Start()
    {
        StartCoroutine(Co_StartTutorialText());
    }


    IEnumerator Co_StartTutorialText()
    {
        yield return new WaitForSeconds(0.1f);
        yield return new WaitForSeconds(NoteSpawnManager.instance.startOffset);

        tutorialCanvas.DOFade(1, 0.5f);
        yield return new WaitForSeconds(3f);
        tutorialText.DOText(tutorialMessages[0], 0).SetEase(Ease.Linear);

        yield return new WaitForSeconds(3f);
        tutorialText.text = "";
        tutorialText.DOText(tutorialMessages[1], 0).SetEase(Ease.Linear);

        yield return new WaitForSeconds(3f);
        tutorialText.text = "";
        tutorialText.DOText(tutorialMessages[2], 0).SetEase(Ease.Linear);

        yield return new WaitForSeconds(3f);
        tutorialText.text = "";
        tutorialText.DOText(tutorialMessages[3], 0).SetEase(Ease.Linear);

        yield return new WaitForSeconds(11f);
        tutorialText.text = "";
        tutorialText.DOText(tutorialMessages[4], 0).SetEase(Ease.Linear);

        yield return new WaitForSeconds(2f);
        tutorialText.text = "";
        tutorialText.DOText(tutorialMessages[5], 0).SetEase(Ease.Linear);

        yield return new WaitForSeconds(2f);
        tutorialText.text = "";
        tutorialText.DOText(tutorialMessages[6], 0).SetEase(Ease.Linear);

        yield return new WaitForSeconds(11f);
        tutorialText.text = "";
        tutorialText.DOText(tutorialMessages[7], 0).SetEase(Ease.Linear);

        yield return new WaitForSeconds(2f);
        tutorialText.text = "";
        tutorialText.DOText(tutorialMessages[8], 0).SetEase(Ease.Linear);

        yield return new WaitForSeconds(2f);
        tutorialText.text = "";
        tutorialText.DOText(tutorialMessages[9], 0).SetEase(Ease.Linear);

        yield return new WaitForSeconds(2.4f);
        tutorialText.text = "";
        tutorialText.DOText(tutorialMessages[10], 0).SetEase(Ease.Linear);

        yield return new WaitForSeconds(2f); //롱노트 끝나는 티이밍에 가까움
        tutorialText.text = "";
        tutorialText.DOText(tutorialMessages[11], 0).SetEase(Ease.Linear);

        yield return new WaitForSeconds(3f);
        tutorialText.text = "";
        tutorialText.DOText(tutorialMessages[12], 0).SetEase(Ease.Linear);

        yield return new WaitForSeconds(4f);
        tutorialText.text = "";
        tutorialText.DOText(tutorialMessages[13], 0).SetEase(Ease.Linear);

        yield return new WaitForSeconds(4f);
        tutorialText.text = "";
        tutorialText.DOText(tutorialMessages[14], 0).SetEase(Ease.Linear);

        PlayerPrefs.SetInt("isFirstBoot", 1);
    }
}
