using UnityEngine;
using System;
using System.Net.WebSockets;
using System.Threading;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Mvm;

class SignalingServerController : Singleton<SignalingServerController>
{
    #region Properties
    Thread serverThread = null;
    SynchronizationContext syncContext;
    bool threadRunning;

    UserProfile userProfile;
    static ClientWebSocket webSocket;
    public OnlineStatuses usersOnlineStatus;

    #endregion

    #region MonoBehaviour
    protected override void Awake()
    {
        base.Awake();
        syncContext = SynchronizationContext.Current;
        userProfile = GetComponent<UserProfile>();
        EventsPool.Instance.AddListener(typeof(ConnectToServerEvent),
            new Action(InitWebSocketConnection));

        EventsPool.Instance.AddListener(typeof(HangupEvent),
            new Action(() =>
            {
                WebRTCManager.Instance.DisposeAllWebRTCConnections();
            }));

        serverThread = new Thread(() => {
            ConnectToSignalingServer();
            ReceiveServerMessagesAsync();
        });
    }

    private async void OnApplicationQuit()
    {
        threadRunning = false;

        WebRTCManager.Instance.DisposeAllWebRTCConnections();

        if (serverThread?.IsAlive == true)
            serverThread?.Join();

        if (webSocket != null && webSocket?.State != WebSocketState.Closed)
            await webSocket?.CloseAsync(WebSocketCloseStatus.NormalClosure, "Shutting down the socket", CancellationToken.None);
    }

    #endregion

    public void InitWebSocketConnection()
    {
        threadRunning = true;
        serverThread?.Start();
    }

    async void ConnectToSignalingServer()
    {
        // TODO throw err 
        string token = userProfile.userData.Token;
        if (token == null || token == "") return;

        try
        {
            Debug.Log("Connecting to MVM server (WebRTC) ...");

            string url = "ws://" + Server.ServerUrl + ":" + Server.Port + "/wsrtc";

            // Create a new instance of ClientWebSocket
            webSocket = new ClientWebSocket();
            webSocket.Options.SetRequestHeader("Authorization", token);

            // Connect to the server
            await webSocket.ConnectAsync(new Uri(url), CancellationToken.None);

            EventsPool.Instance.InvokeEvent(typeof(LoginStatusEvent), true);
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception: {0}", e);
            EventsPool.Instance.InvokeEvent(typeof(LoginStatusEvent), false);
        }
    }

    async void ReceiveServerMessagesAsync()
    {
        while (threadRunning)
        {
            if (webSocket.State != WebSocketState.Open) continue;
            try
            {
                byte[] responseBuffer = new byte[9000];
                WebSocketReceiveResult responseResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(responseBuffer), CancellationToken.None);

                string message = Encoding.UTF8.GetString(responseBuffer, 0, responseResult.Count);
                SignalingMessage socketMessage = JsonConvert.DeserializeObject<SignalingMessage>(message);

                switch (socketMessage.Type)
                {
                    case "offer":
                        await HandleOfferMessage(socketMessage);
                        break;

                    case "answer":
                        HandleAnswerMessage(socketMessage);
                        break;

                    case "ice":
                        HandleICEMessage(socketMessage);
                        break;

                    case "user_enter":
                        HandleUserEnterMessage(socketMessage);
                        break;

                    case "leave_room":
/*                        localAudioStream?.Dispose();
                        localAudioStream = null;
                        pc?.Dispose();
                        pc = null;*/
                        break;

                    case "get_users_online_status_list":
                        HandleGetUsersOnlineStatus(socketMessage);
                        break;

                    case "user_status_changed":
                        HandleUserStatusChanged(socketMessage);
                        break;

                    case "notification":
                        HandleNotificationMessage(socketMessage);
                        break;
                    case "error":
                        HandleError(socketMessage);
                        break;
                    case "chat_message":
                        HandleChatMessageReceived(socketMessage);
                        break;

                    default:
                        Debug.Log("Received message type : " + socketMessage.Type + " No events assigned to this type");
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning(e.ToString());
            }
        }
    }

    private async Task HandleOfferMessage(SignalingMessage socketMessage)
    {
        Debug.Log("Received Offer from " + socketMessage.FromId);
        await WebRTCManager.Instance.CreateNewWebRTCConnection(socketMessage.FromId);
        WebRTCManager.Instance.ReceiveOffer(socketMessage.FromId, socketMessage);
    }

    private void HandleAnswerMessage(SignalingMessage socketMessage)
    {
        Debug.Log("Received answer from " + socketMessage.FromId);
        WebRTCManager.Instance.ReceiveAnswer(socketMessage.FromId, socketMessage);
    }

    private void HandleICEMessage(SignalingMessage socketMessage)
    {
        Debug.Log("Received Ice from " + socketMessage.FromId);
        WebRTCManager.Instance.ReceiveICE(socketMessage.FromId, socketMessage);
    }

    private async void HandleUserEnterMessage(SignalingMessage socketMessage)
    {
        Debug.Log("user enter with id " + socketMessage.FromId);
        await WebRTCManager.Instance.CreateNewWebRTCConnection(socketMessage.FromId);
        WebRTCManager.Instance.SendOffer(socketMessage.FromId);
    }

