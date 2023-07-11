using System;
using System.Net.Sockets;
using UnityEngine;
using System.Threading;
using Mvm;
using Newtonsoft.Json;
using System.Collections;

public class TrackingReceiver : Singleton<TrackingReceiver>
{
    static public float jawWeight;
    static public float eyeLeftDown;
    static public float eyeLeftUp;
    static public float eyeLeftOut;
    static public float eyeLeftIn;

    static public float eyeRightDown;
    static public float eyeRightUp;
    static public float eyeRightOut;
    static public float eyeRightIn;

    static public float mouthSmile;

    #region Attributes

    [SerializeField]
    private int localPort = 5004;

    #endregion

    #region CachedVars

    NetworkStream pyStream;
    WebRTC_Client selfClient;
    ClientController selfController;
    OrientationProcessor selfOrProcessor;

    #endregion

    #region Private

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
        selfOrProcessor = GetComponentInChildren<OrientationProcessor>();
        selfClient = GetComponentInChildren<WebRTC_Client>();
        selfController = GetComponentInChildren<ClientController>();

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
            yield break;

        // Create Python Server and Stream
        processManager.CreatePythonServer(localPort);
        pyStream = processManager.PythonStream;

        // Start Recieving
        mainThread = new Thread(() =>
        {
            threadRunning = true;
            EventsPool.Instance.InvokeEvent(typeof(CreateAvatarEvent));
            ReceivePyMessages();
        });
        mainThread?.Start();
        yield return null;
    }

    void ReceivePyMessages()
    {
        Debug.Log(@"Listening now on Python server on port " + $"{localPort}");
        while (threadRunning && pyStream.CanRead)
        {
            if (pyStream.DataAvailable)
            {
                // Read the response from the python script
                byte[] messageData = new byte[20000];

                int bytes = pyStream.Read(messageData, 0, messageData.Length);
                if (bytes == 0) return;

                Debug.Log(bytes);

                //Keypoints response = Keypoints.Parser.ParseFrom(messageData, 0, bytes);
                BlendShapes response2 = BlendShapes.Parser.ParseFrom(messageData, 0, bytes);
                Debug.Log(response2.BlendShapes_[25]);

                jawWeight = response2.BlendShapes_[25].Score;

                eyeLeftDown = response2.BlendShapes_[11].Score;
                eyeLeftUp = response2.BlendShapes_[17].Score;
                eyeLeftIn = response2.BlendShapes_[13].Score;
                eyeLeftOut = response2.BlendShapes_[15].Score;

                eyeRightDown = response2.BlendShapes_[12].Score;
                eyeRightUp = response2.BlendShapes_[18].Score;
                eyeRightIn = response2.BlendShapes_[14].Score;
                eyeRightOut = response2.BlendShapes_[16].Score;

                mouthSmile = response2.BlendShapes_[44].Score;

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
