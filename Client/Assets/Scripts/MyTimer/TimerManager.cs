using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerManager
{

    public TimerManager()
    {
        m_Timers = new Dictionary<int, TimerNode>();
        m_AddTimers = new List<TimerNode>();
        m_RemoveTimers = new List<TimerNode>();
    }

    public delegate void TimerHandler();
    private class TimerNode
    {
        public TimerHandler callback;
        public float time; //首次调用时间延迟
        public int repeat; //重复次数
        public float repeatRate; //重复间隔
        public float passedTime; //经过时间
        public bool isRemoved; //标记是否移除
        public int id; //标记该定时器
    }

    private Dictionary<int, TimerNode> m_Timers = null; //要执行的Timer

    private List<TimerNode> m_AddTimers = null; //新增Timer缓存
    private List<TimerNode> m_RemoveTimers = null; //删除Timer缓存

    private int m_AutoIncId = 1;

    //添加一个定时器
    //methodName：回调函数
    //time：延迟时间
    //repeatTime：间隔时间
    //repeat：重复次数，<=0表示无限重复
    public int AddTimer(TimerHandler methodName, float time, float repeatTime, int repeat = 0)
    {
        TimerNode timer = new TimerNode();
        timer.callback = methodName;
        timer.time = time;
        timer.repeatRate = repeatTime;
        timer.repeat = repeat;
        timer.passedTime = 0f;
        timer.isRemoved = false;
        timer.id = m_AutoIncId;
        m_AutoIncId++;
        m_AddTimers.Add(timer); //先加到添加缓存
        return timer.id;
    }

    //删除一个定时器
    public void RemoveTimer(int id)
    {
        if (!m_Timers.ContainsKey(id))
        {
            return;
        }
        TimerNode timer = m_Timers[id];
        timer.isRemoved = true; //先标记，延迟删除
    }

    public void Update()
    {
        float dt = Time.deltaTime;
        //添加新的定时器
        for(int i = 0; i < m_AddTimers.Count; i++)
        {
            m_Timers.Add(m_AddTimers[i].id, m_AddTimers[i]);
        }
        m_AddTimers.Clear();

        foreach(TimerNode timer in m_Timers.Values)
        {
            if (timer.isRemoved)
            {
                m_RemoveTimers.Add(timer);
                continue;
            }

            timer.passedTime += dt;
            if(timer.passedTime >= (timer.time + timer.repeatRate))
            {
                timer.callback();
                timer.repeat--;
                timer.passedTime -= (timer.time + timer.repeatRate);
                timer.time = 0f;
                if(timer.repeat == 0)
                {
                    timer.isRemoved = true;
                    m_RemoveTimers.Add(timer);
                }
            }
        }

        //删除定时器
        for (int i = 0; i < m_RemoveTimers.Count; i++)
        {
            m_Timers.Remove(m_RemoveTimers[i].id);
        }
        m_RemoveTimers.Clear();
    }
}
