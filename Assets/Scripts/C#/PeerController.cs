using Mvm;
using Newtonsoft.Json;
using System.Text;
using Unity.WebRTC;
using UnityEngine;

public class PeerController : MonoBehaviour
{

    [SerializeField]
    GameObject peerPrefab;

    #region CachedVars

    string peerId;
    RTCDataChannel dataChannel;
    OrientationProcessor orProcessor;
    WebRTC_Client webRTC_Client;

    #endregion

    private void Awake()
    {
        orProcessor = GetComponent<OrientationProcessor>();
        webRTC_Client = GetComponent<WebRTC_Client>();
    }

    public void SetPeerController(string peerId, RTCDataChannel dataChannel)
    {
        this.peerId = peerId; 
        this.dataChannel = dataChannel;
        dataChannel.OnMessage = OnDataChannelMessage;
    }

    void OnDataChannelMessage(byte[] bytes)
    {
        BlendShapes responseMessage = JsonConvert.DeserializeObject<BlendShapes>(Encoding.UTF8.GetString(bytes));
        Debug.Log(responseMessage);

/*        Keypoints response = new Keypoints();

        foreach (Mvm.Keypoint point in responseMessage.Keypoints)
        {
            response.Points.Add(new Keypoint
            {
                X = point.X,
                Y = point.Y,
                Z = point.Z,
            });
        }*/
        // TODO: modify SetPoints in order to take blendShapes 
        //orProcessor.SetPoints(responseMessage);
    }
}
