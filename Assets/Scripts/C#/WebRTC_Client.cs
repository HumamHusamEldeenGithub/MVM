using System.Collections;
using UnityEngine;
using Unity.WebRTC;
using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using Newtonsoft.Json;
using Mvm;
using System.Collections.Generic;

public class WebRTC_Client : MonoBehaviour
{
    #region Static
    Thread serverThread = null;
    bool threadRunning;

    ClientWebSocket webSocket;
    RTCPeerConnection pc;
    List<RTCDataChannel> dataChannels = new List<RTCDataChannel>();
    SynchronizationContext syncContext;
    UserProfile userProfile;
    AudioStreamTrack localAudioStream;
    MediaStream audioStream;
    [SerializeField] GameObject audioSourcePrefab;
    [SerializeField] GameObject avatarPrefab;
    private DelegateOnTrack pc2Ontrack;

    public string userId ;
    public bool ConnectToWebRTC;
    public string peerId;
    public string roomId = "8fc3e523-42ec-4154-b341-44bf35a559c2";
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
    #endregion

    #region MonoBehaviour
    private void Awake()
    {
        syncContext = SynchronizationContext.Current;
        userProfile = GetComponent<UserProfile>();
        userId = userProfile.username;

        serverThread = new Thread(() => {
            ConnectToMVMServer();
            InitPeerConnection();
            ReceiveServerMessagesAsync();
        });
        
    }

