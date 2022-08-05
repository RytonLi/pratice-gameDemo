using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MyEventSystem
{
    //�¼���
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

    //��Ӽ����¼�
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

    //ɾ��ָ�������¼�
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

    //��ѯ�����¼��Ƿ����
    public bool HasListener(string eventName)
    {
        return m_EventTable.ContainsKey(eventName);
    }

    //�����¼�
    public void SendEvent(string eventName)
    {
        Action callbacks;
        if (m_EventTable.TryGetValue(eventName, out callbacks))
        {
            callbacks.Invoke();
        }
    }

    //����¼���
    public void Clear()
    {
        m_EventTable.Clear();
    }
}
