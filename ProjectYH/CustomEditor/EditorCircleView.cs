using TMPro;
using UnityEngine;

public class EditorCircleView : MonoBehaviour
{
    public TMP_Text inCircle;

    public void InitText(string text, float fontSize)
    {
        inCircle.text = text;
        inCircle.fontSize = fontSize;
    }
}
