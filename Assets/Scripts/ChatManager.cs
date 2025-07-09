using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Chat;
using UnityEngine;
                                            
public class ChatManager : MonoBehaviour, IChatClientListener
{
    // 채팅 이벤트 (총 11개)
    // 0. 서버 로그
    // 1. 서버 접속/해제(카카오톡 접속/해제)
    // 2. 채널 접속/해제(카카오톡 채팅방(1:1, 오픈채팅) 접속/해제
    // 3. 메세지 수신 (1:1, 오픈채팅)
    // 4. 다른 사람 방 입장/퇴장 (카카오톡 단톡방 입장/퇴장)
    // 5. 친구 이벤트(친구 상태 변화)
    public static ChatManager Instance { get; private set; }
    
    private ChatClient _client;

    public event Action OnDataChanged;
    
    private const string DEFAULT_GLOBAL_CHANNEL = "global";
    private const string DEFAULT_NOTICE_CHANNEL = "notice";
    
    private List<Chat> _chats = new List<Chat>();
    public  List<Chat> Chats => _chats;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // IChatClientListener 구현 객체를 this로 넘겨줘서 초기화한다.
        _client = new ChatClient(this);   
        
        // 디버그 로그 레벨
        _client.DebugOut = DebugLevel.ALL;
        
        // 서버 지역 설정(US, ES, ASIA만 있음)
        _client.ChatRegion = "ASIA";
        
        // 유저 ID
        var auth = new AuthenticationValues("hesther");
        
        // 채팅 연결
        _client.Connect("3ef16cc8-f034-402c-b0fa-30a27a25e0a2", "1.0.0", auth);
    }

    private void Update()
    {
        // ChatClient는 MonoBehaviour가 아니므로, 매 프레임마다 서비스를 호출해줘야 네트워크 메시지가 처리되고, 콜백 메서드들이 실행된다.
        _client.Service();

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            _client.PublishMessage(DEFAULT_GLOBAL_CHANNEL, "오늘도 화이팅!");
        }
    }

    // 포톤챗 내부 로그 발생시 호출되는 함수
    public void DebugReturn(DebugLevel level, string message)
    {
        switch (level)
        {
            case DebugLevel.ERROR:
                Debug.LogError(message);
                break;
            case DebugLevel.WARNING:
                Debug.LogWarning(message);
                break;
            default:
                Debug.Log(message);
                break;
        }
    }
    
    // 서버 접속 상태
    public void OnChatStateChange(ChatState state)
    {
        Debug.Log($"포톤챗 상태: {state}");
    }
    
    
    // 서버와 연결이 끊겼을 때 호출되는 함수
    // 서버와 연결되었을 때 호출되는 함수
    public void OnConnected()
    {
        var channelOption = new ChannelCreationOptions();
        channelOption.PublishSubscribers = true;
        
        Debug.Log("포톤챗 접속 완료");
        // _client.Subscribe("global"); // 채널 1개 구독
        _client.Subscribe(DEFAULT_GLOBAL_CHANNEL, 0, 20, creationOptions: channelOption); // 채널 여러개 구독
    }
    
    public void OnDisconnected()
    {
        Debug.Log("포톤챗 접속 종료(실패)");
    }
    
    public void OnSubscribed(string[] channels, bool[] results)
    {
        for (int i = 0; i < channels.Length; i++)
        {
            Debug.Log($"[PhotonChat] Subscribed ▶ {channels[i]} (success={results[i]})");
        }
        
        OnDataChanged?.Invoke();
    }
    
    public void OnUnsubscribed(string[] channels)
    {
        foreach (var ch in channels)
        {
            Debug.Log($"[PhotonChat] Unsubscribed ▶ {ch}");
        }

        OnDataChanged?.Invoke();
    }

    public void SendPublicChatMessage(string message)
    {
        if (_client == null || !_client.CanChat)
        {
            return;
        }
        _client.PublishMessage(DEFAULT_GLOBAL_CHANNEL, message);
    }
    
    
    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        for (int i = 0; i < messages.Length; i++)
        {
            Debug.Log($"[{channelName}] {senders[i]}: {messages[i]}");
            if (senders[i] == "hesther")
            {
                _chats.Add(new Chat(EChatType.Mine, senders[i], messages[i].ToString()));
            }
            else
            {
                _chats.Add(new Chat(EChatType.Other, senders[i], messages[i].ToString()));
            }
        }
        OnDataChanged?.Invoke();
    }
    
    public void OnPrivateMessage(string sender, object message, string channelName)
    {
        Debug.Log($"[Whisper] {sender} > {message}");
    }
    public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
    {
        Debug.Log("OnStatusUpdate");
    }
    public void OnUserSubscribed(string channel, string user)
    {
        Chat chat = new Chat(EChatType.System, user, $"{user}님이 방에 입장했습니다.");
        _chats.Add(chat);
        OnDataChanged?.Invoke();
    }
    public void OnUserUnsubscribed(string channel, string user)
    {
        Chat chat = new Chat(EChatType.System, user, $"{user}님이 방에서 퇴장했습니다.");
        _chats.Add(chat);
        OnDataChanged?.Invoke();
    }
}
