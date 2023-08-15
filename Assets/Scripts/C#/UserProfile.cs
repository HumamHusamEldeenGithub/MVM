using System.Collections.Generic;
using Mvm;
using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.Collections;
using UnityEngine;

public class UserProfile : Singleton<UserProfile>
{
    public UserData userData;
    public string Token, RefreshToken;

    public string Username { get; private set; }
    public string Password { get; private set; }

    private void LoginUser(string username, string password, string refreshToken = "")
    {
        this.Username = username;
        this.Password = password;

        var runner = TaskPool.Instance;

        async void login(string username, string password, string refreshToken = "")
        {
            EventsPool.Instance.InvokeEvent(typeof(ToggleLoadingPanelEvent), true);

            LoginUserResponse res;
            if (refreshToken != "")
            {
                res = await Server.LoginByRefreshToken(new LoginByRefreshTokenRequest { RefreshToken = refreshToken });
            }
            else
            {
                res = await Server.Login(new LoginUserRequest { Username = username, Password = password });
            }
             

            if (res == null)
            {
                EventsPool.Instance.InvokeEvent(typeof(LoginStatusEvent), false);
                EventsPool.Instance.InvokeEvent(typeof(ToggleLoadingPanelEvent), false);
                return;
            }
            else
            {
                userData = new UserData
                {
                    Id = res.Id,
                    Username = username,
                    Password = password,
                    Token = res.Token,
                    RefreshToken = res.RefreshToken,
                };

                Token = res.Token;
                RefreshToken = res.RefreshToken;

                RefreshTokenManager.Instance.StoreRefreshToken(res.RefreshToken);

                await GetMyProfile();
                await GetMyFriends();
                await GetMyNotifications();

                EventsPool.Instance.InvokeEvent(typeof(ConnectToServerEvent));
            }

            EventsPool.Instance.InvokeEvent(typeof(ToggleLoadingPanelEvent), false);
        }

        runner.AddTasks(new List<Action<CancellationToken>>
            {
                token => login(username, password,refreshToken),
            },
            out _
        );
    }

    private void CreateUser(string username,string email,string phonenumber, string password)
    {
        this.Username = username;
        this.Password = password;

        var runner = TaskPool.Instance;

        async void createUser(string username, string email, string phonenumber, string password)
        {
            CreateUserResponse res = await Server.CreateUser(new CreateUserRequest { Username = username, Email = email, Password = password });

            if (res == null)
            {
                EventsPool.Instance.InvokeEvent(typeof(LoginStatusEvent), false);
                return;
            }
            
            userData = new UserData
            {
                Id = res.Id,
                Username = username,
                Password = password,
                Token = res.Token,
                RefreshToken = res.RefreshToken,
            };

            Token = res.Token;
            RefreshToken = res.RefreshToken;

            await GetMyProfile();

            RefreshTokenManager.Instance.StoreRefreshToken(RefreshToken);
            EventsPool.Instance.InvokeEvent(typeof(ShowTakePictuePanelEvent));
            EventsPool.Instance.InvokeEvent(typeof(LoginStatusEvent), true);
        }

        runner.AddTasks(new List<Action<CancellationToken>>
            {
                token => createUser(username,email,phonenumber, password),
            },
            out _
        );
    }

    async Task GetMyProfile()
    {
        var userProfile = await Server.GetProfile("");
        userData.Id = userProfile.Profile.Id;
        userData.Username = userProfile.Profile.Username;
        userData.Email = userProfile.Profile.Email;

        if (userProfile.AvatarSettings == null)
        {
            userData.AvatarSettings = GetDefaultAvatarSettings().AvatarSettings;
        }
        else
        {
            userData.AvatarSettings = userProfile.AvatarSettings;
        }
        if (userProfile.UserRooms != null)
        {
            userData.Rooms = userProfile.UserRooms;
        }
    }

    public async Task GetMyFriends()
    {
        var friends = await Server.GetFriends();
        userData.Friends = friends.Profiles;
        userData.PendingFriendRequests = friends.Pending;
        userData.SentFriendRequests = friends.SentRequests;
    }

    public void GetMyProfile(bool loginStatus)
    {
        if (loginStatus)
        {
            var runner = TaskPool.Instance;
            runner.AddTasks(new List<Action<CancellationToken>>
            {
                token => GetMyProfile(),
            },
                out _
            );

        }
    }
    public async Task GetMyNotifications()
    {
        var notifications = await Server.GetNotifications();
        userData.Notifications = notifications.Notifications;
    }

    public class UserData
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public int Age { get; set; }
        public RepeatedField<Mvm.UserProfile>  Friends { get; set; }
        public RepeatedField<string> PendingFriendRequests { get; set; }
        public RepeatedField<string> SentFriendRequests { get; set; }
        public RepeatedField<Mvm.Notification> Notifications { get; set; }
        public Mvm.AvatarSettings AvatarSettings { get; set; }
        public RepeatedField<Room> Rooms { get; set; }
    }

    public class PeerData
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public Mvm.AvatarSettings AvatarSettings { get; set; }
    }

    override protected void Awake()
    {
        base.Awake();
        EventsPool.Instance.AddListener(typeof(SubmitCreateUserEvent), new Action<string,string,string,string>(CreateUser));
        EventsPool.Instance.AddListener(typeof(SubmitLoginEvent), new Action<string,string,string>(LoginUser));
        EventsPool.Instance.AddListener(typeof(LoginStatusEvent),new Action<bool>(GetMyProfile));
    }

    public static async Task<PeerData> GetPeerData(string peerId)
    {
        var userProfile = await Server.GetUserProfileFeatures(new GetUserProfileFeaturesRequest
        {
            Id = peerId,
        });
        return new PeerData
        {
            Id = userProfile.Profile.Id,
            Username = userProfile.Profile.Username,
            Email = userProfile.Profile.Email,
            AvatarSettings = userProfile.AvatarSettings
        };
    }

    public static PeerData GetDefaultAvatarSettings()
    {
        Mvm.AvatarSettings avatarSettings = new Mvm.AvatarSettings
        {
            HeadStyle = "0",
            HairStyle = "0",
            EyebrowsStyle = "0",
            EyeStyle = "0",
            NoseStyle = "0",
            MouthStyle = "0",
            SkinImperfection = "0",
            Tattoo = "0",
            HairColor = "#000000FF",
            BrowsColor = "#000000FF",
            SkinColor = "#dbc488FF",
            EyeColor = "#000000FF",
            Gender = "Male",
            RoomBackgroundColor= "#000000FF",
        };
        return new PeerData
        {
            AvatarSettings = avatarSettings
        };
    }
}