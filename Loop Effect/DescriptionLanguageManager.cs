using UnityEngine;
using TMPro;

public class DescriptionLanguageManager : MonoBehaviour
{
    public TMP_Text description;

   [TextArea(3, 5)] public string englishDescription;
   [TextArea(3, 5)] public string koreanDescription;

    private void Start()
    {
        if (description == null) description = GetComponent<TMP_Text>();
        switch (GameManager.Instance.language)
        {
            case Language.English:
                description.text = englishDescription;
                break;
            case Language.Korean:
                description.text = koreanDescription;
                break;
        }
    }
}
