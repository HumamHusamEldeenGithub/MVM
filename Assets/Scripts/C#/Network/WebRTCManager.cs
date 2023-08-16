using Mvm;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.WebRTC;
using UnityEngine;
using Google.Protobuf;
using System.IO;
using System.Threading.Tasks;

public class WebRTCManager : Singleton<WebRTCManager>
{
    #region Properties
    SynchronizationContext syncContext;
    AudioStreamTrack localAudioStream;
    Dictionary<string,WebRTCController> webRTCConnections = new Dictionary<string, WebRTCController>();
    #endregion

    #region MonoBehaviour
    protected override void Awake()
    {
        syncContext = SynchronizationContext.Current;

        EventsPool.Instance.AddListener(typeof(WebRTCConnectionClosedEvent),
            new Action<string>(DisposeWebRTCConnection));
    }
    public void CaptureAudio()
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

    public async Task CreateNewWebRTCConnection(string peerId)
    {
        TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
        syncContext.Post(new SendOrPostCallback(async o =>
        {
            try
            {
                GameObject newObj = new GameObject();
                newObj.transform.parent = transform;
                WebRTCController webRTCController = newObj.AddComponent<WebRTCController>();
                webRTCConnections.Add(peerId, webRTCController);

                var peerData = await UserProfile.GetPeerData(peerId);
                if (peerData.AvatarSettings == null)
                {
                    peerData.AvatarSettings = UserProfile.GetDefaultAvatarSettings().AvatarSettings;
                }

                webRTCController.InitPeerConnection(localAudioStream, peerId, peerData);
                tcs.SetResult(true);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        }), null);

        await tcs.Task;
    }

    public void SendOffer(string peerId)
    {
        syncContext.Post(new SendOrPostCallback(o =>
        {
            StartCoroutine(webRTCConnections[peerId].SendOffer());
        }), null);
    }

    public void ReceiveOffer(string peerId, SignalingMessage msg)
    {
        syncContext.Post(new SendOrPostCallback(o =>
        {
            StartCoroutine(webRTCConnections[peerId].OnReceiveOfferSuccess(msg));
        }), null);
    }
    public void ReceiveAnswer(string peerId , SignalingMessage msg)
    {
        syncContext.Post(new SendOrPostCallback(o =>
        {
            StartCoroutine(webRTCConnections[peerId].OnReceiveAnswerSuccess(msg));
        }), null);
    }

    public void ReceiveICE(string peerId, SignalingMessage msg)
    {
        syncContext.Post(new SendOrPostCallback(o =>
        {
            webRTCConnections[peerId].OnReceiveIce(msg);
        }), null);
        
    }

    public void SendMessageToDataChannel(DataChannelMessage message)
    {
        syncContext.Post(new SendOrPostCallback(o =>
        {
            message.TrackingMessage.Date = TimestampController.apiDate.ToString();
            byte[] byteArray;
            using (var memoryStream = new MemoryStream())
            {
                message.WriteTo(memoryStream);
                byteArray = memoryStream.ToArray();
            }
                
            foreach (WebRTCController peer in webRTCConnections.Values)
            {
                peer.SendMessageToDataChannel(byteArray);
            }
        }), null);

    }

    public static void PublishAvatarSettingsToPeer(RTCDataChannel dataChannel)
    {

        byte[] byteArray;
        using (var memoryStream = new MemoryStream())
        {
            DataChannelMessage msg = new DataChannelMessage
            {
                Type = DataChannelMessageType.AvatarMessage,
                AvatarMessage = UserProfile.Instance.userData.AvatarSettings
            };
            msg.WriteTo(memoryStream);
            byteArray = memoryStream.ToArray();
        }
        dataChannel.Send(byteArray);
    }
    public void PublishAvatarSettingsToAll()
    {
        syncContext.Post(new SendOrPostCallback(o =>
        {
            byte[] byteArray;
            using (var memoryStream = new MemoryStream())
            {
                DataChannelMessage msg = new DataChannelMessage
                {
                    Type = DataChannelMessageType.AvatarMessage,
                    AvatarMessage = UserProfile.Instance.userData.AvatarSettings
                };
                msg.WriteTo(memoryStream);
                byteArray = memoryStream.ToArray();
            }
            foreach (WebRTCController peer in webRTCConnections.Values)
            {
                peer.SendMessageToDataChannel(byteArray);
            }
        }), null);
    }

    #region Dispose
    void DisposeWebRTCConnection(string peerId)
    {
        syncContext.Post(new SendOrPostCallback(o =>
        {
            webRTCConnections[peerId].pc?.Close();
            webRTCConnections[peerId].pc = null;
            webRTCConnections.Remove(peerId);
        }), null);
    }

    public void DisposeAllWebRTCConnections()
    {
        syncContext.Post(new SendOrPostCallback(o =>
        {
            foreach (string key in webRTCConnections.Keys)
            {
                Debug.Log(key);
                DisposeWebRTCConnection(key);
            }
        }), null);
    }
    #endregion
}
