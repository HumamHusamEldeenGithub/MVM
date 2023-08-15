using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEditor.Build.Content;
using UnityEngine.SceneManagement;

public class EventsPool : Singleton<EventsPool>
{
    private Dictionary<Type, Delegate> eventsDictionary;

    private Queue<Tuple<Delegate, object[]>> eventsQueue;

    protected override void Awake()
    {
        base.Awake();
        eventsDictionary = new Dictionary<Type, Delegate>();
        eventsQueue = new Queue<Tuple<Delegate, object[]>>();
        SceneManager.sceneUnloaded += ClearAll;
    }
    public void AddListener(Type eventType, Delegate listener)
    {
        Delegate thisEvent;
        if (eventsDictionary.TryGetValue(eventType, out thisEvent))
        {
            eventsDictionary[eventType] = Delegate.Combine(thisEvent, listener);
        }
        else
        {
            eventsDictionary[eventType] = listener;
        }
    }

    public void RemoveListener(Type eventType, Delegate listener)
    {
        if (eventsDictionary == null) return;
        Delegate thisEvent;
        if (eventsDictionary.TryGetValue(eventType, out thisEvent))
        {
            eventsDictionary[eventType] = Delegate.Remove(thisEvent, listener);
        }
    }

    public void InvokeEvent(Type eventType, params object[] args)
    {
        Delegate thisEvent;
        if (eventsDictionary.TryGetValue(eventType, out thisEvent))
        {
            eventsQueue.Enqueue(new Tuple< Delegate, object[]>(thisEvent, args));
        }
    }

    // This is done because some delegates can be invoked outside of Unity's main thread
    private void Update()
    {
        while(eventsQueue.Count > 0)
        {
            var thisEvent = eventsQueue.Dequeue();
            thisEvent.Item1.DynamicInvoke(thisEvent.Item2);

        }
    }
    private void ClearAll(Scene sc)
    {
        UnityEngine.Debug.Log("Clear");
        eventsQueue.Clear();
        eventsDictionary.Clear();
    }
}