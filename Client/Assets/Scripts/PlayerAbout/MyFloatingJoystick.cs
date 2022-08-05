using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MyFloatingJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    //摇杆方向
    public float Horizontal;
    public float Vertical;

    //鼠标按下时设置背景、摇杆坐标
    public virtual void OnPointerDown(PointerEventData eventData)
    {
        Vector3 placePosition = Vector3.zero;
        //将二维坐标映射为三维坐标
        RectTransformUtility.ScreenPointToWorldPointInRectangle(GetComponent<RectTransform>(), eventData.position, m_Camera, out placePosition);
        m_Background.GetComponent<RectTransform>().position = placePosition;
        m_Handle.GetComponent<RectTransform>().position = placePosition;
        m_Background.SetActive(true);
    }

    //拖拽鼠标时，设置摇杆方向，设置摇杆显示位置
    public virtual void OnDrag(PointerEventData eventData)
    {  
        
        Vector2 position = RectTransformUtility.WorldToScreenPoint(m_Camera, m_Background.GetComponent<RectTransform>().position);
        //鼠标所在点减去背景所在点，得到方向
        m_Input = eventData.position - position;
        m_Input.Normalize();
        Horizontal = m_Input.x;
        Vertical = m_Input.y;

        //设置摇杆位置为鼠标位置
        Vector3 handlePosition = Vector3.zero;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(GetComponent<RectTransform>(), eventData.position, m_Camera, out handlePosition);
        RectTransform backgroundRect = m_Background.GetComponent<RectTransform>();
        //不超过背景的边界
        m_Handle.GetComponent<RectTransform>().position = Vector3.MoveTowards(backgroundRect.position, handlePosition, 0.2f);
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
        m_Background.SetActive(false);
        //鼠标松开时将方向归零
        Horizontal = 0f;
        Vertical = 0f;
    }

    private GameObject m_Background;
    private GameObject m_Handle;
    private Canvas m_Canvas;
    private Camera m_Camera;
    private Vector2 m_Input;

    private void Start()
    {
        m_Background = transform.Find("Background").gameObject;
        m_Handle = m_Background.transform.Find("Handle").gameObject;
        m_Canvas = GetComponentInParent<Canvas>();

        m_Camera = null;
        if (m_Canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            m_Camera = m_Canvas.worldCamera;
        }
    }
}
