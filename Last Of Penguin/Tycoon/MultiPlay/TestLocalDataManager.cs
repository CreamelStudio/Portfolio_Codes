using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestLocalDataManager : MonoBehaviour
{
    public static TestLocalDataManager Instance { get; private set; }

    [System.Serializable]
    public class PlayerData
    {
        public int playerIndex;  // 플레이어 인덱스 (0, 1, 2, 3...)
        public CharacterProfile characterProfile; // 선택한 캐릭터 이름
        public InputDevice device; // 입력 장치 (키보드, 게임패드)
        public bool inStat = false;
    }

    public List<PlayerData> players = new List<PlayerData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬이 변경돼도 유지
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddPlayer(int index, CharacterProfile character, InputDevice device)
    {
        players.Add(new PlayerData { playerIndex = index, characterProfile = character, device = device });
    }

    public void ClearPlayers()
    {
        players.Clear();
    }
}
