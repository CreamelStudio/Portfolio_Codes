using UnityEngine;
using TMPro;

public class PartManager : MonoBehaviour
{
    public static PartManager Instance;

    public int partsCount = 0;
    public TMP_Text partsText;

    void Awake()
    {
        Instance = this;
    }

    public void AddPart()
    {
        if (partsCount < 3) partsCount++;
        partsText.text = $"Part : {partsCount}/3";
    }
}
