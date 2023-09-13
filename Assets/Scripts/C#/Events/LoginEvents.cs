using UnityEngine.Events;

public class SubmitLoginEvent : UnityEvent<string,string,string> { }

public class LoginStatusEvent : UnityEvent<bool> { }