using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioCtrl : MonoBehaviour
{
    public void PlayFootstep()
    {
        if (!m_Footstep.isPlaying)
        {
            //Debug.Log("isPlaying");
            m_Footstep.Play();
        }
    }

    public void StopFootstep()
    {
        if (m_Footstep.isPlaying)
        {
            //Debug.Log("isStop");
            m_Footstep.Stop();
        }
    }

    public void PlayEscape()
    {
        if (!m_HasAudioPlayed)
        {
            //Debug.Log("isPlaying");
            m_Escape.Play();
            m_HasAudioPlayed = true;
        }
    }

    public void PlayCaught()
    {
        if (!m_HasAudioPlayed)
        {
            //Debug.Log("isPlaying");
            m_Caught.Play();
            m_HasAudioPlayed = true;
        }
    }

    //Òª²¥·ÅµÄÉùÒô
    private AudioSource m_Footstep;
    private AudioSource m_Escape;
    private AudioSource m_Caught;
    private bool m_HasAudioPlayed = false;

    private void Start()
    {
        m_Footstep = transform.Find("Footstep").gameObject.GetComponent<AudioSource>();
        m_Escape = transform.Find("Escape").gameObject.GetComponent<AudioSource>();
        m_Caught = transform.Find("Caught").gameObject.GetComponent<AudioSource>();

        MyEventSystem.Instance.AddListener("PlayFootstep", PlayFootstep);
        MyEventSystem.Instance.AddListener("StopFootstep", StopFootstep);
        MyEventSystem.Instance.AddListener("PlayEscape", PlayEscape);
        MyEventSystem.Instance.AddListener("PlayCaught", PlayCaught);
    }
}
