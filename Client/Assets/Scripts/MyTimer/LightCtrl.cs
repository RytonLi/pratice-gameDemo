using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightCtrl : MonoBehaviour
{
    private Light m_Light;
    private TimerManager m_TimerManager;

    private void Start()
    {
        m_Light = GetComponent<Light>();
        m_TimerManager = new TimerManager();
        m_TimerManager.AddTimer(LightFlick, 0f, 0.3f);
    }

    private void Update()
    {
        m_TimerManager.Update();
    }

    private void LightFlick()
    {
        if(Mathf.Approximately(m_Light.intensity, 10f))
        {
            m_Light.intensity = 100f;
        }
        else
        {
            m_Light.intensity = 10f;
        }
    }
}
