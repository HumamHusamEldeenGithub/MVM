using System.Collections.Generic;
using UnityEngine;
using Mvm;
using System;
using System.Threading;

public class UserProfile : Singleton<UserProfile>
{
    public UserData userData;

    public string Username
    {
        get; private set;
    }
    public string Password
    {
        get; private set;
    }

    private void LoginUser(string username, string password)
    {
        this.Username = username;
        this.Password = password;

        var runner = TaskPool.Instance;

        async void login(string username, string password)
        {
            LoginUserResponse res = await Server.Login(new LoginUserRequest { Username = username, Password = password });

            if (res == null)
            {
                EventsPool.Instance.InvokeEvent(typeof(LoginStatusEvent), false); 
                return;
            }
            Debug.Log($"Login in succeeded for {username}");
            EventsPool.Instance.InvokeEvent(typeof(LoginStatusEvent), true);
            userData = new UserData
            {
                Username = username,
                Password = password,
                Token = res.Token,
                RefreshToken = res.RefreshToken,
            };
        }

        runner.AddTasks(new List<Action<CancellationToken>>
            {
                token => login(username, password),
            },
            out _
        );
    }
    public class UserData
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public FacialFeatures UserFeatures { get; set; }
        public Gender UserGender { get; set; }
        public int Age { get; set; }
    }

    override protected void Awake()
    {
        EventsPool.Instance.AddListener(typeof(SubmitLoginEvent), new Action<string, string>(LoginUser));
    }
}