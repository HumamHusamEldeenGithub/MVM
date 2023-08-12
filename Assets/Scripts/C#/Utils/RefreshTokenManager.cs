using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public class RefreshTokenManager : Singleton<RefreshTokenManager>
{
    string dataFolderPath;
    override protected void Awake()
    {
        base.Awake();
        dataFolderPath = Application.persistentDataPath;
    }
    private string GetTokenFilePath()
    {
        return $"{dataFolderPath}/refresh_token.json";
    }

    public void StoreRefreshToken(string refreshToken)
    {
        Mvm.LoginUserResponse tokenData = new Mvm.LoginUserResponse { RefreshToken = refreshToken };
        string json = JsonConvert.SerializeObject(tokenData);

        string filePath = GetTokenFilePath();

        File.WriteAllText(filePath, json);

        Debug.Log("refreshToken written successfully.");
    }

    public string GetRefreshToken()
    {
        string filePath = GetTokenFilePath();
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            Mvm.LoginUserResponse tokenData = JsonConvert.DeserializeObject<Mvm.LoginUserResponse>(json);
            return tokenData.RefreshToken;
        }
        return null;
    }
}
