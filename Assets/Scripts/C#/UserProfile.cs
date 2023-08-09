using System.Collections.Generic;
using Mvm;
using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.Collections;

public class UserProfile : Singleton<UserProfile>
{
    public UserData userData;
    public string Token, RefreshToken;

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

            EventsPool.Instance.InvokeEvent(typeof(LoginStatusEvent), true);
        }

        runner.AddTasks(new List<Action<CancellationToken>>
            {
                token => login(username, password),
            },
            out _
        );
    }

    private void GetMyProfile(bool loginStatus)
    {
        if (loginStatus)
        {
            var runner = TaskPool.Instance;

            async void getMyProfile()
            {
                var userProfile = await Server.GetProfile();
                userData.Id = userProfile.Profile.Id;
                userData.Email = userProfile.Profile.Email;
                
                if (userProfile.AvatarSettings == null)
                {
                    userData.AvatarSettings = GetDefaultAvatarSettings().AvatarSettings;
                } 
                else
                {
                    userData.AvatarSettings = userProfile.AvatarSettings;
                }
                if (userProfile.UserRooms !=null)
                {
                    userData.Rooms = userProfile.UserRooms;
                }
            }
            runner.AddTasks(new List<Action<CancellationToken>>
            {
                token => getMyProfile(),
            },
                out _
            );

        }
    }
    public class UserData
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public FacialFeatures UserFeatures { get; set; }
        public Gender UserGender { get; set; }
        public int Age { get; set; }
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
        EventsPool.Instance.AddListener(typeof(SubmitLoginEvent), new Action<string, string>(LoginUser));
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
            HeadStyle = "0" , 
            HairStyle = "0",
            EyebrowsStyle = "0" , 
            EyeStyle="0",
            NoseStyle = "0",
            MouthStyle="0",
            SkinImperfection="0",
            Tattoo ="0" , 
            HairColor = "#000000FF" , 
            BrowsColor = "#000000FF",
            SkinColor = "#dbc488FF",
            EyeColor = "#000000FF",
            Gender = "Male",
            RoomBackground= "#000000FF",
        };
        return new PeerData
        {
            AvatarSettings = avatarSettings
        };
    }
}