using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class GamingEnding : MonoBehaviour
{
    public float FadeDuration = 1f;
    public float DisplayImageDuration = 1f;
    public GameObject Player;
    public Canvas FaderCanvas;
    public CanvasGroup ExitBackgroundCanvasGroup;
    public CanvasGroup CaughtBackgroundImageCanvasGroup;

    private bool m_IsPlayerAtExit; //玩家是否逃脱
    private bool m_IsPlayerCaught; //玩家是否被怪物抓住
    private float m_Timer;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == Player)
        {
            m_IsPlayerAtExit = true;
        }
    }

    public void CaughtPlayer()
    {
        m_IsPlayerCaught = true;
    }

    private void Start()
    {
        m_Timer = 0f;
    }

    private void Update()
    {
        if (m_IsPlayerAtExit)
        {
            MyEventSystem.Instance.SendEvent("PlayEscape");
            EndLevel(ExitBackgroundCanvasGroup, false);
        }
        else if (m_IsPlayerCaught)
        {
            MyEventSystem.Instance.SendEvent("PlayCaught");
            EndLevel(CaughtBackgroundImageCanvasGroup, true);
        }
    }

    private void EndLevel(CanvasGroup ImageCanvasGroup, bool DoRestart)
    {
        FaderCanvas.sortingOrder = 10;

        m_Timer += Time.deltaTime;

        ImageCanvasGroup.alpha = m_Timer / FadeDuration; //渐变

        if(m_Timer > FadeDuration + DisplayImageDuration)
        {
            if (DoRestart)
            {
                MyEventSystem.Instance.SendEvent("ResetPosition");
                FaderCanvas.sortingOrder = 0;
                ImageCanvasGroup.alpha = 0f;
                m_Timer = 0f;
                m_IsPlayerCaught = false;
            }
            else
            {
                Application.Quit();
            }
        }
    }
}