    private async void HandleNotificationMessage(SignalingMessage socketMessage)
    {
        var newNotification = JsonConvert.DeserializeObject<Mvm.Notification>((string)socketMessage.Data);
        Debug.Log($"Notification from {socketMessage.FromId} {newNotification.Message}  -- {newNotification.Type}");

        UserProfile.Instance.userData.Notifications.Add(newNotification);
        if (newNotification.Type == (int)NotificationType.FriendRequest || newNotification.Type == (int)NotificationType.AcceptRequest)
        {
            await UserProfile.Instance.GetMyFriends();
            await SendRefreshFriendsEvent();
        }
        EventsPool.Instance.InvokeEvent(typeof(ReceivedNotificationEvent));

    }

    private void HandleGetUsersOnlineStatus(SignalingMessage socketMessage)
    {
        Debug.Log("get user online status  " + socketMessage.Data);
        usersOnlineStatus = JsonConvert.DeserializeObject<OnlineStatuses>((string)socketMessage.Data);
        foreach (OnlineStatus onlineStatus in usersOnlineStatus.Users)
        {
            Debug.Log($"ID: {onlineStatus.Id} -- Username: {onlineStatus.Username} -- + IsOnline:{onlineStatus.IsOnline}");
        }
        EventsPool.Instance.InvokeEvent(typeof(UsersOnlineStatusEvent), usersOnlineStatus);
    }

    private void HandleUserStatusChanged(SignalingMessage socketMessage)
    {
        var newOnlineStatus = JsonConvert.DeserializeObject<OnlineStatus>((string)socketMessage.Data);
        Debug.Log($"User {socketMessage.FromId} has changed his status to {newOnlineStatus.IsOnline}");
        UpdateUserOnlineStatus(newOnlineStatus);
        EventsPool.Instance.InvokeEvent(typeof(UsersOnlineStatusEvent), usersOnlineStatus);
    }

    private void HandleChatMessageReceived(SignalingMessage socketMessage)
    {
        var chatMessage = JsonConvert.DeserializeObject<SocketChatMessage>((string)socketMessage.Data);
        Debug.Log($"Received chat message from {socketMessage.FromId} Message : {chatMessage.Message}");
        EventsPool.Instance.InvokeEvent(typeof(ChatMessageReceviedEvent), chatMessage);
    }

    private void HandleError(SignalingMessage socketMessage)
    {
        var newSocketError = JsonConvert.DeserializeObject<ErrorMessage>((string)socketMessage.Data);
        Debug.Log($"Socket Error {newSocketError.Error} StatusCode = {newSocketError.StatusCode} Type {newSocketError.Type}");
        switch (newSocketError.Type)
        {
            case (int)ErrorMessageType.RoomNotAuthorized:
                EventsPool.Instance.InvokeEvent(typeof(HangupEvent));
                break;
        }
        EventsPool.Instance.InvokeEvent(typeof(ShowPopupEvent), new object[] {
                newSocketError.Error,
                3f,
                Color.red
            });
    }

    public static async Task SendMessageToServerAsync(SignalingMessage message)
    {
        Debug.Log("send message to sever socket");
        byte[] buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
        await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
    }

    public async Task SendJoinRoomEvent(string roomId)
    {
        var message = new SignalingMessage
        {
            Type = "join_room",
            Data = roomId,
        };
        await SendMessageToServerAsync(message);
    }

    public async void ConnectToRoom(string roomId)
    {
        EventsPool.Instance.InvokeEvent(typeof(ToggleLoadingPanelEvent), true);
        await SendJoinRoomEvent(roomId);
        WebRTCManager.Instance.CaptureAudio();
        EventsPool.Instance.InvokeEvent(typeof(RoomConnectedStatusEvent), true);

        await Task.Delay(2000);
        EventsPool.Instance.InvokeEvent(typeof(ToggleLoadingPanelEvent), false);
    }

    private void UpdateUserOnlineStatus(OnlineStatus newOnlineStatus)
    {
        foreach (OnlineStatus onlineStatus in usersOnlineStatus.Users)
        {
            if (onlineStatus.Id == newOnlineStatus.Id)
            {
                onlineStatus.IsOnline = newOnlineStatus.IsOnline;
                return;
            }
        }

        usersOnlineStatus.Users.Add(newOnlineStatus);

    }

    public async Task SendRefreshFriendsEvent()
    {
        await SignalingServerController.SendMessageToServerAsync(new SignalingMessage
        {
            Type = "refreshFriends"
        });
    }

    public async void SendChatMessage(string chatId ,string toUserId, string message)
    {
        var json = JsonConvert.SerializeObject(new SocketChatMessage
        {
            ChatId = chatId,
            UserId = UserProfile.Instance.userData.Id,
            Message = message
        });
        await SendMessageToServerAsync(new SignalingMessage
        {
            Type = "chat_message",
            ToId = toUserId,
            Data = json
        });
    }
}
[Serializable]
public class SignalingMessage
{
    public string Type { get; set; }
    public object Data { get; set; }
    public string ToId { get; set; }
    public string FromId { get; set; }
    public string[] IceCandidates { get; set; }
}
[Serializable]
public class PeerICE
{
    public string Candidate { get; set; }
    public string SdpMid { get; set; }
    public int? SdpMLineIndex { get; set; }
}

