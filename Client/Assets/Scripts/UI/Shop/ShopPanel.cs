using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf;
using TCCamp;
using UnityEngine.Events;
using UnityEngine.UI;

public class ShopPanel : MonoBehaviour
{

    private int SelectedIndex;
    private int BuyNum;
    private class ShopItemObj{
        public ShopItem item;
        public GameObject obj;
    }

    private SortedDictionary<int, ShopItemObj> m_Items = new SortedDictionary<int, ShopItemObj>();

    //处理购买响应
    private void OnBuyRsp(int cmd, IMessage msg)
    {
        PlayerBuyRsp rsp = (PlayerBuyRsp)msg;
        string str = "返回：" + rsp.Result + ", " + rsp.Reason;
        m_Board.AddMessage(str);

        if (rsp.Result != 0) return;

        //购买成功，更新背包和金钱
        MyEventSystem.Instance.SendEvent("SendUpdateBagReq");
        MyEventSystem.Instance.SendEvent("SendMoneyReq");
    }

    //确认购买，发送购买请求
    private void ClickSummitBuyButton()
    {
        PlayerBuyReq req = new PlayerBuyReq();
        req.PlayerID = PlayerInfo.Instance.Id;
        req.ItemId = SelectedIndex;
        req.ItemNum = BuyNum;

        m_Client.SendMsg(((int)CLIENT_CMD.ClientBuyReq), req);
        MsgBox.SetActive(false);
    }

    private void ClickCancelBuyButton()
    {
        MsgBox.SetActive(false);
    }

    private void ClickSummitNumButton()
    {
        bool result = int.TryParse(m_NumInput.text, out BuyNum);
        if(result == true)
        {
            ShopItem selectedItem = m_Items[SelectedIndex].item;
            m_MsgText.text = "您确定要花费" + selectedItem.Price * BuyNum +
                "购买 " + BuyNum + "*" + selectedItem.Name.ToStringUtf8() + " 吗？";
            MsgBox.SetActive(true);
            BuyNumBox.SetActive(false);
        }
        else
        {
            string str = "请输入数字...";
            m_Board.AddMessage(str);
        }
    }

    private void ClickCancelNumButton()
    {
        BuyNumBox.SetActive(false);
    }

    private void ClickBuyButton()
    {
        if (SelectedIndex == -1) return;

        BuyNumBox.SetActive(true);
    }

    //收到商店列表响应回调，缓存商店列表
    private void OnShopCompleteReq(int cmd, IMessage msg)
    {
        ShopCompleteReq req = (ShopCompleteReq)msg;
        foreach(ShopItem item in req.Items)
        {
            ShopItemObj addItem = new ShopItemObj();
            addItem.item = item;
            m_Items.Add(item.Id, addItem);
        }

        ShopCompleteRsp rsp = new ShopCompleteRsp();
        rsp.PlayerID = PlayerInfo.Instance.Id;
        rsp.Result = 0;
        rsp.Reason = "获取完整商店列表成功...";
        m_Client.SendMsg(((int)CLIENT_CMD.ClientShopcompleteRsp), rsp);

        CreateShopItemObj();
        //获得商品列表后再初始化背包，否则缺少商品信息，可以考虑数据本地化
        MyEventSystem.Instance.SendEvent("InitBackpack");
        SetMoney();
    }

    private void OutlineSelectItem()
    {
        Outline outline = m_Items[SelectedIndex].obj.GetComponent<Outline>();
        outline.effectDistance = new Vector2(3f, 3f);
        foreach(int i in m_Items.Keys)
        {
            if (i == SelectedIndex) continue;

            outline = m_Items[i].obj.GetComponent<Outline>();
            outline.effectDistance = new Vector2(0f, 0f);
        }
    }

    private void ResetShopSelectItem()
    {
        SelectedIndex = -1;
        foreach (int i in m_Items.Keys)
        {
            Outline outline = m_Items[i].obj.GetComponent<Outline>();
            outline.effectDistance = new Vector2(0f, 0f);
        }
    }

    private void SetSelectedIndex(int index)
    {
        SelectedIndex = index;
        OutlineSelectItem();
    }

    //根据商店列表生成商店物品对象
    private void CreateShopItemObj()
    {
        foreach(ShopItemObj cur in m_Items.Values)
        {
            cur.obj = Instantiate<GameObject>(m_ItemPrefabs, Content.transform);
            cur.obj.SetActive(true);

            //设置物品图标
            GameObject itemName = cur.obj.transform.Find("ItemName").gameObject;
            GameObject itemNum = cur.obj.transform.Find("ItemNum").gameObject;
            itemName.GetComponent<Text>().text = cur.item.Name.ToStringUtf8();
            itemNum.GetComponent<Text>().text = "";

            //设置物品按钮
            int tmp = cur.item.Id;
            ItemButton itemButton = cur.obj.GetComponent<ItemButton>();
            itemButton.Description = Description;
            itemButton.Item = cur.item;
            itemButton.m_Text = Description.GetComponentInChildren<Text>();
            itemButton.onClick.AddListener(() => { SetSelectedIndex(tmp); });
        }
    }

    public ShopItem FindShopItem(int itemID)
    {
        ShopItemObj itemObj = null;
        ShopItem item = null;
        if(m_Items.TryGetValue(itemID, out itemObj))
        {
            item = itemObj.item;
        }
        return item;
    }

    public void SetMoney()
    {
        MoneyText.text = PlayerInfo.Instance.Money.ToString();
    }

    public Client m_Client;
    public BoardManage m_Board;
    public GameObject Content;
    public GameObject Description;
    public Text MoneyText;
    public Button BuyButton;
    public GameObject MsgBox;
    public GameObject BuyNumBox;

    private GameObject m_ItemPrefabs;

    private Text m_MsgText;
    private Button m_SummitBuyButton;
    private Button m_CancelBuyButton;

    private InputField m_NumInput;
    private Button m_SummitNumButton;
    private Button m_CancelNumButton;

    private void Start()
    {
        MyEventSystem.Instance.AddListener("ResetShopSelectItem", ResetShopSelectItem);
        EventModule.Instance.AddNetEvent(((int)SERVER_CMD.ServerShopcompleteReq), OnShopCompleteReq);
        EventModule.Instance.AddNetEvent(((int)SERVER_CMD.ServerBuyRsp), OnBuyRsp);

        m_ItemPrefabs = Resources.Load("GameItem", typeof(GameObject)) as GameObject;

        m_MsgText = MsgBox.transform.Find("MsgText").gameObject.GetComponent<Text>();
        m_SummitBuyButton = MsgBox.transform.Find("Summit").gameObject.GetComponent<Button>();
        m_CancelBuyButton = MsgBox.transform.Find("Cancel").gameObject.GetComponent<Button>();

        m_NumInput = BuyNumBox.transform.Find("NumInput").gameObject.GetComponent<InputField>();
        m_SummitNumButton = BuyNumBox.transform.Find("Summit").gameObject.GetComponent<Button>();
        m_CancelNumButton = BuyNumBox.transform.Find("Cancel").gameObject.GetComponent<Button>();

        BuyButton.onClick.AddListener(ClickBuyButton);

        m_SummitBuyButton.onClick.AddListener(ClickSummitBuyButton);
        m_CancelBuyButton.onClick.AddListener(ClickCancelBuyButton);

        m_SummitNumButton.onClick.AddListener(ClickSummitNumButton);
        m_CancelNumButton.onClick.AddListener(ClickCancelNumButton);

        SelectedIndex = -1;
    }
}
