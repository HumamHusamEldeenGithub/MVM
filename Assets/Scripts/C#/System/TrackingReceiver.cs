using System;
using System.Net.Sockets;
using UnityEngine;
using System.Threading;
using Mvm;
using Newtonsoft.Json;
using System.Collections;
using System.Linq;

public class TrackingReceiver : Singleton<TrackingReceiver>
{
    #region Attributes

    [SerializeField]
    private int localPort = 5004;

    #endregion

    #region CachedVars

    NetworkStream pyStream;
    WebRTCController selfClient;
    ClientsManager selfController;
    OrientationProcessor selfOrProcessor;

    #endregion

    #region Private

    BlendShapesReadyEvent blendShapesReadyEvent;
    ProcessManager processManager;
    Thread mainThread = null;
    bool threadRunning = false;

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

        selfOrProcessor = GetComponentInChildren<OrientationProcessor>();
        selfClient = GetComponentInChildren<WebRTCController>();
        selfController = GetComponentInChildren<ClientsManager>();
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
            Debug.Log("Thread?");
            threadRunning = true;
            ReceivePyMessages();
        });

        ClientsManager.Instance.CreateNewRoomSpace(ref blendShapesReadyEvent);
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

                BlendShapes response = BlendShapes.Parser.ParseFrom(messageData, 0, bytes);
                blendShapesReadyEvent.Invoke(response);

                /* Keypoints Code
                
                Keypoints keypoints = Keypoints.Parser.ParseFrom(messageData, 0, bytes);
                SimpleSocketMessage socketMessage = new SimpleSocketMessage {
                    Message = "test",
                };

                foreach (Blend point in keypoints.Points)
                {
                    socketMessage.Keypoints.Add(new Mvm.Keypoint
                    {
                        X = point.X,
                        Y = point.Y,
                        Z = point.Z,
                    });
                }

                selfClient.SendMsg(JsonConvert.SerializeObject(socketMessage));
                selfOrProcessor.SetPoints(keypoints);

                */
            }
            else
            {
                Debug.Log("No Data Available.");
            }
        }
    }
    
    #endregion

}
