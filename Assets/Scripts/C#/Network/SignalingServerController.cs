using UnityEngine;
using System;
using System.Net.WebSockets;
using System.Threading;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;


class SignalingServerController : MonoBehaviour
{
    #region Properties
    Thread serverThread = null;
    SynchronizationContext syncContext;
    bool threadRunning;

    UserProfile userProfile;
    ClientWebSocket webSocket;
    List<OnlineStatus> OnlineStatuses = new List<OnlineStatus>();

    public WebRTCController webRTCController;
    #endregion

    #region MonoBehaviour
    private void Awake()
    {
        syncContext = SynchronizationContext.Current;
        userProfile = GetComponent<UserProfile>();
        EventsPool.Instance.AddListener(typeof(LoginStatusEvent),
            new Action<bool>(InitWebSocketConnection));

        EventsPool.Instance.AddListener(typeof(SignalingMessageEvent),
            new Action<SignalingMessage>(SendMessageToServerAsync));

        serverThread = new Thread(() => {
            ConnectToSignalingServer();
            ReceiveServerMessagesAsync();
        });
    }
    async void OnDestroy()
    {
        threadRunning = false;
        webRTCController.pc?.Close();
        webRTCController.pc = null;

        if (serverThread?.IsAlive == true)
            serverThread?.Join();

        if (webSocket != null && webSocket?.State != WebSocketState.Closed)
            await webSocket?.CloseAsync(WebSocketCloseStatus.NormalClosure, "Shutting down the socket", CancellationToken.None);
    }
    #endregion

    public void InitWebSocketConnection(bool isLoggedIn)
    {
        if (isLoggedIn)
        {
            threadRunning = true;
            serverThread?.Start();
        }
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

            Debug.Log("Connected to MVM server");
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception: {0}", e);
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
                        Debug.Log("Received Offer from " + socketMessage.FromId);
                        syncContext.Post(new SendOrPostCallback(o =>
                        {
                            // Access UI controls or do other work in the main thread
                            StartCoroutine(webRTCController.OnReceiveOfferSuccess(socketMessage));
                        }), null);
                        break;

                    case "answer":
                        Debug.Log("Received answer from " + socketMessage.FromId);
                        syncContext.Post(new SendOrPostCallback(o =>
                        {
                            // Access UI controls or do other work in the main thread
                            StartCoroutine(webRTCController.OnReceiveAnswerSuccess(socketMessage));
                        }), null);
                        break;

                    case "ice":
                        Debug.Log("Received Ice for " + socketMessage.ToId);
                        syncContext.Post(new SendOrPostCallback(o =>
                        {
                            webRTCController.OnReceiveIce(socketMessage);
                        }), null);
                        break;
                    case "user_enter":
                        Debug.Log("user enter with id " + socketMessage.FromId);
                        syncContext.Post(new SendOrPostCallback(o =>
                        {
                            StartCoroutine(webRTCController.SendOffer(socketMessage.FromId));
                        }), null);
                        break;
                    case "leave_room":
/*                        localAudioStream?.Dispose();
                        localAudioStream = null;
                        pc?.Dispose();
                        pc = null;*/
                        break;
                    case "get_users_online_status_list":
                        OnlineStatuses = socketMessage.Data as List<OnlineStatus>;
                        foreach (OnlineStatus onlineStatus in OnlineStatuses)
                        {
                            Debug.Log(onlineStatus.ID + "--" + onlineStatus.IsOnline);
                        }
                        break;
                    case "user_status_changed":
                        Debug.Log($"User {socketMessage.FromId} has changed his status to {(bool)socketMessage.Data}");
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

    async void SendMessageToServerAsync(SignalingMessage message)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
        await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
    }

    public void SendJoinRoomEvent(string roomId)
    {
        var message = new SignalingMessage
        {
            Type = "join_room",
            Data = roomId,
        };
        SendMessageToServerAsync(message);
    }

    public void ConnectToRoom(string roomId)
    {
        webRTCController.InitPeerConnection();
        SendJoinRoomEvent(roomId);
        EventsPool.Instance.InvokeEvent(typeof(RoomConnectedStatusEvent), true);
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
[Serializable]
public class OnlineStatus
{

    public string ID { get; set; }
    public bool IsOnline { get; set; }
}

