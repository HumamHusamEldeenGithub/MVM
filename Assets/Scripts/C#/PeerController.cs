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

        BlendShapesReadyEvent evnt = new BlendShapesReadyEvent();

        void OnDataChannelMessage(byte[] bytes)
        {
            try
            {
                BlendShapes responseMessage = BlendShapes.Parser.ParseFrom(bytes, 0, bytes.Length);
                Debug.Log(responseMessage.Date);
                //BlendShapes responseMessage = JsonConvert.DeserializeObject<BlendShapes>(Encoding.UTF8.GetString(bytes));
                evnt.Invoke(responseMessage);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        dataChannel.OnMessage += OnDataChannelMessage;

        Initialize(ref evnt);
    }
    public void Initialize(ref BlendShapesReadyEvent evnt)
    {
        evnt.AddListener(blendshapeAnimator.SetBlendShapes);
    }

}
