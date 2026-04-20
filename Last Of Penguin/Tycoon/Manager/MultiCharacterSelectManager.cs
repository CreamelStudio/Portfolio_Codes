using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiCharacterSelectManager : MonoBehaviour
{
    [SerializeField] private List<CharacterProfile> characterList = new List<CharacterProfile>();
    public int playerCount; // 플레이어 명수
    public bool[] playerReady = { false, false, false, false };

    public void PlusPlayer()
    {
        // if 특정키를 눌렀을 때
        // 캐릭터 추가
    }

    public void ChangePlayer(int playerNum)
    {
        // if 특정키를 눌렀을 때 
        // playerNum 에 따른 플레이어 캐릭터 변경
    }

    public void SelectCharacter(int playerNum)
    {
        // if 특정키를 눌렀을 때
        // playerNum 에 따른 플레이어 선택 및 준비
    }

    public void StartGame()
    {
        // if 방장이라면 모두 준비되었을 때 특정키를 누르면
        // 게임 시작
    }
}
