using TMPro;
using UnityEngine;

public class PopUpManager : MonoBehaviour
{
    System.Action _OnClickConfirmButton, _OnClickCancelButton;
    public static PopUpManager instance;

    public GameObject popup;

    public GameObject way1;
    public GameObject way2;

    public TMP_Text titleText;
    public TMP_Text descText;

    public Animator confirmAnim;
    public Animator cancelAnim;

    private void Awake()
    {
        instance = this;
    }

    public void PopupShow(string title, string desc, System.Action OnClickConfirmButton, System.Action OnClickCancelButton)
    {
        _OnClickConfirmButton = OnClickConfirmButton;
        _OnClickCancelButton = OnClickCancelButton;

        titleText.text = title;
        descText.text = desc;

        way2.SetActive(true);
        popup.SetActive(true);
    }
    public void PopupShow(string title, string desc, System.Action OnClickConfirmButton)
    {
        _OnClickConfirmButton = OnClickConfirmButton;

        titleText.text = title;
        descText.text = desc;

        way1.SetActive(true);
        popup.SetActive(true);
    }

    public void ConfirmButton()
    {
        _OnClickConfirmButton();
        popup.SetActive(false);
        way1.SetActive(false);
        way2.SetActive(false);
        ManageConfirmAnim(false);
    }

    public void CancelButton()
    {
        _OnClickCancelButton();
        popup.SetActive(false);
        way1.SetActive(false);
        way2.SetActive(false);
        ManageConfirmAnim(false);
    }

    public void ManageConfirmAnim(bool isOn)
    {
        if(isOn)
        {
            confirmAnim.SetBool("isOn", true);
            confirmAnim.SetBool("isOff", false);
        }
        else
        {
            confirmAnim.SetBool("isOn", false);
            confirmAnim.SetBool("isOff", true);
        }
    }
    public void ManageCancelAnim(bool isOn)
    {
        if (isOn)
        {
            cancelAnim.SetBool("isOn", true);
            cancelAnim.SetBool("isOff", false);
        }
        else
        {
            cancelAnim.SetBool("isOn", false);
            cancelAnim.SetBool("isOff", true);
        }
    }
}
