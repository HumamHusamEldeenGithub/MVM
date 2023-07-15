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
using Google.Protobuf;
using System.Net.Http;

public class WebRTCController : MonoBehaviour
{
    #region Properties

    public string peerId;

    public RTCPeerConnection pc;

    UserProfile userProfile;

    RTCDataChannel peerDataChannel;

    [SerializeField] GameObject audioSourcePrefab;

    #endregion

    #region MonoBehaviour
    private void Awake()
    {
        userProfile = GetComponent<UserProfile>();
    }

    #endregion

    #region Init Peer
    RTCConfiguration GetSelectedSdpSemantics()
    {
        Debug.Log("GetSelectedSdpSemantics");
        RTCConfiguration config = default;
        config.iceServers = new RTCIceServer[]
        {
            new RTCIceServer { urls = new string[] { "stun:stun.l.google.com:19302" } }
        };

        return config;
    }

    public void InitPeerConnection(AudioStreamTrack localAudioStream, string peerId)
    {
        this.peerId = peerId;

        var configuration = GetSelectedSdpSemantics();
        pc = new RTCPeerConnection(ref configuration);
        Debug.Log($"Created local peer connection object user {userProfile.Username}");

        pc.OnIceCandidate = OnIceCandidate;
        pc.OnIceConnectionChange = OnIceConnectionChange;

        Debug.Log($"Add Tracks from {userProfile.Username}");
        pc.AddTrack(localAudioStream);

        pc.OnTrack = e =>
        {
            if (e.Track is AudioStreamTrack audioTrack)
            {
                OnAddTrack(audioTrack);
            }
        }; ;
    }

    void OnAddTrack(AudioStreamTrack track)
    {
        GameObject newAudioSource = Instantiate(audioSourcePrefab);
        newAudioSource.name = userProfile.name + "-audio";

        AudioSource audioSource = newAudioSource.GetComponent<AudioSource>();
        audioSource.SetTrack(track);
        audioSource.loop = true;
        audioSource.Play();
    }

    #endregion

    #region Offer
    public IEnumerator SendOffer()
    {
        Debug.Log($"pc {userProfile.Username} SendOffer start");

        RTCDataChannelInit conf = new RTCDataChannelInit();
        RTCDataChannel dataChannel = pc.CreateDataChannel("data", conf);
        dataChannel.OnOpen = OnDataChannelOpened;

        peerDataChannel = dataChannel;

        Debug.Log("pc1 createOffer start");
        RTCOfferAnswerOptions offerOptions = new RTCOfferAnswerOptions();
        var op = pc.CreateOffer(ref offerOptions);
        yield return op;

        if (op.IsError)
        {
            Debug.LogError("Error in creating offer in user " + userProfile.Username);
            Debug.LogError(op.Error);
            yield return null;
        }

        else
        {
            var message = new SignalingMessage
            {
                Type = "offer",
                ToId = peerId,
                Data = op.Desc,
            };

            yield return OnCreateOfferSuccess(op.Desc);
            EventsPool.Instance.InvokeEvent(typeof(SignalingMessage), message);
        }
        Debug.Log($"pc {userProfile.Username} SendOffer end");
    }

    IEnumerator OnCreateOfferSuccess(RTCSessionDescription desc)
    {
        Debug.Log($"pc {userProfile.Username} OnCreateOfferSuccess start");
        var op = pc.SetLocalDescription(ref desc);
        yield return op;

        if (op.IsError)
        {
            Debug.LogError("Error in SetLocalDescription in user " + userProfile.Username);
            Debug.LogError(op.Error);
        }
        Debug.Log($"pc {userProfile.Username} OnCreateOfferSuccess end");
    }

    public IEnumerator OnReceiveOfferSuccess(SignalingMessage socketMessage)
    {
        RTCSessionDescription desc = JsonConvert.DeserializeObject<RTCSessionDescription>(socketMessage.Data.ToString());

        // TODO 
        pc.OnDataChannel = channel =>
        {
            peerDataChannel = channel;
        };

        Debug.Log($"pc {userProfile.Username} SetRemoteDescription start");
        var op = pc.SetRemoteDescription(ref desc);
        yield return op;
        if (op.IsError)
        {
            Debug.LogError("Error in SetRemoteDescription in user " + userProfile.Username);
            Debug.LogError(op.Error);
            yield return null;
        }
        Debug.Log($"pc {userProfile.Username} SetRemoteDescription end");

        Debug.Log($"pc {userProfile.Username} CreateAnswer start");
        RTCOfferAnswerOptions offerOptions = new RTCOfferAnswerOptions();
        var op2 = pc.CreateAnswer(ref offerOptions);
        yield return op2;

        if (op2.IsError)
        {
            Debug.LogError("Error in CreateAnswer in user " + userProfile.Username);
            Debug.LogError(op2.Error);
            yield return null;

        }
        else
        {
            yield return OnCreateAnswerSuccess(op2.Desc);
        }
        Debug.Log($"pc {userProfile.Username} CreateAnswer start");
    }

