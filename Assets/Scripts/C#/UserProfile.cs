using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mvm;
using System.Threading.Tasks;

public class UserProfile : MonoBehaviour
{
    public UserData userData;
    [SerializeField] public string username;
    [SerializeField] string password;

    public bool login;

    public async Task LoginUser()
    {
        LoginUserResponse res = await Server.Login(new LoginUserRequest { Username = username, Password=password });
        if (res == null) return;
        Debug.Log($"Login in succeeded for {username}");
        userData = new UserData
        {
            Username = username,
            Password = password,
            Token = res.Token,
            RefreshToken = res.RefreshToken
        };
    }

    [SerializeField]
    public class UserData
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }

    private async void Update()
    {
        if (login)
        {
            login = false;
            await LoginUser();
        }
    }
}
