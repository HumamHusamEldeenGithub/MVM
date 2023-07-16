using System.Collections;
using UnityEngine;
using Unity.WebRTC;
using System;
using Newtonsoft.Json;

public class WebRTCController : MonoBehaviour
{
    #region Properties

    public string peerId;

    public RTCPeerConnection pc;

    RTCDataChannel peerDataChannel;

    [SerializeField] GameObject audioSourcePrefab;

    private PeerController peerController;
    private AudioStreamTrack remoteStreamTrack;

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
        Debug.Log($"Created local peer connection object user ");

        pc.OnIceCandidate = OnIceCandidate;
        pc.OnIceConnectionChange = OnIceConnectionChange;
        pc.OnDataChannel = channel =>
        {
            Debug.Log("Call onDataChannel");
            this.peerDataChannel = channel;
            this.peerController = ClientsManager.Instance.CreateNewRoomSpace(peerId, peerDataChannel).PeerController;
            Debug.Log("Call Audio Stream in onDataChannel");
            if (this.remoteStreamTrack != null)
            {
                Debug.Log("Audio Stream Not Null");
                OnAddTrack(this.remoteStreamTrack);
            }

        };

        Debug.Log($"Add Tracks ");
        pc.AddTrack(localAudioStream);

        pc.OnTrack = e =>
        {
            if (e.Track is AudioStreamTrack audioTrack)
            {
                if (peerController != null)
                {
                    OnAddTrack(audioTrack);
                }
                else
                {
                    this.remoteStreamTrack = audioTrack;
                }
            }
        }; ;
    }

    // TODO : find a better way to init remote audio 
    void OnAddTrack(AudioStreamTrack track)
    {
        peerController.SetTrack(track);
    }

    #endregion

    #region Offer
    public IEnumerator SendOffer()
    {
        Debug.Log($"pc SendOffer start");

        RTCDataChannelInit conf = new RTCDataChannelInit
        {
            ordered = true,
            maxRetransmits = 0,
        };
        
        RTCDataChannel dataChannel = pc.CreateDataChannel("data", conf);
        dataChannel.OnOpen = OnDataChannelOpened;

        peerDataChannel = dataChannel;

        Debug.Log("pc1 createOffer start");
        RTCOfferAnswerOptions offerOptions = new RTCOfferAnswerOptions();
        var op = pc.CreateOffer(ref offerOptions);
        yield return op;

        if (op.IsError)
        {
            Debug.LogError("Error in creating offer in user ");
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
            yield return SignalingServerController.SendMessageToServerAsync(message);
        }
        Debug.Log($"pc SendOffer end");
    }

    IEnumerator OnCreateOfferSuccess(RTCSessionDescription desc)
    {
        Debug.Log($"pc OnCreateOfferSuccess start");
        var op = pc.SetLocalDescription(ref desc);
        yield return op;

        if (op.IsError)
        {
            Debug.LogError("Error in SetLocalDescription in user ");
            Debug.LogError(op.Error);
        }
        Debug.Log($"pc OnCreateOfferSuccess end");
    }

    public IEnumerator OnReceiveOfferSuccess(SignalingMessage socketMessage)
    {
        RTCSessionDescription desc = JsonConvert.DeserializeObject<RTCSessionDescription>(socketMessage.Data.ToString());

        Debug.Log($"pc SetRemoteDescription start");
        var op = pc.SetRemoteDescription(ref desc);
        yield return op;
        if (op.IsError)
        {
            Debug.LogError("Error in SetRemoteDescription in user " );
            Debug.LogError(op.Error);
            yield return null;
        }
        Debug.Log($"pc SetRemoteDescription end");

        Debug.Log($"pc CreateAnswer start");
        RTCOfferAnswerOptions offerOptions = new RTCOfferAnswerOptions();
        var op2 = pc.CreateAnswer(ref offerOptions);
        yield return op2;

        if (op2.IsError)
        {
            Debug.LogError("Error in CreateAnswer in user ");
            Debug.LogError(op2.Error);
            yield return null;

        }
        else
        {
            yield return OnCreateAnswerSuccess(op2.Desc);
        }
        Debug.Log($"pc CreateAnswer start");
    }

    #endregion

    #region Answer
    IEnumerator OnCreateAnswerSuccess(RTCSessionDescription desc)
    {
        Debug.Log($"pc OnCreateAnswerSuccess start");
        var op2 = pc.SetLocalDescription(ref desc);
        yield return op2;

        if (op2.IsError)
        {
            Debug.LogError("Error in SetLocalDescription in user ");
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
            yield return SignalingServerController.SendMessageToServerAsync(message);
        }
        Debug.Log($"pc OnCreateAnswerSuccess end");
    }

    public IEnumerator OnReceiveAnswerSuccess(SignalingMessage socketMessage)
    {
        RTCSessionDescription desc = JsonConvert.DeserializeObject<RTCSessionDescription>(socketMessage.Data.ToString());

        Debug.Log("pc setRemoteDescription start");
        var op = pc.SetRemoteDescription(ref desc);
        yield return op;

        if (op.IsError)
        {
            Debug.LogError("Error in SetRemoteDescription in user ");
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

        SignalingMessage iceMessage = new SignalingMessage
        {
            Type = "ice",
            Data = JsonConvert.SerializeObject(peerIce),
        };
        Debug.Log($"Send ICE");
        await SignalingServerController.SendMessageToServerAsync(iceMessage);
    }

    public void OnReceiveIce(SignalingMessage message)
    {
        Debug.Log("Call OnReceiveIce");
        try
        {
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
                Debug.Log($"IceConnectionState: New");
                break;
            case RTCIceConnectionState.Checking:
                Debug.Log($"IceConnectionState: Checking");
                break;
            case RTCIceConnectionState.Closed:
                Debug.Log($"IceConnectionState: Closed");
                EventsPool.Instance.InvokeEvent(typeof(WebRTCConnectionClosedEvent), peerId);
                break;
            case RTCIceConnectionState.Completed:
                Debug.Log($"IceConnectionState: Completed");
                break;
            case RTCIceConnectionState.Connected:
                Debug.Log($"IceConnectionState: Connected");
                if (peerDataChannel != null)
                {
                    peerController = ClientsManager.Instance.CreateNewRoomSpace(peerId, peerDataChannel).PeerController;
                }
                break;
            case RTCIceConnectionState.Disconnected:
                Debug.Log($"IceConnectionState: Disconnected");
                EventsPool.Instance.InvokeEvent(typeof(WebRTCConnectionClosedEvent), peerId);
                break;
            case RTCIceConnectionState.Failed:
                Debug.Log($"IceConnectionState: Failed");
                EventsPool.Instance.InvokeEvent(typeof(WebRTCConnectionClosedEvent), peerId);
                break;
            case RTCIceConnectionState.Max:
                Debug.Log($"IceConnectionState: Max");
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
        Debug.Log($"Opened a dataChannel");
        Debug.Log("Call Audio Stream");
        if (this.remoteStreamTrack != null)
        {
            Debug.Log("Audio Stream Not Null");
            OnAddTrack(this.remoteStreamTrack);
        }
    }

    #endregion
}