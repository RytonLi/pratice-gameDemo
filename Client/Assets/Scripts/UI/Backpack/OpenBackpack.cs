using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OpenBackpack : MonoBehaviour
{

    public GameObject Backpack;
    private bool m_IsOpen = false;

    private void SetShopState()
    {
        m_IsOpen = !m_IsOpen;
        if (m_IsOpen)
        {
            Backpack.SetActive(true);
        }
        else
        {
            Backpack.SetActive(false);
            MyEventSystem.Instance.SendEvent("ResetBackpackSelectItem");
        }
    }

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(SetShopState);
    }
}
