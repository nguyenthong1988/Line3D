using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Listenner
{
    public object target = null;
    public System.Reflection.MethodInfo callback = null;

    public Listenner(object target, System.Reflection.MethodInfo callback)
    {
        this.target = target;
        this.callback = callback;
    }

    public void Invoke(params object[] args)
    {
        if (target != null && callback != null)
        {
            callback.Invoke(target, args);
        }
    }
}

public class EventManager : ClassSingleton<EventManager>
{
    private static Dictionary<GameEvent, List<Listenner>> m_Subscribers = new Dictionary<GameEvent, List<Listenner>>();

    ////
    ///methodName: must be public
    ///

    public void Register(GameEvent eventID, object target, string methodName)
    {
        var type = target.GetType();
        System.Reflection.MethodInfo invoker = type.GetMethod(methodName);

        if (!m_Subscribers.ContainsKey(eventID))
            m_Subscribers[eventID] = new List<Listenner>();

        if (!IsSubscriberExists(eventID, target))
            m_Subscribers[eventID].Add(new Listenner(target, invoker));
    }

    public void Dispatch(GameEvent eventID, params object[] args)
    {
        if (!m_Subscribers.TryGetValue(eventID, out var subscribers))
            return;

        foreach (var listenner in subscribers)
        {
            listenner.Invoke(args);
        }
    }

    public void UnRegister(GameEvent eventID, object target)
    {
        if (!m_Subscribers.TryGetValue(eventID, out var subscribers))
            return;

        m_Subscribers[eventID].RemoveAll(l => l.target == target);

        if (m_Subscribers[eventID].Count == 0) m_Subscribers.Remove(eventID);
    }

    private static bool IsSubscriberExists(GameEvent eventID, object target)
    {
        if (!m_Subscribers.TryGetValue(eventID, out var subscribers)) return false;

        bool exists = false;

        for (int i = 0; i < subscribers.Count; i++)
        {
            if (subscribers[i] == target)
            {
                exists = true;
                break;
            }
        }

        return exists;
    }

    public void UnRegisterAllInTarget(object target)
    {
        foreach (var subscribers in m_Subscribers.Values)
        {
            subscribers.RemoveAll(l => l.target == target);
        }
    }
}