    #endregion

    #region Answer
    IEnumerator OnCreateAnswerSuccess(RTCSessionDescription desc)
    {
        Debug.Log($"pc {userProfile.Username} OnCreateAnswerSuccess start");
        var op2 = pc.SetLocalDescription(ref desc);
        yield return op2;

        if (op2.IsError)
        {
            Debug.LogError("Error in SetLocalDescription in user " + userProfile.Username);
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
            var message = new SignalingMessage
            {
                Type = "answer",
                ToId = peerId,
                Data = answer

            };
            EventsPool.Instance.InvokeEvent(typeof(SignalingMessage), message);
        }
        Debug.Log($"pc {userProfile.Username} OnCreateAnswerSuccess end");
    }

    public IEnumerator OnReceiveAnswerSuccess(SignalingMessage socketMessage)
    {
        RTCSessionDescription desc = JsonConvert.DeserializeObject<RTCSessionDescription>(socketMessage.Data.ToString());

        Debug.Log("pc setRemoteDescription start");
        var op = pc.SetRemoteDescription(ref desc);
        yield return op;

        if (op.IsError)
        {
            Debug.LogError("Error in SetRemoteDescription in user " + userProfile.Username);
            Debug.LogError(op.Error);
            yield return null;
        }
        Debug.Log("pc setRemoteDescription end");
    }

    #endregion

    #region ICE
    void OnIceCandidate(RTCIceCandidate iceCandidate)
    {
        PeerICE peerIce = new PeerICE
        {
            Candidate = iceCandidate.Candidate,
            SdpMid = iceCandidate.SdpMid,
            SdpMLineIndex = iceCandidate.SdpMLineIndex
        };

        SignalingMessage iceMessage = new SignalingMessage
        {
            Type = "ice",
            Data = JsonConvert.SerializeObject(peerIce),
        };
        Debug.Log($"Send ICE from {userProfile.Username}");
        EventsPool.Instance.InvokeEvent(typeof(SignalingMessage), iceMessage);
    }

    public void OnReceiveIce(SignalingMessage message)
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
                Debug.Log($"{userProfile.Username} IceConnectionState: New");
                break;
            case RTCIceConnectionState.Checking:
                Debug.Log($"{userProfile.Username} IceConnectionState: Checking");
                break;
            case RTCIceConnectionState.Closed:
                Debug.Log($"{userProfile.Username} IceConnectionState: Closed");
                EventsPool.Instance.InvokeEvent(typeof(WebRTCConnectionClosedEvent), peerId);
                break;
            case RTCIceConnectionState.Completed:
                Debug.Log($"{userProfile.Username} IceConnectionState: Completed");
                ClientsManager.Instance.CreateNewRoomSpace(peerId, peerDataChannel);
                break;
            case RTCIceConnectionState.Connected:
                Debug.Log($"{userProfile.Username} IceConnectionState: Connected");
                ClientsManager.Instance.CreateNewRoomSpace(peerId, peerDataChannel);
                break;
            case RTCIceConnectionState.Disconnected:
                Debug.Log($"{userProfile.Username} IceConnectionState: Disconnected");
                EventsPool.Instance.InvokeEvent(typeof(WebRTCConnectionClosedEvent), peerId);
                break;
            case RTCIceConnectionState.Failed:
                Debug.Log($"{userProfile.Username} IceConnectionState: Failed");
                EventsPool.Instance.InvokeEvent(typeof(WebRTCConnectionClosedEvent), peerId);
                break;
            case RTCIceConnectionState.Max:
                Debug.Log($"{userProfile.Username} IceConnectionState: Max");
                break;
            default:
                break;
        }
    }

    #endregion

    #region Data Channel

    public void SendMessageToDataChannels(string message)
    {
        if (peerDataChannel.ReadyState == RTCDataChannelState.Open)
        {
            peerDataChannel.Send(message);
        }
    }

    public void SendMessageToDataChannels(byte[] message)
    {
        if (peerDataChannel.ReadyState == RTCDataChannelState.Open)
        {
            peerDataChannel.Send(message);
        }
    }

    public void OnDataChannelOpened()
    {
        Debug.Log($"{userProfile.Username} Opened a dataChannel");
    }

    #endregion
}