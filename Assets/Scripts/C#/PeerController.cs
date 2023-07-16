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

    private float stTime = -1;
    public string peerID;
    private RTCDataChannel dataChannel;

    private BlendShapeAnimator blendshapeAnimator;
    private OrientationProcessor orProcessor;

    #endregion

    private void Awake()
    {
        orProcessor = GetComponent<OrientationProcessor>();
        blendshapeAnimator = GetComponent<BlendShapeAnimator>();
    }

    public void Initialize(string peerID, RTCDataChannel dataChannel)
    {
        this.peerID = peerID;
        this.dataChannel = dataChannel;

        void OnDataChannelMessage(byte[] bytes)
        {
            try
            {
                DateTime now = DateTime.Now;
                DateTime unixEpoch = new DateTime(2023, 7, 15, 20, 0, 0, DateTimeKind.Utc);

                float seconds = (float)(now - unixEpoch).TotalSeconds;
                BlendShapes responseMessage = BlendShapes.Parser.ParseFrom(bytes, 0, bytes.Length);
                Debug.LogWarning(responseMessage.Index);
                Debug.Log(seconds - responseMessage.Date);

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
}
