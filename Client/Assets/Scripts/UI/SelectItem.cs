using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/*
 * 选择物品
 */
public class SelectItem : MonoBehaviour
{
    public int SelectedIndex = -1; //记录当前选择物品下标

    private GameObject[] m_Childs;
    private bool m_IsClear = true; //选择物品是否更新
    private bool m_IsUpdate = false; //是否已清空选择
    private bool m_IsSet = false;

    private void SetSelectedIndex(int index)
    {
        SelectedIndex = index;
        m_IsUpdate = true;
    }

    private void OnEnable()
    {
        if (!m_IsSet)
        {
            m_Childs = new GameObject[transform.childCount];
            for (int i = 0; i < transform.childCount; i++)
            {
                m_Childs[i] = transform.GetChild(i).gameObject;
                //Debug.Log(transform.GetChild(i).name);
            }

            for (int i = 0; i < transform.childCount; i++)
            {
                MyButton myButton = m_Childs[i].GetComponent<MyButton>();
                //Debug.Log(i);
                int tmp = i;
                myButton.AddListener(() => { SetSelectedIndex(tmp); });
            }
        }
        m_IsSet = true;
        //Debug.Log("enable");
    }

    void Update()
    {
        if(SelectedIndex != -1)
        {
            //需要更新时
            if (m_IsUpdate)
            {
                Outline outline = m_Childs[SelectedIndex].GetComponent<Outline>();
                outline.effectDistance = new Vector2(3f, 3f);
                for (int i = 0; i < transform.childCount; i++)
                {
                    if (i == SelectedIndex) continue;

                    outline = m_Childs[i].GetComponent<Outline>();
                    outline.effectDistance = new Vector2(0f, 0f);
                }
                m_IsClear = false;
                m_IsUpdate = false;
            }
        }
        else
        {
            //已清空不再重复清空
            if (!m_IsClear)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    Outline outline = m_Childs[i].GetComponent<Outline>();
                    outline.effectDistance = new Vector2(0f, 0f);
                }
                m_IsClear = true;
            }
        }
    }

}
