using System.Collections;
using UnityEngine;
using Unity.WebRTC;
using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Text;

using Newtonsoft.Json;

using System.Net.Http;
using Mvm;

public class WebRTC_Client : MonoBehaviour
{
    #region Static
    readonly string serverUrl = "ec2-16-170-170-2.eu-north-1.compute.amazonaws.com:3000";
    ClientWebSocket webSocket;
    RTCPeerConnection pc;
    RTCDataChannel dataChannel;
    Thread serverThread = null;
    SynchronizationContext syncContext;
    bool threadRunning;

    public string userId ;
    public bool sendOffer,sendMessage;
    public string roomId = "8fc3e523-42ec-4154-b341-44bf35a559c2";
    public string token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJleHAiOjE2ODMxNDU0OTksInVzZXJpZCI6IjU3ZDkxOTNlLWUwMTYtNDlhNy05MTQyLWE4YjI3ZWE2NTMzYyIsInJvbGUiOiIxIn0.F7U2c7cK75GZ8xZDxGH-ABFaYHkHUi1xC9BLq673h1s";
    public RTCSessionDescription pcOffer, pcAnsewer;
    
    [Serializable]
    class Message
    {
        public string Type { get; set; }
        public object Data { get; set; }
        public string ToId { get; set; }
        public string FromId { get; set; }
        public string[] IceCandidates { get; set; }
    }
    [Serializable]
    class PeerICE
    {
        public string Candidate { get; set; }
        public string SdpMid { get; set; }
        public int? SdpMLineIndex { get; set; }
    }

    class PeerICEs
    {
        public string[] ices { get; set; }
    }

    #endregion

    #region MonoBehaviour
    private void Awake()
    {
        syncContext = SynchronizationContext.Current;
        serverThread = new Thread(() => {
            ConnectToMVMServer(token);
            InitPeerConnection();
            ReceiveServerMessagesAsync();
        });
    }
    // Start is called before the first frame update
    void Start()
    {
        threadRunning = true; 
        serverThread?.Start();
        //StartCoroutine(SendOffer());
    }

    // Update is called once per frame
    void Update()
    {
        if (sendOffer)
        {
            sendOffer = false;
            StartCoroutine(SendOffer());
        }
        if (sendMessage)
        {
            sendMessage = false;
            SendMsg("Testing dataChannel");
        }
    }
    async void OnDestroy()
    {
        Debug.Log("Start OnDestroy");
        threadRunning = false; 
        pc?.Close();
        pc = null;
        serverThread?.Join();
        await webSocket?.CloseAsync(WebSocketCloseStatus.NormalClosure, "Shutting down the socket", CancellationToken.None);
        Debug.Log("Finished OnDestroy");
    }

    #endregion

