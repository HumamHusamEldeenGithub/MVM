using Mvm;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Unity.WebRTC;
using UnityEngine;

public class PeerController : MonoBehaviour
{
    #region Private

    public string peerID;
    private RTCDataChannel dataChannel;

    private BlendShapeAnimator blendshapeAnimator;
    private OrientationProcessor orProcessor;
    private AudioSource audioSource;

    #endregion

    private void Awake()
    {
        orProcessor = GetComponent<OrientationProcessor>();
        blendshapeAnimator = GetComponent<BlendShapeAnimator>();
        audioSource = GetComponent<AudioSource>();
    }

    public void Initialize(string peerID, RTCDataChannel dataChannel)
    {
        this.peerID = peerID;
        this.dataChannel = dataChannel;

        void OnDataChannelMessage(byte[] bytes)
        {
            try
            {
/*                DateTime now = DateTime.Now;
                DateTime unixEpoch = new DateTime(2023, 7, 15, 20, 0, 0, DateTimeKind.Utc);

                float seconds = (float)(now - unixEpoch).TotalSeconds;*/
                BlendShapes responseMessage = BlendShapes.Parser.ParseFrom(bytes, 0, bytes.Length);

                SetBlendShapes(responseMessage);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
        if(dataChannel != null)
        {
            dataChannel.OnMessage += OnDataChannelMessage;

            Debug.Log(dataChannel.Id);
        }
        else
        {
            Debug.Log("No channel");
        }
    }

    public void SetBlendShapes(BlendShapes blendshapes)
    {
        blendshapeAnimator.SetBlendShapes(blendshapes);
    }

    public void SetTrack(AudioStreamTrack track)
    {
        audioSource.SetTrack(track);
        audioSource.loop = true;
        audioSource.volume = 1.0f;
        audioSource.Play();
    }
}
