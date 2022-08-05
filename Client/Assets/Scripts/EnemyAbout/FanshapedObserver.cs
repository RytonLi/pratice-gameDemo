using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FanshapedObserver : MonoBehaviour
{
    private const float INSPECTOR_SPACE = 10f;

    public float OffsetX;
    public float OffsetY;
    public float OffsetZ;
    [Space(INSPECTOR_SPACE)]
    public float Length;
    public float Angle;
    public int Accurate;
    [Space(INSPECTOR_SPACE)]
    public Transform Player;
    public GamingEnding gamingEnding;

    private FanShapedCollider Collider;
    private bool m_IsPlayerInRange;

    private void Enter(Collider other)
    {
        //Debug.Log("enter");
        if (other.transform == Player)
        {
            m_IsPlayerInRange = true;
        }
    }

    private void Exit(Collider other)
    {
        //Debug.Log("exit");
        if (other.transform == Player)
        {
            m_IsPlayerInRange = false;
        }
    }

    private void Start()
    {
        Collider = new FanShapedCollider();
        
        Collider.SetParameter(Length, Angle, Accurate);
        Collider.Enter += Enter;
        Collider.Exit += Exit;
    }

    private void Update()
    {
        Collider.SetPosition(transform.position + new Vector3(OffsetX, OffsetY, OffsetZ), transform.forward);
        Collider.Detect();

        if (m_IsPlayerInRange)
        {
            //…‰œﬂºÏ≤‚
            Vector3 direction = Player.position - transform.position + Vector3.up;
            Ray ray = new Ray(transform.position, direction);
            RaycastHit raycastHit;
            if (Physics.Raycast(ray, out raycastHit))
            {
                if (raycastHit.collider.transform == Player)
                {
                    gamingEnding.CaughtPlayer();
                }
            }
        }
    }
}
