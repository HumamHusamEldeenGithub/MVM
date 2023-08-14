using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomPanel : MonoBehaviour
{
    public void Hangup()
    {
        EventsPool.Instance.AddListener(typeof(HangupEvent))
    }
}
