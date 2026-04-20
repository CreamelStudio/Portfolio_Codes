using UnityEngine;
using ProjectYH_Server.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using BackEnd;
using System.Collections;
using BackEnd.Socketio;
using System.Collections.Concurrent;

public enum ClientState
{
    Offline,
    Online,
    InRoom,
    Wating,
    InGame,
    Result
}

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance;

    [Header("Server Settings")]
    [SerializeField] private string serverURL = "http://localhost:8080/gameHub";
    private HubConnection connection;
    public List<RoomData> rooms;
    public RoomData _currentRoom {get; private set;}


    public ClientState clientState = ClientState.Online;


    //Events Handlers
    public event Action OnDisconnectServer;
    public event Action OnConnectServer;
    public event Action OnLoadingServer;
    public event Action<RoomData> OnJoinedRoom;
    public event Action OnLeftRoom;
    public event Action<RoomData> OnRoomUpdated;
    public event Action<List<RoomData>, bool> OnRoomListUpdated;

    private readonly ConcurrentQueue<Action> _executionQueue = new ConcurrentQueue<Action>();


    // 라이프 사이클, 핸들러 등 중복 등록 방지 변수
    private bool _handlersRegistered = false;
    private bool _lifecycleRegistered = false;

    public bool isRoomOwner => 
        _currentRoom != null && 
        Backend.UserInDate.Equals(_currentRoom.Players[0].Guid) && 
        Backend.UserNickName.Equals(_currentRoom.Players[0].Nickname);

    async void Start()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        BuildConnectionOnce();
        RegisterHandlersOnce();
        RegisterLifecycleOnce();
        await StartConnectionOnce();

        _ = RefreshLoop();
    }

    void Update()
    {
        while (_executionQueue.TryDequeue(out var action))
        {
            action?.Invoke();
        }
    }

    #region 서버 시스템 메소드

    /// <summary>
    /// 메인 스레드에서 실행할 액션 큐에 추가하는 함수
    /// </summary>
    /// <param name="action">실행할 액션</param>
    public void EnqueueMainThread(Action action)
    {
        _executionQueue.Enqueue(action);
    }
    
    /// <summary>
    /// SignalR 커넥트 빌더의 기초 설정 [한번만 실행]
    /// </summary>
    private void BuildConnectionOnce()
    {
        OnLoadingServer?.Invoke();
        if (connection != null) return;

        connection = new HubConnectionBuilder()
            .WithUrl(serverURL)
            .WithAutomaticReconnect()
            .Build();
    }

    /// <summary>
    /// 서버 이벤트 핸들러 등록 함수 [한번만 실행]
    /// </summary>
    private void RegisterHandlersOnce()
    {
        if (_handlersRegistered) return;
        _handlersRegistered = true;

        connection.On<RoomData>("RoomJoined", room =>
        {
            EnqueueMainThread(() =>
            {
                _currentRoom = room;
                clientState = ClientState.InRoom;
                Debug.Log($"[RoomJoined] {room.RoomName} ({room.Players?.Count ?? 0}/{room.MaxPlayers})");
                OnJoinedRoom?.Invoke(room);
            });
        });

        connection.On<PlayerIdentity>("PlayerJoined", player =>
        {
            EnqueueMainThread(() =>
            {
                _currentRoom?.Players.Add(player);
                OnRoomUpdated?.Invoke(_currentRoom);
                Debug.Log($"[PlayerJoined] {player.Nickname}");
            });
        });

        connection.On<string>("PlayerLeft", (connectionId) =>
        {
            EnqueueMainThread(() =>
            {
                var player = _currentRoom?.Players.Find(p => p.ConnectionId == connectionId);
                if (player != null)
                {
                    _currentRoom?.Players.Remove(player);
                    OnRoomUpdated?.Invoke(_currentRoom);
                    Debug.Log($"[PlayerLeft] {player.Nickname}");
                }
            });
        });

        connection.On("RoomDeleted", () =>
        {
            EnqueueMainThread(() =>
            {
                Debug.Log("[RoomDeleted]");
                _currentRoom = null;
                clientState = ClientState.Online;
                OnLeftRoom?.Invoke();
            });
        });
        connection.On("ReturnToLobby", () =>
        {
            EnqueueMainThread(() =>
            {
                _currentRoom = null;
                clientState = ClientState.Online;
                OnLeftRoom?.Invoke();
                Console.WriteLine($"\n[Event] Returned to Lobby (Room Reset).");
            });
        });
    }

    /// <summary>
    /// 서버 연결 라이프사이클 이벤트 등록 함수 | EX 서버 연결이 끊겼을 때 [한번만 실행]
    /// </summary>
    private void RegisterLifecycleOnce()
    {
        if (_lifecycleRegistered) return;
        _lifecycleRegistered = true;

        connection.Reconnecting += (error) => //서버 연결이 끊겼을 때 이벤트 실행
        {
            Debug.LogWarning($"[SignalR] Reconnecting... {error}");
            OnDisconnectServer?.Invoke();
            return Task.CompletedTask;
        };

        connection.Reconnected += (connectionId) => //서버와 재연결 되었을 때 이벤트 실행
        {
            Debug.Log($"[SignalR] Reconnected. id={connectionId}");
            _ = UpdateRoomList();
            OnConnectServer?.Invoke();
            return Task.CompletedTask;
        };

        connection.Closed += (error) => //서버와 완전히 연결이 종료 되었을 때 이벤트 실행
        {
            Debug.LogError($"[SignalR] Closed: {error}");
            OnDisconnectServer?.Invoke();
            return Task.CompletedTask;
        };
    }

    /// <summary>
    /// 서버 연결 시작 함수 [한번만 실행]
    /// </summary>
    /// <returns></returns>
    private async Task StartConnectionOnce()
    {
        if (connection.State == HubConnectionState.Connected) return;

        try
        {
            await EnsureConnected();
            Debug.Log("[SignalR] Connected.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Project YH - Multi] Connection failed: {ex}");
        }
    }

    public async void DisconnectServer()
    {
        if (connection != null)
        {
            try { await connection.StopAsync(); } catch { }
            try { await connection.DisposeAsync(); } catch { }
            OnDisconnectServer?.Invoke();
        }
    }

    private async void OnApplicationQuit()
    {
        DisconnectServer();
    }

    #endregion
    #region  기본 서버 메소드
    #nullable enable

    /// <summary>
    /// 방 생성 함수
    /// </summary>
    /// <param name="roomId">방의 id값 [방 이름]</param>
    /// <param name="maxPlayers">최대 플레이어 인원 수</param>
    /// <param name="password">방 비밀번호 (선택 사항)</param>
    public async Task CreateRoom(string roomId, int maxPlayers, string? password = null)
    {
        await connection.InvokeAsync("CreateRoom", roomId, maxPlayers, Backend.UserNickName, Backend.UserInDate, password);
    }

    /// <summary>
    /// 방 참가 함수
    /// </summary>
    /// <param name="roomId">방의 id값 [방 이름]</param>
    /// <param name="password">방 비밀번호 (선택 사항)</param>
    public async Task JoinRoom(string roomId, string? password = null)
    {
        await connection.InvokeAsync("JoinRoom", roomId, Backend.UserNickName, Backend.UserInDate, password);
    }

    #nullable disable

    /// <summary>
    /// 방 나가기 함수
    /// </summary>
    /// <returns></returns>
    public async Task LeaveRoom()
    {
        await connection.InvokeAsync("LeaveRoom");
        clientState = ClientState.Online;
        OnLeftRoom?.Invoke();
        _currentRoom = null;
    }

    /// <summary>
    /// 서버 연결 시작 함수
    /// </summary>
    /// <returns></returns>
    private async Task EnsureConnected()
    {
        if (connection.State == HubConnectionState.Connected)
            return;

        await connection.StartAsync();
        OnConnectServer?.Invoke();
        Debug.Log("[SignalR] Connected.");
    }

    /// <summary>
    /// 서버 연결 시도 함수
    /// </summary>
    /// <returns></returns>
    public async Task<bool> TryConnectServer()
    {
        try
        {
            if (connection.State == HubConnectionState.Disconnected) await EnsureConnected();
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Project YH - Multi] Connection failed: {ex}");
            return false;
        }
    }

    /// <summary>
    /// 방 목록 갱신 함수
    /// </summary>
    /// <param name="isUserAction">유저의 요청 여부</param>
    /// <returns>Event[OnRoomListUpdated[List RoomData, bool]</returns>
    public async Task UpdateRoomList(bool isUserAction = false)
    {
        if(connection.State != HubConnectionState.Connected)
        {
            Debug.LogWarning("[Project YH - Multi] 서버에 연결되어 있지 않습니다.");
            return;
        }
        
        try
        {
            OnLoadingServer?.Invoke();
            var fetchedRooms = await connection.InvokeAsync<List<RoomData>>("GetRoomList");
            rooms = fetchedRooms ?? new List<RoomData>();

            Debug.Log($"[List] Rooms: {rooms.Count}");
            foreach (var r in rooms) Debug.Log($"- {r.RoomName} ({r.Players?.Count ?? 0}/{r.MaxPlayers})");
            
            OnRoomListUpdated?.Invoke(rooms, isUserAction);
            OnConnectServer?.Invoke();
        }
        catch (Exception ex)
        {
            Debug.LogError($"[List] Error: {ex}");
        }
    }
    #endregion

    #region 서버 접속 상태 관리
    /// <summary>
    /// 서버 접속 상태 갱신 루프
    /// </summary>
    /// <returns></returns>
    private async Task RefreshLoop()
    {
        while (Application.isPlaying)
        {
            await Task.Delay(1000);
            await RefreshServerConnectionRoutine();
        }
    }

    /// <summary>
    /// 서버 접속 상태 갱신 함수
    /// </summary>
    /// <returns></returns>
    private async Task RefreshServerConnectionRoutine()
    {
        if(connection.State == HubConnectionState.Connected)
        {
            await UpdateRoomList();
        }
        else
        { 
            Debug.LogError("[ProjectYH - Server] 서버와 연결이 끊겼습니다.");
            OnDisconnectServer?.Invoke();
        }
    }
    #endregion
}
