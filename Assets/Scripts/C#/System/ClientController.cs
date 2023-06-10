using System;
using Unity.WebRTC;
using UnityEngine;

public class ClientController : MonoBehaviour
{
    [SerializeField]
    GameObject peerPrefab;

    private void Awake()
    {
        EventsPool.Instance.AddListener(typeof(CreateAvatarEvent), new Action(CreateSelfAvatar));
    }
    public void CreateNewAvatar(string peerId, RTCDataChannel dataChannel)
    {
        GameObject newAvatar = Instantiate(peerPrefab);
        newAvatar.transform.name = peerId;
        newAvatar.GetComponent<PeerController>().SetPeerController(peerId, dataChannel);
    }
    public void CreateSelfAvatar()
    {
        GameObject newAvatar = Instantiate(peerPrefab);
        newAvatar.GetComponent<PeerController>().enabled = false;
    }
}
