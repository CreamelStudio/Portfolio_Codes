using UnityEngine;
using System;
using TMPro;

public class PopupManager : MonoBehaviour
{
    public static PopupManager Instance;

    private Action confirmAction;
    private Action cancelAction;

    public GameObject popupObj;
    public TMP_Text popupText;

    public bool isOnPopup;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this.gameObject);
    }

    private void Start()
    {
        PopupBase.ActivePopupCount = 0; // √ ±‚»≠
    }

    public void StartPopup(string popUpText, Action confirm, Action cancel)
    {
        popupText.text = popUpText;
        confirmAction = confirm;
        cancelAction = cancel;

        isOnPopup = true;
        popupObj.SetActive(true);
    }

    public void ConfirmButton()
    {
        confirmAction();
        isOnPopup = false;
        popupObj.SetActive(false);
    }

    public void CancelButton() 
    {
        cancelAction();
        isOnPopup = false;
        popupObj.SetActive(false);
    }
}
