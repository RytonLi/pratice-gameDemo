using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Google.Protobuf;
using TCCamp;

public class PlayerMovement : MonoBehaviour
{
    public float turnSpeed = 20f;//旋转速度

    //public MyFloatingJoystick Joystick;
    public Client m_Client;
    public BoardManage m_Board;

    private Animator m_Animator;
    private Rigidbody m_Rigidbody;  //刚体
    private Vector3 m_Movement;     //移动向量
    private Quaternion m_Rotation = Quaternion.identity;//旋转四元数
    private bool m_IsEnable = false;
    bool m_IsWalking;

    private TimerManager m_Timer;

    private void EnablePlayerMove()
    {
        m_IsEnable = true;
    }

    void Start()
    {
        m_Timer = new TimerManager();
        //获取组件
        m_Animator = GetComponent<Animator>();  //获取Animator组件
        m_Rigidbody = GetComponent<Rigidbody>();//获取Rigidbody组件
        MyEventSystem.Instance.AddListener("EnablePlayerMove", EnablePlayerMove);
        MyEventSystem.Instance.AddListener("SetPlayerPosition", SetPlayerPosition);
        MyEventSystem.Instance.AddListener("ResetPosition", ResetPosition);

        EventModule.Instance.AddNetEvent(((int)SERVER_CMD.ServerStatusRsp), OnStatusRsp);
    }

   private void SetPlayerPosition()
    {
        m_Rigidbody.MovePosition(new Vector3(PlayerInfo.Instance.PositionX, 0f,PlayerInfo.Instance.PositionY));
        m_Rigidbody.MoveRotation(Quaternion.Euler(0f, PlayerInfo.Instance.Rotation, 0f));
    }

    private void ResetPosition()
    {
        m_Rigidbody.MovePosition(new Vector3(-9.8f, 0f, -3.2f));
        m_Rigidbody.MoveRotation(Quaternion.Euler(0f, 0f, 0f));
    }

    void FixedUpdate()
    {
        if (m_IsEnable)
        {
            //获取按键输入
            float horizontal = Input.GetAxis("Horizontal"); //水平输入AD键
            float vertical = Input.GetAxis("Vertical");     //竖直输入WS键

            //根据输入设置移动向量
            m_Movement.Set(horizontal, 0f, vertical);//（X,Y,Z）轴
            m_Movement.Normalize();                  //转化为单位向量，表示移动方向

            //根据输入判断是否在行走
            bool hasHorizontalInput = !Mathf.Approximately(horizontal, 0f);//是否有水平输入（根据输入是否等于0）
            bool hasVerticalInput = !Mathf.Approximately(vertical, 0f);    //是否有竖直输入（根据输入是否等于0）
            m_IsWalking = hasHorizontalInput || hasVerticalInput;       //如果有水平或者竖直输入，则判断为在行走。"||"或。

            //设置动画参数
            m_Animator.SetBool("IsWalking", m_IsWalking);

            if (m_IsWalking)
            {
                MyEventSystem.Instance.SendEvent("PlayFootstep");
            }
            else
            {
                MyEventSystem.Instance.SendEvent("StopFootstep");
            }

            //旋转相关
            Vector3 desiredForward = Vector3.RotateTowards
                (transform.forward, m_Movement, turnSpeed * Time.deltaTime, 0f);//（现在前方向量，目标方向向量，角速度，向量大小变化）
            m_Rotation = Quaternion.LookRotation(desiredForward);               //设置旋转四元数，暂时理解为旋转方法要用的参数
        }
    }

    private void LateUpdate()
    {
        if (m_IsEnable)
        {
            PlayerStatusReq req = new PlayerStatusReq();
            req.PlayerID = PlayerInfo.Instance.Id;
            req.PositionX = m_Rigidbody.position.x;
            req.PositionY = m_Rigidbody.position.z;
            req.Rotation = m_Rigidbody.rotation.eulerAngles.y;
            req.IsWalking = m_IsWalking;
            //发送角色状态信息
            m_Client.SendMsg(((int)CLIENT_CMD.ClientStatusReq), req);
        }
    }

    private void OnStatusRsp(int cmd, IMessage msg)
    {
        PlayerStatusRsp rsp = (PlayerStatusRsp)msg;
        
        if(rsp.Result != 0)
        {
            string str = "返回：" + rsp.Result + ", " + rsp.Reason;
            m_Board.AddMessage(str);
        }
    }

    //当播放动画的时候，调用Rigidbody(刚体)控制角色运动
    private void OnAnimatorMove()
    {
        if (m_IsEnable)
        {
            //移动
            m_Rigidbody.MovePosition(m_Rigidbody.position + m_Movement * m_Animator.deltaPosition.magnitude);
            //旋转
            m_Rigidbody.MoveRotation(m_Rotation);
        }
    }
}
