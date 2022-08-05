using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MyEventSystem
{
    //事件表
    private Dictionary<string, Action> m_EventTable = null;

    private MyEventSystem()
    {
        m_EventTable = new Dictionary<string, Action>();
    }

    private static MyEventSystem _Instance;

    public static MyEventSystem Instance
    {
        get
        {
            if (_Instance == null)
                _Instance = new MyEventSystem();
            return _Instance;
        }
    }

    //添加监听事件
    public void AddListener(string eventName, Action eventHandler)
    {
        Action callbacks;
        if(m_EventTable.TryGetValue(eventName, out callbacks))
        {
            m_EventTable[eventName] = callbacks + eventHandler;
        }
        else
        {
            m_EventTable[eventName] = eventHandler;
        }
    }

    //删除指定监听事件
    public void RemoveListener(string eventName, Action eventHandler)
    {
        Action callbacks;
        if (m_EventTable.TryGetValue(eventName, out callbacks))
        {
            callbacks = (Action)Delegate.RemoveAll(callbacks, eventHandler);
            if(callbacks == null)
            {
                m_EventTable.Remove(eventName);
            }
            else
            {
                m_EventTable[eventName] = callbacks;
            }
        }
    }

    //查询监听事件是否存在
    public bool HasListener(string eventName)
    {
        return m_EventTable.ContainsKey(eventName);
    }

    //发送事件
    public void SendEvent(string eventName)
    {
        Action callbacks;
        if (m_EventTable.TryGetValue(eventName, out callbacks))
        {
            callbacks.Invoke();
        }
    }

    //清空事件表
    public void Clear()
    {
        m_EventTable.Clear();
    }
}
