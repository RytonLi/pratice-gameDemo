using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardManage : MonoBehaviour
{
    public void AddMessage(string text)
    {
        Num += 1;
        GameObject obj = Instantiate<GameObject>(Message, Content.transform);
        obj.GetComponent<Text>().text = Num + ": " + text;
        obj.SetActive(true);
        Bar.value = 0f;
    }

    public Scrollbar Bar;
    public GameObject Content;

    private GameObject Message;
    public float baseHeight;
    public int Num = 0;

    void Start()
    {
        Message = Resources.Load("Message", typeof(GameObject)) as GameObject;
    }
}
