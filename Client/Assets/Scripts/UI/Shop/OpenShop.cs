using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/*
 * 打开对应菜单
 */
public class OpenShop : MonoBehaviour
{
    public GameObject Shop;
    private bool m_IsOpen = false;

    private void SetShopState()
    {
        m_IsOpen = !m_IsOpen;
        if (m_IsOpen)
        {
            Shop.SetActive(true);
        }
        else
        {
            Shop.SetActive(false);
            MyEventSystem.Instance.SendEvent("ResetShopSelectItem");
        }
    }

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(SetShopState);
    }
}
