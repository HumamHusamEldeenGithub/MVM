using System;
using System.Net.Sockets;
using UnityEngine;
using System.Threading;
using Mvm;
using System.Collections;
using System.Collections.Generic;

public class TrackingReceiver : Singleton<TrackingReceiver>
{
    #region Attributes

    [SerializeField]
    private int localPort = 5004;

    #endregion

    #region CachedVars

    NetworkStream pyStream;
    WebRTCManager webRTCManager;
    ClientsManager selfController;

    #endregion

    #region Private

    BlendShapesReadyEvent blendShapesReadyEvent;
    ProcessManager processManager;
    Thread mainThread = null;
    bool threadRunning = false;
    PeerController peerController;

    #endregion

    #region Monobehaviour

    override protected void Awake()
    {
        base.Awake();
        Initialize();

        EventsPool.Instance.AddListener(typeof(RoomConnectedStatusEvent), new Action<bool>(StartReceiving));
    }

    override protected void OnDestroy()
    {
        threadRunning = false;
        if (mainThread?.IsAlive == true)
        {
            mainThread.Join();
        }
    }

    #endregion

    #region Methods

    void Initialize()
    {
        blendShapesReadyEvent = new BlendShapesReadyEvent();
        processManager = ProcessManager.Instance;

        selfController = GetComponentInChildren<ClientsManager>();
        webRTCManager = GetComponentInChildren<WebRTCManager>();
    }

    public void StartReceiving(bool success)
    {
        if (!success)
            return;
        // Start Python server
        StartCoroutine(startReceivingCoroutine());
    }

    IEnumerator startReceivingCoroutine()
    {
        // Wait for Process Manager to finish
        while (processManager.SetupComplete == ProcessManager.Status.Waiting)
        {
            yield return null;
        }
        if (processManager.SetupComplete != ProcessManager.Status.Ready)
        {
            yield break;
        }

        // Create Python Server and Stream
        processManager.CreatePythonServer(localPort);

        while(processManager.PythonStream == null)
        {
            yield return null;
        }

        pyStream = processManager.PythonStream;

        // Start Recieving
        mainThread = new Thread(() =>
        {
            threadRunning = true;
            ReceivePyMessages();
        });

        peerController = ClientsManager.Instance.CreateNewRoomSpace("self" , null , new UserProfile.PeerData
        {
            Id = UserProfile.Instance.userData.Id,
            Username = UserProfile.Instance.userData.Username,
            Email = UserProfile.Instance.userData.Email,
            AvatarSettings = UserProfile.Instance.userData.AvatarSettings
        }).PeerController;
        mainThread?.Start();
    }

    void ReceivePyMessages()
    {
        while (threadRunning && pyStream.CanRead)
        {
            if (pyStream.DataAvailable)
            {
                // Read the response from the python script
                byte[] messageData = new byte[20000];

                int bytes = pyStream.Read(messageData, 0, messageData.Length);
                if (bytes == 0) return;

                DataChannelMessage response = DataChannelMessage.Parser.ParseFrom(messageData, 0, bytes);

                response.FaceRotationMessage = new FaceRotationKeypointsMessage
                {
                    Point1 = response.TrackingMessage.Keypoints.Keypoints_[234],
                    Point2 = response.TrackingMessage.Keypoints.Keypoints_[152],
                    Point3 = response.TrackingMessage.Keypoints.Keypoints_[454],
                    Point4 = response.TrackingMessage.Keypoints.Keypoints_[10]
                };

                response.TrackingMessage.Keypoints = null;

                peerController.SetTrackingData(response);
                webRTCManager.SendMessageToDataChannel(response);
            }
        }
    }
    
    #endregion

}