    // Update is called once per frame
    void Update()
    {
        if (ConnectToWebRTC)
        {
            ConnectToWebRTC = false;
            ConncetToRoom();
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

    void CreateNewAvatar(string peerId , RTCDataChannel dataChannel)
    {
        GameObject newAvatar = Instantiate(avatarPrefab);
        newAvatar.transform.name = peerId;
        newAvatar.GetComponent<PeerController>().SetPeerController(peerId, dataChannel);
    }

    #endregion

    #region Websocket
    public void ConncetToRoom()
    {
        threadRunning = true;
        serverThread?.Start();
    }
    async void ConnectToMVMServer()
    {
        // TODO throw err 
        string token = userProfile.userData.Token;
        if (token == null || token == "") return;

        try
        {
            Debug.Log("Connecting to MVM server (WebRTC) ...");

            string url = "ws://" + Server.ServerUrl + ":" + Server.Port + "/wsrtc?room=" + roomId;

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
                        Debug.Log("Received Offer from " + socketMessage.FromId);
                        syncContext.Post(new SendOrPostCallback(o =>
                        {
                            // Access UI controls or do other work in the main thread
                            StartCoroutine(OnReceiveOfferSuccess(socketMessage));
                        }), null);
                        break;

                    case "answer":
                        Debug.Log("Received answer from " + socketMessage.FromId);
                        syncContext.Post(new SendOrPostCallback(o =>
                        {
                            // Access UI controls or do other work in the main thread
                            StartCoroutine(OnReceiveAnswerSuccess(socketMessage));
                        }), null);
                        break;

                    case "ice":
                        Debug.Log("Received Ice for " + socketMessage.ToId);
                        syncContext.Post(new SendOrPostCallback(o =>
                        {
                            OnReceiveIce(socketMessage);
                        }), null);
                        break;
                    case "user_enter":
                        Debug.Log("user enter with id " + socketMessage.FromId);
                        syncContext.Post(new SendOrPostCallback(o =>
                        {
                            StartCoroutine(SendOffer(socketMessage.FromId));
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


        syncContext.Post(new SendOrPostCallback(o =>
        {

                Debug.Log($"Add Tracks from {userProfile.username}");
                CaptureAudio();
                pc.AddTrack(localAudioStream);
            
        }), null);

        pc2Ontrack = e =>
        {
            if (e.Track is AudioStreamTrack audioTrack)
            {
                OnAddTrack(audioTrack);
            }
        };

        pc.OnTrack = pc2Ontrack;  
    }

    void OnAddTrack(AudioStreamTrack track)
    {
        syncContext.Post(new SendOrPostCallback(o =>
        {
            GameObject newAudioSource = Instantiate(audioSourcePrefab);
            newAudioSource.name = userProfile.name+"-audio";
                
            AudioSource audioSource = newAudioSource.GetComponent<AudioSource>();
            audioSource.SetTrack(track);
            audioSource.loop = true;
            audioSource.Play();
        }), null);
    }

    private void CaptureAudio()
    {
        AudioSource localAudioSource = GetComponent<AudioSource>();
        var deviceName = Microphone.devices[0];
        Microphone.GetDeviceCaps(deviceName, out int minFreq, out int maxFreq);
        var micClip = Microphone.Start(deviceName, true, 1, 48000);

        // set the latency to “0” samples before the audio starts to play.
        while (!(Microphone.GetPosition(deviceName) > 0)) { }

        localAudioSource.clip = micClip;
        localAudioSource.loop = true;
        localAudioSource.Play();
        localAudioStream = new AudioStreamTrack(localAudioSource);
    }

    #endregion

    #region Offer
    IEnumerator SendOffer(string toPeerId)
    {
        Debug.Log($"pc {userId} SendOffer start");

        RTCDataChannelInit conf = new RTCDataChannelInit();
        RTCDataChannel dataChannel = pc.CreateDataChannel("data", conf);
        dataChannel.OnOpen = OnDataChannelOpened;

        CreateNewAvatar(toPeerId, dataChannel);

        dataChannels.Add(dataChannel);
        

        Debug.Log("pc1 createOffer start");
        RTCOfferAnswerOptions offerOptions = new RTCOfferAnswerOptions();
        var op = pc.CreateOffer(ref offerOptions);
        yield return op;

        if (op.IsError)
        {
            Debug.LogError("Error in creating offer in user " + userId);
            Debug.LogError(op.Error);
            yield return null;
        }
        
        else
        {
            var message = new Message
            {
                Type = "offer",
                ToId = toPeerId,
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
            // TODO: add this component to game object 
            CreateNewAvatar(socketMessage.FromId, channel);
            dataChannels.Add(channel);
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

        Debug.Log($"pc {userId} CreateAnswer start");
        RTCOfferAnswerOptions offerOptions = new RTCOfferAnswerOptions();
        var op2 = pc.CreateAnswer(ref offerOptions);
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
        }
        Debug.Log($"pc {userId} OnCreateAnswerSuccess end");
    }

    IEnumerator OnReceiveAnswerSuccess(Message socketMessage)
    {
        RTCSessionDescription desc = JsonConvert.DeserializeObject<RTCSessionDescription>(socketMessage.Data.ToString());

        Debug.Log("pc setRemoteDescription start");
        var op = pc.SetRemoteDescription(ref desc);
        yield return op;

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

    void OnReceiveIce(Message message)
    {
        Debug.Log("Call OnReceiveIce");
        try
        {
            Debug.Log((string)message.Data);
            var iceCandidate = JsonConvert.DeserializeObject<PeerICE>((string)message.Data);
            RTCIceCandidateInit iceCandidateInit = new RTCIceCandidateInit
            {
                candidate = iceCandidate.Candidate,
                sdpMid = iceCandidate.SdpMid,
                sdpMLineIndex = iceCandidate.SdpMLineIndex
            };

            var candidate = new RTCIceCandidate(iceCandidateInit);
            pc.AddIceCandidate(candidate);
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
        foreach(RTCDataChannel channel in dataChannels)
        {
            channel.Send(message);
        }
    }

    public void SendMsg(byte[] message)
    {
        foreach (RTCDataChannel channel in dataChannels)
        {
            channel.Send(message);
        }
    }

    public void OnDataChannelOpened()
    {
        Debug.Log($"{userId} Opened a dataChannel");
    }

    #endregion
}