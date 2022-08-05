using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCtrl : MonoBehaviour
{

    private const float INSPECTOR_SPACE = 10f;

    public GameObject Character;             //在unity hierarchy面拖进去相应节点
    [Space(INSPECTOR_SPACE)]
    public bool EnabledFollow = true;
    [Space(INSPECTOR_SPACE)]
    public float Fov = 48f;
    [Space(INSPECTOR_SPACE)]
    public float FollowSpeed = 6f; //跟随速度
    [Space(INSPECTOR_SPACE)]
    //相对跟随对象偏移值
    public float OffsetPositionX = 0f;
    public float OffsetPositionY = 6f;
    public float OffsetPositionZ = -5.2f;
    [Space(INSPECTOR_SPACE)]
    //旋转角度
    public float CameraEulerAnglesX = 48f;
    public float CameraEulerAnglesY = 0f;
    public float CameraEulerAnglesZ = 0f;


    private bool m_EnabledUpadte = false;
    private Vector3 m_offsetPos;
    private Vector3 m_offsetEuler;
    private Transform m_characterTrans; //跟随对象transform
    private Transform m_cameraTrans; //相机transform
    private Camera FollowCharacterCamera;
    void Start()
    {
        FollowCharacterCamera = GetComponent<Camera>();
        //检查是否传入对象
        m_EnabledUpadte = Character != null && FollowCharacterCamera != null;
        if (m_EnabledUpadte)
        {
            Reset();
            m_characterTrans = Character.transform;
            m_cameraTrans = FollowCharacterCamera.transform;
            m_cameraTrans.position = m_characterTrans.position + m_offsetPos;
            m_cameraTrans.eulerAngles = m_characterTrans.eulerAngles + m_offsetEuler;
        }
        else
        {
            Debug.LogError("Check this component Character And CameraObject is null!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (m_EnabledUpadte && EnabledFollow)
        {
            Vector3 targetPos = Character.transform.position + m_offsetPos;
            m_cameraTrans.position = Vector3.Lerp(m_cameraTrans.position, targetPos, FollowSpeed * Time.deltaTime); //线性插值
        }

        m_cameraTrans.eulerAngles = new Vector3(CameraEulerAnglesX, CameraEulerAnglesY, CameraEulerAnglesZ);
    }

    //在所有Update结束后进行
    private void LateUpdate()
    {
        Reset();
    }

    private void Reset()
    {
        m_offsetPos.x = OffsetPositionX;
        m_offsetPos.y = OffsetPositionY;
        m_offsetPos.z = OffsetPositionZ;

        if (FollowCharacterCamera)
        {
            FollowCharacterCamera.fieldOfView = Fov;
        }
    }
}
