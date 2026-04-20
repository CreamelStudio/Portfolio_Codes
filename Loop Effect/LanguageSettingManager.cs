using UnityEngine;
using UnityEngine.UI;



public class LanguageSettingManager : MonoBehaviour
{
    public RawImage enButton;
    public RawImage krButton;

    public Color defaultButtonColor = new Color(0.8f, 0.8f, 0.8f);
    public Color selectedButtonColor = new Color(0.9056604f, 0.8334962f, 0f);

    void Start()
    {
        SetLanguage(PlayerPrefs.GetInt("LanguageSet", 0));
    }

    /// <summary>
    /// Set language and update button colors
    /// </summary>
    /// <param name="language">0 = English, 1 = Korean</param>
    public void SetLanguage(int language)
    {
        GameManager.Instance.language = (Language)language;
        
        switch(GameManager.Instance.language)
        {
            case Language.English:
                enButton.color = Color.yellow;
                krButton.color = defaultButtonColor;
                break;
            case Language.Korean:
                enButton.color = defaultButtonColor;
                krButton.color = Color.yellow;
                break;
        }

        PlayerPrefs.SetInt("LanguageSet", language);
    }
}
