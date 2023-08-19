using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.IO;

public class BuildScript : IPreprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }

    public void OnPreprocessBuild(BuildReport report)
    {
        /*
            UnityEngine.Debug.Log("I am enter");
            string sourcePath = "Assets/Scripts/Python";
            string destinationPath = "Assets/StreamingAssets/Python";

            if (!Directory.Exists(destinationPath))
            {
                Directory.CreateDirectory(destinationPath);
            }

            foreach (string file in Directory.GetFiles(sourcePath))
            {
                File.Copy(file, Path.Combine(destinationPath, Path.GetFileName(file)), true);
            }
        */
    }
}
