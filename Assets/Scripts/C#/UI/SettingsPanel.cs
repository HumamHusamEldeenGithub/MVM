using UnityEngine;

public class SettingsPanel : MonoBehaviour
{
    public void Exit()
    {
        Application.Quit();
    }

    public void Logout()
    {
        EventsPool.Instance.InvokeEvent(typeof(LogoutEvent));
    }
}
