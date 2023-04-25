using System;
using System.Net.Sockets;
using UnityEngine;
using System.Threading;
using Mvm;
using Google.Protobuf;
using System.Net.WebSockets;

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

    #endregion

    #region Process

    System.Diagnostics.Process pyProcess;

    Thread mainThread = null;
    Thread serverThread = null;
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
        serverThread = new Thread(()=> {
            ConnectToMVMServer();
            ReceiveServerMessagesAsync();
        });
    }

    void Start()
    {
        threadRunning = true;
        mainThread?.Start();
        serverThread?.Start();
    }

    private void OnDestroy()
    {
        threadRunning = false;
        mainThread?.Join();
        serverThread?.Join();

        pyClient?.Close();
        pyStream?.Close();

        webSocket?.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);

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

    async void ConnectToMVMServer()
    {
        try
        {
            Debug.Log("Connecting to MVM server ...");

            string host = "ws://ec2-16-170-170-2.eu-north-1.compute.amazonaws.com:3000";

            string url = host+ "/websocket?room=8fc3e523-42ec-4154-b341-44bf35a559c2";

            // Create a new instance of ClientWebSocket
            webSocket = new ClientWebSocket();
            webSocket.Options.SetRequestHeader("Authorization", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJleHAiOjE2ODI0NTQzNjAsInVzZXJpZCI6IjRkMWMyOGFlLTU1ZmEtNDhhMS1iMTU1LTQxZWM5MGY5ZjA4MSIsInJvbGUiOiIxIn0.SI1xjMoaVXGJw9zFHP2GCCoiz7QxdpQOWju3URKdAWo");
            
            // Connect to the server
            await webSocket.ConnectAsync(new Uri(url), CancellationToken.None);

            Debug.Log("Connected to MVM server");
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception: {0}", e);
        }
    }

    async void ReceivePyMessages()
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


                    byte[] messageBytes = socketMessage.ToByteArray();
                    var messageSegment = new ArraySegment<byte>(messageBytes);
                    await webSocket.SendAsync(messageSegment, WebSocketMessageType.Binary, true, CancellationToken.None);

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
    async void ReceiveServerMessagesAsync()
    {
        while (threadRunning)
        {
            try
            {
                byte[] responseBuffer = new byte[9000];
                WebSocketReceiveResult responseResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(responseBuffer), CancellationToken.None);

                // Create a Protobuf message instance and deserialize the response message into it
                SocketMessage2 responseMessage = SocketMessage2.Parser.ParseFrom(responseBuffer, 0, responseResult.Count);

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
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
        }
    }

    #endregion

}
