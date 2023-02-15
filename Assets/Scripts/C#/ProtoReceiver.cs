using System;
using System.Net.Sockets;
using UnityEngine;
using System.Threading;

public class ProtoReceiver : MonoBehaviour
{
    TcpClient client;
    NetworkStream stream;
    string pyPath = @"C:\Users\Humam\AppData\Local\Microsoft\WindowsApps\PythonSoftwareFoundation.Python.3.10_qbz5n2kfra8p0\python.exe";
    System.Diagnostics.Process pyProcess;
    string projectPath = "";
    bool stopThread ;

    void Start()
    {
        projectPath = Application.dataPath;
        new Thread(() =>
        {
            StartPythonServer();
            ConnectToServer();
            ReceiveMessage();

        }).Start();

    }

    private void OnDestroy()
    {
        stopThread = true;
        pyProcess.Kill();
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
        while (true)
        {
            if (stopThread) break;

            // Read the response from the python script
            byte[] messageData = new byte[9000];

            int bytes = stream.Read(messageData, 0, messageData.Length);
            if (bytes == 0) return;

            Keypoints response = Keypoints.Parser.ParseFrom(messageData, 0, bytes);

            Debug.Log("Received:  = " + response.ToString());
        }
    }

}
