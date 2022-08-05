using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 * 购买选择的物品
 */
public class ShopBuyItem : MonoBehaviour
{
    public GameObject Content;
    public GameObject Message;
    public Text MessageText;

    private void Callback()
    {
        int selectedIndex = Content.GetComponent<SelectItem>().SelectedIndex;
        //Debug.Log(selectedIndex);
        if (selectedIndex == -1) return;
        GameObject selectedChild = Content.transform.GetChild(selectedIndex).gameObject;
        string selectedText = selectedChild.transform.GetChild(0).gameObject.GetComponent<Text>().text;
        //Debug.Log(selectedText);
        MessageText.text = "您确定要购买 " + selectedText + " 吗?";

        //Message.transform.localPosition = new Vector3(0f, 0f, 0f);
        Message.SetActive(true);
    }

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<MyButton>().AddListener(Callback);
    }
}
