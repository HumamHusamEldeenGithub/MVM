using Mvm;
using Newtonsoft.Json;
using System;
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
            BlendShapes responseMessage = JsonConvert.DeserializeObject<BlendShapes>(Encoding.UTF8.GetString(bytes));
            evnt.Invoke(responseMessage);
        }

        dataChannel.OnMessage += OnDataChannelMessage;

        Initialize(ref evnt);
    }
    public void Initialize(ref BlendShapesReadyEvent evnt)
    {
        evnt.AddListener(blendshapeAnimator.SetBlendShapes);
    }

}
