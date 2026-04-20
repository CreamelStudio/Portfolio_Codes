using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TestLocalGameManager : MonoBehaviour
{
    public Transform[] spawnPoints; // 플레이어 스폰 위치
    public GameObject[] characterPrefabs; // 캐릭터 프리팹 리스트

    private void Start()
    {
        //SpawnPlayers();
    }

    void SpawnPlayers()
    {
        List<TestLocalDataManager.PlayerData> players = TestLocalDataManager.Instance.players;

        for (int i = 0; i < players.Count; i++)
        {
            TestLocalDataManager.PlayerData data = players[i];
            CharacterProfile characterData = data.characterProfile;
            if (data == null) continue;

            GameObject player = Instantiate(characterData.inGameObject, spawnPoints[i].position, Quaternion.identity);
            PlayerInput input = player.GetComponent<PlayerInput>();
            input.SwitchCurrentControlScheme(data.device);
            Debug.Log($"Player {data.playerIndex} 스폰: {data.characterProfile}, {data.device}");
        }
    }
}