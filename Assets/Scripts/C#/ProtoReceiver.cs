
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class ProtoReceiver : MonoBehaviour
{
    TcpClient client;
    NetworkStream stream;
    string pyPath = @"C:\Users\Humam\AppData\Local\Microsoft\WindowsApps\PythonSoftwareFoundation.Python.3.10_qbz5n2kfra8p0\python.exe";
    

    void Start()
    {
        StartPythonServer();
        ConnectToServer();
    }

    void StartPythonServer()
    {
        System.Diagnostics.Process process = new System.Diagnostics.Process();
        process.StartInfo.FileName = pyPath;
        process.StartInfo.Arguments = Application.dataPath + @"/Scripts/Python/Server.py";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.CreateNoWindow = true;
        process.Start();
    }

    void ConnectToServer()
    {
        try
        {
            client = new TcpClient("localhost", 5000);
            stream = client.GetStream();
            StartCoroutine(ReceiveLoop());
        }
        catch(Exception err)
        {
            Debug.LogError("Couldn't connect to server , err : " + err);
        }
    }

    IEnumerator ReceiveLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.05f);
            ReceiveMessage();
        }
    }

    void ReceiveMessage()
    {
        // Read the response from the python script
        byte[] messageData = new byte[4096];
        
        int bytes = stream.Read(messageData, 0, messageData.Length);
        if (bytes == 0) return;

        Keypoint response = Keypoint.Parser.ParseFrom(messageData, 0, bytes);

        Debug.Log("Received: X = " + response.X + " Y = " + response.Y);
    }

}
