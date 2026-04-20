using UnityEngine;
using ProjectYH_Server.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using BackEnd;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class MultiRoomListManager : MonoBehaviour
{
    public static MultiRoomListManager Instance;

    private List<RoomData> rooms = new List<RoomData>();

    [Header("UI Settings")]
    public GameObject buttonPrefab;
    public Transform roomListContainer;

    public GameObject failedConnectUI;

    public GameObject loadingUI;

    [Header("Private Room UI")]
    public GameObject privateRoomPasswordUI;
    public Button enterPasswordButton;
    public TMP_InputField passwordInputField;


    async void Start()
    {
        if(Instance == null)
        {
            Instance = this;
            GameManager.instance.isMultiPlay = true;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        NetworkManager.Instance.OnRoomListUpdated += UpdateRoomUI;

        NetworkManager.Instance.OnDisconnectServer += () =>
        {
            loadingUI.SetActive(false);
            failedConnectUI.SetActive(true);
        };

        NetworkManager.Instance.OnLoadingServer += () =>
        {
            loadingUI.SetActive(true);
        };

        NetworkManager.Instance.OnConnectServer += () =>
        {
            loadingUI.SetActive(false);
            failedConnectUI.SetActive(false);
        };
    }

    
    /// <summary>
    /// Private Room 입장 UI 오픈
    /// </summary>
    /// <param name="room"></param>
    public async void JoinPrivateRoom(RoomData room)
    {
        privateRoomPasswordUI.SetActive(true);
        enterPasswordButton.onClick.RemoveAllListeners();
        enterPasswordButton.onClick.AddListener(async () =>
        {
            string password = passwordInputField.text;
            passwordInputField.text = "";
            await NetworkManager.Instance.JoinRoom(room.RoomId, password);
            privateRoomPasswordUI.SetActive(false);
        });
    }

    /// <summary>
    /// Private Room 입장 UI 닫기
    /// </summary>
    public void ClosePrivateRoomUI()
    {
        privateRoomPasswordUI.SetActive(false);
    }

    /// <summary>
    /// 룸 리스트 UI 업데이트
    /// </summary>
    /// <param name="updatedRooms"></param>
    /// <param name="isUserAction"></param>
    private void UpdateRoomUI(List<RoomData> updatedRooms, bool isUserAction)
    {
        rooms = updatedRooms;

        foreach(Transform child in roomListContainer)
        {
            Destroy(child.gameObject);
        }

        for(int i = 0; i < rooms.Count; i++)
        {
            GameObject buttonObj = Instantiate(buttonPrefab, roomListContainer);
            RoomButtonIniter initer = buttonObj.GetComponent<RoomButtonIniter>();
            initer.Init((i + 1).ToString(), rooms[i], rooms[i].IsPrivate);
        }

        loadingUI.SetActive(false);
    }

    
    /// <summary>
    /// 빠른 입장 기능
    /// </summary>
    public async void QuickJoinRoom()
    {
        await NetworkManager.Instance.UpdateRoomList();

        foreach(RoomData room in rooms)
        {
            if(room.Players.Count < room.MaxPlayers && !room.IsPrivate && room.State == RoomState.Waiting)
            {
                await NetworkManager.Instance.JoinRoom(room.RoomId);
                return;
            }

            if(room.Players.Count < room.MaxPlayers && !room.IsPrivate)
            {
                await NetworkManager.Instance.JoinRoom(room.RoomId);
                return;
            }
        }
    }

    /// <summary>
    /// 메인 메뉴로 돌아가기
    /// </summary>
    public void Quit()
    {
        NetworkManager.Instance.DisconnectServer();
        GameManager.instance.isMultiPlay = false;

        SceneManager.LoadSceneAsync("MenuSelect");
    }

    /// <summary>
    /// 재접속 시도 함수 
    /// </summary>
    public async void ReconnectTry()
    {
        failedConnectUI.SetActive(false);
        loadingUI.SetActive(true);
        await NetworkManager.Instance.TryConnectServer();
    }

    
}