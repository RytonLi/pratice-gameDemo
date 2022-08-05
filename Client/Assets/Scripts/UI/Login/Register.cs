using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Google.Protobuf;
using TCCamp;
using System.Text;

public class Register : MonoBehaviour
{
    private void SendRegisterReq()
    {
        PlayerCreateReq req = new PlayerCreateReq();
        req.PlayerID = Id.text;
        req.Password = Password.text;
        req.Name = ByteString.CopyFrom(Encoding.Default.GetBytes(Name.text));

        m_Client.SendMsg(((int)CLIENT_CMD.ClientCreateReq), req);
    }

    private void OnRegitserRsp(int cmd, IMessage msg)
    {
        PlayerCreateRsp rsp = (PlayerCreateRsp)msg;
        string str = "·µ»Ø£º" + rsp.Result + ", " + rsp.Reason;
        m_Board.AddMessage(str);

        if (rsp.Result != 0) return;

        PlayerSyncData syncData = rsp.PlayerData;
        PlayerInfo.Instance.Init(syncData);

        str = "»¶Ó­Äú£¬" + PlayerInfo.Instance.Name;
        m_Board.AddMessage(str);
        m_Login.SetActive(false);

        MyEventSystem.Instance.SendEvent("EnablePlayerMove");
    }

    public InputField Id;
    public InputField Password;
    public InputField Name;
    public Client m_Client;
    public BoardManage m_Board;
    public GameObject m_Login;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(SendRegisterReq);
        EventModule.Instance.AddNetEvent(((int)SERVER_CMD.ServerCreateRsp), OnRegitserRsp);
    }

}
