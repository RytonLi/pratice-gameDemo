using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/*
 * 自定义Button
 */
public class MyButton : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    public Color PressedColor;

    private UnityEvent m_ClickEvent = new UnityEvent();
    private Color m_NormalColor;

    //添加点击按钮触发事件
    public void AddListener(UnityAction call)
    {
        m_ClickEvent.AddListener(call);
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;
        m_ClickEvent.Invoke(); //按下时触发回调
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        GetComponent<Image>().color = PressedColor;
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
        GetComponent<Image>().color = m_NormalColor;
    }

    private void Start()
    {
        m_NormalColor = GetComponent<Image>().color;
    }
}