    #region Websocket
    async void ConnectToMVMServer(string token)
    {
        try
        {
            Debug.Log("Connecting to MVM server (WebRTC) ...");

            string url = "ws://" + serverUrl + "/wsrtc?room=" + roomId;

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

    async Task SendMessageToServerAsync(Message message)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
        await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
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
                // Create a Protobuf message instance and deserialize the response message into it
                Message socketMessage = JsonConvert.DeserializeObject<Message>(message);

                switch (socketMessage.Type)
                {
                    case "offer":
                        Debug.LogWarning("Received Offer from " + socketMessage.FromId);
                        syncContext.Post(new SendOrPostCallback(o =>
                        {
                            // Access UI controls or do other work in the main thread
                            StartCoroutine(OnReceiveOfferSuccess(socketMessage));
                        }), null);
                        break;

                    case "answer":
                        Debug.LogWarning("Received answer from " + socketMessage.FromId);
                        syncContext.Post(new SendOrPostCallback(o =>
                        {
                            // Access UI controls or do other work in the main thread
                            StartCoroutine(OnReceiveAnswerSuccess(socketMessage));
                        }), null);
                        break;

                    case "getIce":
                        Debug.LogWarning("Received Ice for " + socketMessage.ToId);
                        syncContext.Post(new SendOrPostCallback(o =>
                        {
                            // OnReceiveIce(socketMessage.Data);
                        }), null);
                        break;

                }
            }
            catch (Exception e)
            {
                Debug.LogWarning(e.ToString());
            }
        }
    }
    #endregion

    #region REST
    public async Task<string> GetPeerIce(string userId)
    {
        using HttpClient client = new HttpClient();
        try
        {
            string responseContent;
            do
            {
                await Task.Delay(1000);
                HttpResponseMessage response = await client.GetAsync("http://" +serverUrl +"/ice?id=" + userId);
                response.EnsureSuccessStatusCode(); // throws exception if HTTP status code is not success (i.e. 200-299)

                responseContent = await response.Content.ReadAsStringAsync();
                Debug.Log($"Result from REST = " + responseContent);
            } while (responseContent == "" || responseContent == "{}");
            return responseContent;
            
        }
        catch (HttpRequestException ex)
        {
            // handle exception
            Debug.LogError($"Error retrieving data from API: {ex.Message}");
            return null;
        }
    }
    #endregion

    #region Init Peer
    RTCConfiguration GetSelectedSdpSemantics()
    {
        RTCConfiguration config = default;
        config.iceServers = new RTCIceServer[]
        {
            new RTCIceServer { urls = new string[] { "stun:stun.l.google.com:19302" } }
        };

        return config;
    }

    void InitPeerConnection()
    {
        Debug.Log("GetSelectedSdpSemantics");

        var configuration = GetSelectedSdpSemantics();
        pc = new RTCPeerConnection(ref configuration);
        Debug.Log($"Created local peer connection object user {userId}");

        pc.OnIceCandidate = OnIceCandidate;

        pc.OnIceConnectionChange = OnIceConnectionChange;
    }

    #endregion

    #region Offer
    IEnumerator SendOffer()
    {
        Debug.Log($"pc {userId} SendOffer start");
        RTCOfferAnswerOptions opt = new RTCOfferAnswerOptions { iceRestart = true };
        var op = pc.CreateOffer(ref opt);
        yield return op;

        if (op.IsError)
        {
            Debug.LogError("Error in creating offer in user " + userId);
            Debug.LogError(op.Error);
            yield return null;
        }
        
        else
        {
            RTCDataChannelInit conf = new RTCDataChannelInit();
            dataChannel = pc.CreateDataChannel("data", conf);
            dataChannel.OnOpen = OnDataChannelOpened;
            dataChannel.OnMessage = OnDataChannelMessage;

            var message = new Message
            {
                Type = "offer",
                ToId = "4d1c28ae-55fa-48a1-b155-41ec90f9f081",
                Data = op.Desc,
            };

            yield return OnCreateOfferSuccess(op.Desc);
            yield return SendMessageToServerAsync(message);
        }
        Debug.Log($"pc {userId} SendOffer end");
    }

    IEnumerator OnCreateOfferSuccess(RTCSessionDescription desc)
    {
        Debug.Log($"pc {userId} OnCreateOfferSuccess start");
        var op = pc.SetLocalDescription(ref desc);
        yield return op;

        if (op.IsError)
        {
            Debug.LogError("Error in SetLocalDescription in user " + userId);
            Debug.LogError(op.Error);
        }
        Debug.Log($"pc {userId} OnCreateOfferSuccess end");
    }

    IEnumerator OnReceiveOfferSuccess(Message socketMessage)
    {
        RTCSessionDescription desc = JsonConvert.DeserializeObject<RTCSessionDescription>(socketMessage.Data.ToString());

        pc.OnDataChannel = channel =>
        {
            dataChannel = channel;
            dataChannel.OnMessage = OnDataChannelMessage;
        };

        Debug.Log($"pc {userId} SetRemoteDescription start");
        var op = pc.SetRemoteDescription(ref desc);
        yield return op;
        if (op.IsError)
        {
            Debug.LogError("Error in SetRemoteDescription in user " + userId);
            Debug.LogError(op.Error);
            yield return null;
        }
        Debug.Log($"pc {userId} SetRemoteDescription end");

        var peerIceRaw = GetPeerIce(socketMessage.FromId);
        while (!peerIceRaw.IsCompleted) { yield return null; }

        OnReceiveIce(peerIceRaw.Result);

        Debug.Log($"pc {userId} CreateAnswer start");
        // Since the 'remote' side has no media stream we need
        // to pass in the right constraints in order for it to
        // accept the incoming offer of audio and video.
        RTCOfferAnswerOptions opt = new RTCOfferAnswerOptions { iceRestart = true };
        var op2 = pc.CreateAnswer(ref opt);
        yield return op2;

        if (op2.IsError)
        {
            Debug.LogError("Error in CreateAnswer in user " + userId);
            Debug.LogError(op2.Error);
            yield return null;

        }
        else
        {
            yield return OnCreateAnswerSuccess(op2.Desc, socketMessage.FromId);
        }
        Debug.Log($"pc {userId} CreateAnswer start");
    }

    #endregion

    #region Answer
    IEnumerator OnCreateAnswerSuccess(RTCSessionDescription desc,string peerId)
    {
        Debug.Log($"pc {userId} OnCreateAnswerSuccess start");
        var op2 = pc.SetLocalDescription(ref desc);
        yield return op2;

        if (op2.IsError)
        {
            Debug.LogError("Error in SetLocalDescription in user " + userId);
            Debug.LogError(op2.Error);
            yield return null;
        }
        else
        {
            var answer = new RTCSessionDescription
            {
                type = RTCSdpType.Answer,
                sdp = desc.sdp
            };
            var message = new Message
            {
                Type = "answer",
                ToId = peerId,
                Data = answer

            };
            yield return SendMessageToServerAsync(message);

/*            yield return Task.Delay(10000);

            var peerIceRaw = GetPeerIce(peerId);
            while (!peerIceRaw.IsCompleted) { yield return null; }

            OnReceiveIce(peerIceRaw.Result);*/
        }
        Debug.Log($"pc {userId} OnCreateAnswerSuccess end");
    }

    IEnumerator OnReceiveAnswerSuccess(Message socketMessage)
    {
        RTCSessionDescription desc = JsonConvert.DeserializeObject<RTCSessionDescription>(socketMessage.Data.ToString());


        Debug.Log("pc setRemoteDescription start");
        var op = pc.SetRemoteDescription(ref desc);
        yield return op;

        var peerIceRaw = GetPeerIce(socketMessage.FromId);
        while (!peerIceRaw.IsCompleted) { yield return null; }

        OnReceiveIce(peerIceRaw.Result);

        if (op.IsError)
        {
            Debug.LogError("Error in SetRemoteDescription in user " + userId);
            Debug.LogError(op.Error);
            yield return null;
        }
        Debug.Log("pc setRemoteDescription end");
    }

    #endregion

    #region ICE
    async void OnIceCandidate(RTCIceCandidate iceCandidate)
    {
        PeerICE peerIce = new PeerICE
        {
            Candidate = iceCandidate.Candidate,
            SdpMid = iceCandidate.SdpMid,
            SdpMLineIndex = iceCandidate.SdpMLineIndex
        };

        Message iceMessage = new Message
        {
            Type = "ice",
            Data = JsonConvert.SerializeObject(peerIce),
        };
        Debug.Log($"Send ICE from {userId}");
        await SendMessageToServerAsync(iceMessage);
    }

    public void OnReceiveIce(string peerIceJson)
    {
        Debug.Log("Call OnReceiveIce");
        Debug.Log(peerIceJson);
        try
        {
            PeerICEs ices = JsonConvert.DeserializeObject<PeerICEs>(peerIceJson);

            foreach (string ice in ices.ices)
            {
                var iceCandidate = JsonConvert.DeserializeObject<PeerICE>(ice);
                RTCIceCandidateInit iceCandidateInit = new RTCIceCandidateInit
                {
                    candidate = iceCandidate.Candidate,
                    sdpMid = iceCandidate.SdpMid,
                    sdpMLineIndex = iceCandidate.SdpMLineIndex
                };

                var candidate = new RTCIceCandidate(iceCandidateInit);
                pc.AddIceCandidate(candidate);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }

    }

    void OnIceConnectionChange(RTCIceConnectionState state)
    {
        switch (state)
        {
            case RTCIceConnectionState.New:
                Debug.Log($"{userId} IceConnectionState: New");
                break;
            case RTCIceConnectionState.Checking:
                Debug.Log($"{userId} IceConnectionState: Checking");
                break;
            case RTCIceConnectionState.Closed:
                Debug.Log($"{userId} IceConnectionState: Closed");
                break;
            case RTCIceConnectionState.Completed:
                Debug.Log($"{userId} IceConnectionState: Completed");
                break;
            case RTCIceConnectionState.Connected:
                Debug.Log($"{userId} IceConnectionState: Connected");
                break;
            case RTCIceConnectionState.Disconnected:
                Debug.Log($"{userId} IceConnectionState: Disconnected");
                break;
            case RTCIceConnectionState.Failed:
                Debug.Log($"{userId} IceConnectionState: Failed");
                break;
            case RTCIceConnectionState.Max:
                Debug.Log($"{userId} IceConnectionState: Max");
                break;
            default:
                break;
        }
    }

    #endregion

    #region Data Channel
    public void SendMsg(string message)
    {
        dataChannel.Send(message);
    }

    public void SendMsg(byte[] message)
    {
        dataChannel.Send(message);
    }

    public void OnDataChannelMessage(byte[] bytes)
    {

        Debug.Log("Recieved :" + bytes.Length);
        // Testing ...
        SocketMessage2 responseMessage = JsonConvert.DeserializeObject<SocketMessage2>(Encoding.UTF8.GetString(bytes));
        Debug.Log(responseMessage.Message);

        Keypoints response = new Keypoints();
        

        foreach (Mvm.Keypoint point in responseMessage.Keypoints)
        {
            response.Points.Add(new Keypoint
            {
                X = point.X,
                Y = point.Y,
                Z = point.Z,
            });
        }
        OrientationProcessor.SetPoints(response);

    }

    public void OnDataChannelOpened()
    {
        Debug.Log($"{userId} Opened a dataChannel");
        dataChannel.Send("TEST");
    }

    #endregion
}
