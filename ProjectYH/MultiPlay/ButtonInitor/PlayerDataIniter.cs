using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDataIniter : MonoBehaviour
{
    public RawImage playerProfileImage;
    public TMP_Text playerNameText;
    public TMP_Text playerLevelText;
    public GameObject groupOwnerIcon;

    public void Init(string playerName, int playerLevel, Texture2D profileImage, bool isGroupOwner)
    {
        playerNameText.text = playerName;
        playerLevelText.text = $"Lv. {playerLevel}";
        playerProfileImage.texture = profileImage;
        groupOwnerIcon.SetActive(isGroupOwner);
    }
}
