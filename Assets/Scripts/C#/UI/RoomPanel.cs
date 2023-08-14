using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomPanel : MonoBehaviour
{
    public void Hangup()
    {
        EventsPool.Instance.InvokeEvent(typeof(HangupEvent));
    }
}
