using UnityEngine;
using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Collections;
using System.Threading.Tasks;
using Unity.VisualScripting.Antlr3.Runtime;
using System.Threading;
using System.Collections.Generic;
using UnityEditor.PackageManager;

public class ProcessManager : Singleton<ProcessManager>
{
    public enum Status
    {
        Waiting,
        Ready,
        NoPython,
        NoMediapipe,
    }

    #region Static

    string projectPath = "";
                
    string pyPath = @"";

    #endregion

    #region Process

    Process pyProcess;
    TaskPool runner;
    TcpClient pyClient;
    NetworkStream pyStream;

    #endregion

    #region Attributes

    public NetworkStream PythonStream
    {
        get { return pyStream; }
    }

    public Status SetupComplete
    {
        get; private set;
    }

    #endregion

    #region Methods

    private void Initialize()
    {
        void initPath()
        {
            string pathVariable = Environment.GetEnvironmentVariable("PATH");
            string[] paths = pathVariable.Split(';');

            string pythonExecutable = "python.exe"; // or "python3.exe" for Python 3.x

            foreach (string path in paths)
            {
                string fullPath = System.IO.Path.Combine(path, pythonExecutable);
                if (System.IO.File.Exists(fullPath))
                {
                    UnityEngine.Debug.Log("Python found at " +  fullPath);
                    pyPath = @path + "\\python.exe";
                    break;
                }
            }
        }
        void initMediapipe()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = pyPath,
                Arguments = projectPath + @"/Scripts/Python/Setup.py",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                RedirectStandardError = true,
            };

            Process process = new Process();
            process.StartInfo = startInfo;
            process.Start();

            process.WaitForExit();

            try
            {
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                if (!string.IsNullOrEmpty(error))
                {
                    SetupComplete = Status.NoMediapipe;
                }
                else
                {
                    bool isModuleInstalled = bool.Parse(output.Trim());
                    SetupComplete = Status.Ready;
                }
            }
            catch
            {
                SetupComplete = Status.NoMediapipe;
            }

            process.Close();
        }

        projectPath = Application.dataPath;
        runner = TaskPool.Instance;

        runner.AddTasks(new List<Action<CancellationToken>>
            {
                token => initPath(),
                token => initMediapipe()
            },
            out _
        );
        
    }

    public void CreatePythonServer(int localPort)
    {
        DestroyPythonServer();
        pyProcess = new Process();
        pyProcess.StartInfo.FileName = pyPath;
        pyProcess.StartInfo.Arguments = projectPath + @"/Scripts/Python/Server.py" + $" {0}" + $" {localPort}";
        pyProcess.StartInfo.UseShellExecute = false;
        pyProcess.StartInfo.RedirectStandardOutput = true;
        pyProcess.StartInfo.CreateNoWindow = true;
        pyProcess.Start();

        runner.AddTask(new Action<CancellationToken>(token => StartPythonStream(localPort)));
    }

    private void StartPythonStream(int localPort)
    {
        pyClient = new TcpClient("localhost", localPort);
        pyStream = pyClient.GetStream();
    }

    private void DestroyPythonServer()
    {
        pyStream?.Dispose();
        pyStream?.Close();

        pyClient?.Dispose();
        pyClient?.Close();

        if (!pyProcess?.HasExited == true)
        {
            pyProcess?.Kill();
        }

    }

    #endregion

    #region MonoBehaviour

    protected override void Awake()
    {
        base.Awake();

        Initialize();
    }
    override protected void OnDestroy()
    {
        DestroyPythonServer();
    }

    #endregion
}
