using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TCCamp;

public class ItemButton : Button
{
    public override void OnPointerEnter(PointerEventData eventData)
    {
        m_Text.text = "<color=green>��Ʒ���ƣ�</color>" + Item.Name.ToStringUtf8()
                + "\n<color=purple>��Ʒ���ܣ�</color>" + Item.Introduce.ToStringUtf8()
                + "\n<color=red>�۸�</color>" + Item.Price;
        m_Update = true;
        Cursor.visible = false;
        Description.SetActive(true);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        m_Update = false;
        Cursor.visible = true;
        Description.SetActive(false);
    }

    public GameObject Description;
    public ShopItem Item;
    public Text m_Text;

    private bool m_Update;

    private void Update()
    {
        if (m_Update)
        {
            Description.transform.position = Input.mousePosition;
        }
    }

}
