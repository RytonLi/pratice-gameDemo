using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Google.Protobuf;
using TCCamp;

public class BackpackPanel : MonoBehaviour
{
    private int SelectedIndex;

    private class BagItemObj
    {
        public BagItem item;
        public GameObject obj;
    }

    private SortedDictionary<int, BagItemObj> m_Items = new SortedDictionary<int, BagItemObj>();

    //发送更新金钱请求
    private void SendMoneyReq()
    {
        PlayerMoneyReq req = new PlayerMoneyReq();
        req.PlayerID = PlayerInfo.Instance.Id;
        m_Client.SendMsg(((int)CLIENT_CMD.ClientMoneyReq), req);
    }

    //处理更新金钱响应
    private void OnMoneyRsp(int cmd, IMessage msg)
    {
        PlayerMoneyRsp rsp = (PlayerMoneyRsp)msg;
        string str = "返回：" + rsp.Result + ", " + rsp.Reason;
        m_Board.AddMessage(str);

        if (rsp.Result != 0) return;

        PlayerInfo.Instance.Money = rsp.Money;
        m_Shop.SetMoney();

    }

    //发送更新商店请求
    private void SendUpdateBagReq()
    {
        PlayerBagUpdateReq req = new PlayerBagUpdateReq();
        req.PlayerID = PlayerInfo.Instance.Id;
        m_Client.SendMsg(((int)CLIENT_CMD.ClientBagupdateReq), req);
    }

    //处理更新商店响应
    private void OnUpdateBagRsp(int cmd, IMessage msg)
    {
        PlayerBagUpdateRsp rsp = (PlayerBagUpdateRsp)msg;
        string str = "返回：" + rsp.Result + ", " + rsp.Reason;
        m_Board.AddMessage(str);

        if (rsp.Result != 0) return;

        foreach(BagUpdate update in rsp.UpdateList)
        {
            HandleUpdate(update.Cmd, update.Item);
        }
    }

    private void HandleUpdate(int cmd, BagItem item)
    {
        switch (cmd)
        {
            case ((int)ITEM_UPDATE_CMD.ItemAdd):
                {
                    BagItemObj addObj = new BagItemObj();
                    addObj.item = item;
                    m_Items.Add(item.Id, addObj);
                    CreateSingleObj(addObj);
                    break;
                }
            case ((int)ITEM_UPDATE_CMD.ItemDelete):
                {
                    BagItemObj deleteObj = null;
                    if(m_Items.TryGetValue(item.Id, out deleteObj))
                    {
                        GameObject.Destroy(deleteObj.obj);
                        m_Items.Remove(item.Id);
                    }
                    break;
                }
            case ((int)ITEM_UPDATE_CMD.ItemModify):
                {
                    BagItemObj modifyObj = null;
                    if (m_Items.TryGetValue(item.Id, out modifyObj))
                    {
                        modifyObj.item.Num = item.Num;
                        GameObject itemNum = modifyObj.obj.transform.Find("ItemNum").gameObject;
                        itemNum.GetComponent<Text>().text = modifyObj.item.Num.ToString();
                    }
                    break;
                }
        }
    }

    //初始化背包
    private void InitBackpack()
    {
        foreach(BagItem item in PlayerInfo.Instance.m_HoldItems.Values)
        {
            BagItemObj addObj = new BagItemObj();
            addObj.item = item;
            m_Items.Add(item.Id, addObj);
        }
        CreateBackpackItemObj();
    }

    private void CreateBackpackItemObj()
    {
        foreach(BagItemObj cur in m_Items.Values)
        {
            CreateSingleObj(cur);
        }
    }

    private void CreateSingleObj(BagItemObj cur)
    {
        ShopItem curItem = m_Shop.FindShopItem(cur.item.Id);

        cur.obj = Instantiate<GameObject>(m_ItemPrefabs, Content.transform);
        cur.obj.SetActive(true);

        //设置物品图标
        GameObject itemName = cur.obj.transform.Find("ItemName").gameObject;
        GameObject itemNum = cur.obj.transform.Find("ItemNum").gameObject;
        itemName.GetComponent<Text>().text = curItem.Name.ToStringUtf8();
        itemNum.GetComponent<Text>().text = cur.item.Num.ToString();

        //设置物品按钮
        int tmp = cur.item.Id;
        ItemButton itemButton = cur.obj.GetComponent<ItemButton>();
        itemButton.Description = Description;
        itemButton.Item = curItem;
        itemButton.m_Text = Description.GetComponentInChildren<Text>();
        itemButton.onClick.AddListener(() => { SetSelectedIndex(tmp); });
    }

    private void OutlineSelectItem()
    {
        Outline outline = m_Items[SelectedIndex].obj.GetComponent<Outline>();
        outline.effectDistance = new Vector2(3f, 3f);
        foreach (int i in m_Items.Keys)
        {
            if (i == SelectedIndex) continue;

            outline = m_Items[i].obj.GetComponent<Outline>();
            outline.effectDistance = new Vector2(0f, 0f);
        }
    }

    private void ResetBackpackSelectItem()
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

    public Client m_Client;
    public BoardManage m_Board;
    public GameObject Content;
    public GameObject Description;
    public ShopPanel m_Shop;

    private GameObject m_ItemPrefabs;


    void Start()
    {
        MyEventSystem.Instance.AddListener("InitBackpack", InitBackpack);
        MyEventSystem.Instance.AddListener("ResetBackpackSelectItem", ResetBackpackSelectItem);
        MyEventSystem.Instance.AddListener("SendUpdateBagReq", SendUpdateBagReq);
        MyEventSystem.Instance.AddListener("SendMoneyReq", SendMoneyReq);

        EventModule.Instance.AddNetEvent(((int)SERVER_CMD.ServerBagupdateRsp), OnUpdateBagRsp);
        EventModule.Instance.AddNetEvent(((int)SERVER_CMD.ServerMoneyRsp), OnMoneyRsp);

        m_ItemPrefabs = Resources.Load("GameItem", typeof(GameObject)) as GameObject;
    }
}
