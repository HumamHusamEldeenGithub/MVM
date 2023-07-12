using Mvm;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.WebRTC;
using UnityEngine;

public class ClientsManager : Singleton<ClientsManager>
{
    [SerializeField]
    PeerController peerPrefab;

    override protected void Awake()
    {
        base.Awake();
    }

    public void CreateNewFace(string peerID, RTCDataChannel dataChannel)
    {
        PeerController newPeer = Instantiate(peerPrefab);
        newPeer.Initialize(peerID, dataChannel);
    }

    public void CreateNewFace(ref BlendShapesReadyEvent evnt)
    {

        PeerController newPeer = Instantiate(peerPrefab);
        newPeer.Initialize(ref evnt);
    }


}
