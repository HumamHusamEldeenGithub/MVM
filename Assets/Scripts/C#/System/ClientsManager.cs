using Mvm;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.WebRTC;
using UnityEngine;

public class ClientsManager : Singleton<ClientsManager>
{
    [SerializeField]
    RoomSpaceController roomPrefab;

    private Vector3 pos = Vector3.zero;

    static Dictionary<string, RoomSpaceController> participantsRoomSpaces;

    override protected void Awake()
    {
        base.Awake();
        participantsRoomSpaces = new Dictionary<string, RoomSpaceController>();
        EventsPool.Instance.AddListener(typeof(WebRTCConnectionClosedEvent), new Action<string>(ClosePeer));
        EventsPool.Instance.AddListener(typeof(HangupEvent), new Action(CloseAllPeers));
        EventsPool.Instance.AddListener(typeof(RemoveScreenEvent), new Action<RoomSpaceController.RoomRenderTexture>(RemovePeer));
    }

    public RoomSpaceController CreateNewRoomSpace(string peerID = "self", RTCDataChannel dataChannel = null, UserProfile.PeerData user = null)
    {
        RoomSpaceController newPeer;
        Debug.Log("Dictionary is " + participantsRoomSpaces.Count);
        if (participantsRoomSpaces.TryGetValue(peerID, out newPeer))
        {
            // This is for reponening.
            Debug.Log("Already exists");
            return newPeer;
        }
        newPeer = InstantiateRoomSpace(peerID);
        pos += new Vector3(50, 0, 0);
        newPeer.transform.position = pos;

        newPeer.Initialize(peerID, dataChannel, user);
        EventsPool.Instance.InvokeEvent(typeof(CreateNewScreenEvent), newPeer.CurrentRoomRenderTexture);
        return newPeer;
    }

    private void Update()
    {
        Debug.LogWarning(participantsRoomSpaces.Count);
    }

    private RoomSpaceController InstantiateRoomSpace(string id)
    {
        var roomSpace = Instantiate(roomPrefab);
        participantsRoomSpaces.Add(id, roomSpace);
        return roomSpace;
    }

    private void RemovePeer(RoomSpaceController.RoomRenderTexture rt)
    {
        participantsRoomSpaces.Remove(rt.renderTexture.name);
    }

    private void CloseAllPeers()
    {
        foreach(var key in participantsRoomSpaces.Keys.ToList())
        {
            ClosePeer(key);
        }
        participantsRoomSpaces = new Dictionary<string, RoomSpaceController> ();
        Debug.Log("Dictionary is " + participantsRoomSpaces.Count);
    }

    private void ClosePeer(string peerID)
    {
        Debug.LogWarning("DISPOSE");
        RoomSpaceController ctrl;
        participantsRoomSpaces.TryGetValue(peerID, out ctrl);
        ctrl.Dispose();
        participantsRoomSpaces.Remove(peerID);
    }

}
