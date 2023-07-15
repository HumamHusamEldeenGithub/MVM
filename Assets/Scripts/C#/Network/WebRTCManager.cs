using Mvm;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.WebRTC;
using UnityEngine;

public class WebRTCManager : MonoBehaviour
{
    SynchronizationContext syncContext;
    AudioStreamTrack localAudioStream;
    Dictionary<string,WebRTCController> webRTCConnections = new Dictionary<string, WebRTCController>();

    private void Awake()
    {
        syncContext = SynchronizationContext.Current;
        EventsPool.Instance.AddListener(typeof(UserEnterRoomEvent),
            new Action<string>(CreateNewWebRTCConnection));

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

    public void CreateNewWebRTCConnection(string peerId)
    {
        syncContext.Post(new SendOrPostCallback(o =>
        {
            GameObject newObj = new GameObject();
            newObj.transform.parent = transform;
            WebRTCController webRTCController = newObj.AddComponent<WebRTCController>();
            webRTCConnections.Add(peerId, webRTCController);
            webRTCController.InitPeerConnection(localAudioStream, peerId);
        }), null);
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

    public void SetBlendShapesReadyEvent(BlendShapesReadyEvent evnt)
    {
        void SendBlendShapes(BlendShapes blendShapes)
        {
            syncContext.Post(new SendOrPostCallback(o =>
            {
                var json = JsonConvert.SerializeObject(blendShapes);
                foreach (WebRTCController peer in webRTCConnections.Values)
                {
                    peer.SendMessageToDataChannels(json);
                }
            }), null);

        }   
        evnt.AddListener(SendBlendShapes);
    }

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
                DisposeWebRTCConnection(key);
            }
        }), null);

    }
}
