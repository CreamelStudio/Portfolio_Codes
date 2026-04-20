using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TransitionCanvas : MonoBehaviour
{
    public static TransitionCanvas instance;

    [SerializeField] private GameObject transitionObj;
    [SerializeField] private Slider progress;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text decText;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void InitProgress(float prog)
    {
        progress.value = prog;
    }

    public void InitText(string title, string dec)
    {
        titleText.text = title;
        decText.text = dec;
    }

    public void OnOffTransition(bool isOn)
    {
        transitionObj.SetActive(isOn);
    }
}
