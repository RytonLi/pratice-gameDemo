using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 检查是否看到玩家
 */
public class Observer : MonoBehaviour
{
    public Transform Player;
    public GamingEnding gamingEnding;

    bool m_IsPlayerInRange;

    private void OnTriggerEnter(Collider other)
    {
        if(other.transform == Player)
        {
            m_IsPlayerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform == Player)
        {
            m_IsPlayerInRange = false;
        }
    }

    private void Update()
    {
        if (m_IsPlayerInRange)
        {
            //射线检测
            Vector3 direction = Player.position - transform.position + Vector3.up;
            Ray ray = new Ray(transform.position, direction);
            RaycastHit raycastHit;
            if (Physics.Raycast(ray, out raycastHit))
            {
                if(raycastHit.collider.transform == Player)
                {
                    gamingEnding.CaughtPlayer();
                }
            }
        }
    }
}
