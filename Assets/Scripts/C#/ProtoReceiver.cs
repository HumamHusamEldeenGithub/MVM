using System;
using System.Net.Sockets;
using UnityEngine;
using System.Threading;
using Mvm;
using Google.Protobuf;
using System.Net.WebSockets;
using Newtonsoft.Json;

public class ProtoReceiver : MonoBehaviour
{
    #region Static

    string projectPath = "";
    string pyPath = @"";

    #endregion

    #region Network

    TcpClient pyClient ;
    NetworkStream pyStream ;
    ClientWebSocket webSocket;
    WebRTC_Client webRTC; 

    #endregion

    #region Process

    System.Diagnostics.Process pyProcess;

    Thread mainThread = null;
    bool threadRunning = false;

    #endregion

    #region Monobehaviour


    private void Awake()
    {
        projectPath = Application.dataPath;
        if (!InitPaths())
            throw new Exception("Python path was not found!");

        mainThread = new Thread(() =>
        {
            StartPythonServer();
            ConnectToPyServer();
            ReceivePyMessages();
        });
    }

    void Start()
    {
        threadRunning = true;
        mainThread?.Start();
        webRTC = GameObject.Find("C1").GetComponent<WebRTC_Client>();
    }

    private void OnDestroy()
    {
        threadRunning = false;
        mainThread?.Join();

        pyClient?.Close();
        pyStream?.Close();

        pyProcess?.Kill();
    }

    #endregion

    #region Methods

    bool InitPaths()
    {
        projectPath = Application.dataPath;

        string pathVariable = Environment.GetEnvironmentVariable("PATH");
        string[] paths = pathVariable.Split(';');

        string pythonExecutable = "python.exe"; // or "python3.exe" for Python 3.x

        foreach (string path in paths)
        {
            string fullPath = System.IO.Path.Combine(path, pythonExecutable);
            if (System.IO.File.Exists(fullPath))
            {
                Debug.Log($"Python executable found in path: {path}");
                pyPath = @path + "\\python.exe";
                return true;
            }
        }
        return false;
    }

    void StartPythonServer()
    {
        Debug.Log("Starting the python process");
        pyProcess = new System.Diagnostics.Process();
        pyProcess.StartInfo.FileName = pyPath;
        pyProcess.StartInfo.Arguments = projectPath + @"/Scripts/Python/Server.py" + $" {GeneralManager.cameraIndex}";
        pyProcess.StartInfo.UseShellExecute = false;
        pyProcess.StartInfo.RedirectStandardOutput = true;
        pyProcess.StartInfo.CreateNoWindow = true;
        pyProcess.Start();
        Debug.Log("Python process has been started");
    }

    void ConnectToPyServer()
    {
        try
        {
            Debug.Log("Connecting to python server");
            pyClient = new TcpClient("localhost", 5004);
            pyStream = pyClient.GetStream();
            Debug.Log("Connected to python server");
           
        }
        catch(Exception err)
        {
            Debug.LogError("Couldn't connect to server , err : " + err);
        }
    }

    void ReceivePyMessages()
    {
        while (threadRunning)
        {
            try
            {
                if (pyStream.DataAvailable)
                {
                    // Read the response from the python script
                    byte[] messageData = new byte[9000];

                    int bytes = pyStream.Read(messageData, 0, messageData.Length);
                    if (bytes == 0) return;

                    Keypoints response = Keypoints.Parser.ParseFrom(messageData, 0, bytes);
                

                    SimpleSocketMessage socketMessage = new SimpleSocketMessage {
                        Message="test",
                    
                    };
                    foreach (Keypoint point in response.Points)
                    {
                        socketMessage.Keypoints.Add(new Mvm.Keypoint
                        {
                            X = point.X,
                            Y = point.Y,
                            Z = point.Z,
                        });
                    }


                    // Testing ...
                    webRTC.SendMsg(JsonConvert.SerializeObject(socketMessage));

                    //OrientationProcessor.SetPoints(response);
                }
                else
                {
                    //Debug.Log("No Data Available.");
                }
            } catch(Exception e)
            {
                Debug.LogWarning(e);
            }
        }
    }
    #endregion

}
