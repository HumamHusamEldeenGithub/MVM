using System;
using System.Net.Sockets;
using UnityEngine;
using System.Threading;

public class ProtoReceiver : MonoBehaviour
{
    #region Static

    string projectPath = "";
    string pyPath = @"";

    #endregion

    #region Network

    TcpClient client;
    NetworkStream stream;

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
            ConnectToServer();
            ReceiveMessage();
        });
    }

    void Start()
    {
        threadRunning = true;
        mainThread?.Start();
    }

    private void OnDestroy()
    {
        threadRunning = false;
        mainThread?.Join();

        client?.Close();
        stream?.Close();
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
        pyProcess.StartInfo.Arguments = projectPath + @"/Scripts/Python/Server.py"; ;
        pyProcess.StartInfo.UseShellExecute = false;
        pyProcess.StartInfo.RedirectStandardOutput = true;
        pyProcess.StartInfo.CreateNoWindow = true;
        pyProcess.Start();
        Debug.Log("Python process has been started");
    }

    void ConnectToServer()
    {
        try
        {
            Debug.Log("Connecting to server");
            client = new TcpClient("localhost", 5004);
            stream = client.GetStream();
            Debug.Log("Connected");
           
        }
        catch(Exception err)
        {
            Debug.LogError("Couldn't connect to server , err : " + err);
        }
    }

    void ReceiveMessage()
    {
        while (threadRunning)
        {
            if (stream.DataAvailable)
            {
                // Read the response from the python script
                byte[] messageData = new byte[9000];

                int bytes = stream.Read(messageData, 0, messageData.Length);
                if (bytes == 0) return;

                Keypoints response = Keypoints.Parser.ParseFrom(messageData, 0, bytes);

                Debug.Log("Received: = " + response.ToString());
            }
            else
            {
                Debug.Log("No Data Available.");
            }
        }
    }

    #endregion

}
