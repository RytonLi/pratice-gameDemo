using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FanShapedCollider
{
    public Action<Collider> Enter;
    public Action<Collider> Stay;
    public Action<Collider> Exit;

    private Vector3 m_Start = Vector3.zero; //起始点
    private Vector3 m_Direction = Vector3.zero; //方向
    //相对偏移
    private float m_Length = 0f; //检测长度
    private float m_Angle = 0f; //扇形角度
    private int m_Accurate = 0; //精确度

    protected enum ColliderState
    {
        enter, stay, exit
    }

    //存放检测到的碰撞器
    protected Dictionary<Collider, ColliderState> m_Colliders = new Dictionary<Collider, ColliderState>();

    //设置碰撞器位置
    public void SetPosition(Vector3 start, Vector3 direction)
    {
        m_Start = start;
        m_Direction = direction;
    }

    //设置参数
    public void SetParameter(float length, float angle, int accurate)
    {
        m_Length = length;
        m_Angle = angle;
        m_Accurate = accurate;
    }

    public void Detect()
    {
        Vector3 localDirection = m_Direction;
        //正前方发射
        __CastRay(m_Start, localDirection, m_Length);

        for(int i = 0; i < m_Accurate; i++)
        {
            //依次减少角度，向正前方左右两侧各发射一次
            float localAngle = m_Angle / 2 - i * m_Angle / m_Accurate;
            localDirection = Quaternion.Euler(0f, -localAngle, 0f) * m_Direction;
            __CastRay(m_Start, localDirection, m_Length);

            localDirection = Quaternion.Euler(0f, localAngle, 0f) * m_Direction;
            __CastRay(m_Start, localDirection, m_Length);
        }

        //检查是否有碰撞器离开
        List<Collider> keys = new List<Collider>(m_Colliders.Keys);

        foreach (Collider collider in keys)
        {
            if (m_Colliders[collider] == ColliderState.exit)
            {
                if(Exit != null) Exit(collider);
                m_Colliders.Remove(collider);
            }
            else
            {
                m_Colliders[collider] = ColliderState.exit;
            }
        }
    }

    //发射线
    private void __CastRay(Vector3 start, Vector3 direction, float length)
    {
        //Debug
        Debug.DrawRay(start, direction * length, Color.green);        

        RaycastHit[] hits = Physics.RaycastAll(start, direction, length);
        foreach(RaycastHit hit in hits)
        {
            //判断区分第一次进入还是停留
            if (m_Colliders.ContainsKey(hit.collider))
            {
                if(m_Colliders[hit.collider] == ColliderState.exit)
                {
                    if(Stay != null) Stay(hit.collider);
                    m_Colliders[hit.collider] = ColliderState.stay;
                }
            }
            else
            {
                if(Enter != null) Enter(hit.collider);
                m_Colliders[hit.collider] = ColliderState.enter;
            }
        }
    }
}
