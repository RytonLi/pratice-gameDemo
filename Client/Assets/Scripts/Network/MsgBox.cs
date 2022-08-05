using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MsgBox : MonoBehaviour
{
    private UnityAction onCancel;
    private UnityAction onConfirm;

    public Text content;
    public Button cancelBtn;
    public Button confirmBtn;

    private void Awake()
    {
        cancelBtn.onClick.AddListener(delegate { gameObject.SetActive(false); onCancel?.Invoke(); });
        
        confirmBtn.onClick.AddListener(delegate { gameObject.SetActive(false); onConfirm?.Invoke(); });
    }

    public void SetData(string info,UnityAction onConfirm = null,UnityAction onCancel = null)
    {
        content.text = info;
        this.onConfirm = onConfirm;
        this.onCancel = onCancel;
    }
}
