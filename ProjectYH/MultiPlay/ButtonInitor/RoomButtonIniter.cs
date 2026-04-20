using ProjectYH_Server.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomButtonIniter : MonoBehaviour
{
    public TMP_Text roomNumberText;
    public TMP_Text roomNameText;
    public TMP_Text playerCountText;

    public GameObject lockIcon;

    public Button roomButton;
    
    public void Init(string roomNumber, RoomData room, bool isPrivate)
    {
        roomNumberText.text = roomNumber;
        roomNameText.text = room.RoomName;
        lockIcon.SetActive(isPrivate);
        playerCountText.text = $"{room.Players?.Count.ToString() ?? "?"}/{room.MaxPlayers}";
        roomButton.onClick.AddListener(async () =>
        {
            if(!isPrivate) await NetworkManager.Instance.JoinRoom(room.RoomId);
            else MultiRoomListManager.Instance.JoinPrivateRoom(room);
            Debug.Log("Joined Room: " + room.RoomName + " (" + room.RoomId + ")");
        });
    }
}
