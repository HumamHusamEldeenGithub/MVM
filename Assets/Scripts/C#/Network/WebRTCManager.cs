using Mvm;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.WebRTC;
using UnityEngine;

public class WebRTCManager : MonoBehaviour
{
    AudioStreamTrack localAudioStream;
    Dictionary<string,WebRTCController> webRTCConnections = new Dictionary<string, WebRTCController>();

    private void Awake()
    {
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
        WebRTCController webRTCController = new WebRTCController();
        webRTCController.InitPeerConnection(localAudioStream, peerId);
        webRTCConnections.Add(peerId, webRTCController);
    }

    public void SendOffer(string peerId)
    {
        StartCoroutine(webRTCConnections[peerId].SendOffer());
    }

    public void ReceiveOffer(string peerId, SignalingMessage msg)
    {
        StartCoroutine(webRTCConnections[peerId].OnReceiveOfferSuccess(msg));
    }
    public void ReceiveAnswer(string peerId , SignalingMessage msg)
    {
        StartCoroutine(webRTCConnections[peerId].OnReceiveAnswerSuccess(msg));
    }

    public void ReceiveICE(string peerId, SignalingMessage msg)
    {
        webRTCConnections[peerId].OnReceiveIce(msg);
    }

    public void SetBlendShapesReadyEvent(BlendShapesReadyEvent evnt)
    {
        void SendBlendShapes(BlendShapes blendShapes)
        {
            var json = JsonConvert.SerializeObject(blendShapes);
            foreach (WebRTCController peer in webRTCConnections.Values)
            {
                peer.SendMessageToDataChannels(json);
            }
        }
            
        evnt.AddListener(SendBlendShapes);
    }

    void DisposeWebRTCConnection(string peerId)
    {
        webRTCConnections[peerId].pc?.Close();
        webRTCConnections[peerId].pc = null;
        webRTCConnections.Remove(peerId);
    }

    public void DisposeAllWebRTCConnections()
    {
        foreach(string key in webRTCConnections.Keys)
        {
            DisposeWebRTCConnection(key);
        }
    }
}
