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

    [SerializeField]
    private BlendShapeAnimator faceAnimator;

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
        selfOrProcessor = GetComponentInChildren<OrientationProcessor>();
        selfClient = GetComponentInChildren<WebRTCController>();
        selfController = GetComponentInChildren<ClientsManager>();

        processManager = ProcessManager.Instance;

        // Start Python server
        StartCoroutine(StartRecieving());
    }

    IEnumerator StartRecieving()
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

        pyStream = processManager.PythonStream;

        // Start Recieving
        mainThread = new Thread(() =>
        {
            Debug.Log("Thread?");
            threadRunning = true;
            ReceivePyMessages();
        });

        ClientsManager.Instance.CreateNewFace(ref blendShapesReadyEvent);
        mainThread?.Start();
        yield return null;
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

                //Keypoints response = Keypoints.Parser.ParseFrom(messageData, 0, bytes);
                BlendShapes response = BlendShapes.Parser.ParseFrom(messageData, 0, bytes);
                blendShapesReadyEvent.Invoke(response);

                /* SimpleSocketMessage socketMessage = new SimpleSocketMessage {
                    Message = "test",
                        
                };

                foreach (Blend point in response.Points)
                {
                    socketMessage.Keypoints.Add(new Mvm.Keypoint
                    {
                        X = point.X,
                        Y = point.Y,
                        Z = point.Z,
                    });
                }*/

                /*                    Debug.Log(response.Points[0]);

                                selfClient.SendMsg(JsonConvert.SerializeObject(socketMessage));
                                selfOrProcessor.SetPoints(response);*/
            }
            else
            {
                Debug.Log("No Data Available.");
            }
        }
    }
    
    #endregion

}
