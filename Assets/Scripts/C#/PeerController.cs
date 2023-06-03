using Mvm;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using Unity.WebRTC;
using UnityEngine;

public class PeerController : MonoBehaviour
{
    string peerId;
    RTCDataChannel dataChannel;

    public void SetPeerController(string peerId, RTCDataChannel dataChannel)
    {
        this.peerId = peerId; 
        this.dataChannel = dataChannel;
        dataChannel.OnMessage = OnDataChannelMessage;
    }

    void OnDataChannelMessage(byte[] bytes)
    {
        SocketMessage2 responseMessage = JsonConvert.DeserializeObject<SocketMessage2>(Encoding.UTF8.GetString(bytes));

        Keypoints response = new Keypoints();

        foreach (Mvm.Keypoint point in responseMessage.Keypoints)
        {
            response.Points.Add(new Keypoint
            {
                X = point.X,
                Y = point.Y,
                Z = point.Z,
            });
        }
        OrientationProcessor.SetPoints(response);
    }
}
