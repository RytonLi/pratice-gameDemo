using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Google.Protobuf;
using TCCamp;
using System.Text;

public class Login : MonoBehaviour
{
    private void SendLoginReq()
    {
        PlayerLoginReq req = new PlayerLoginReq();
        req.PlayerID = Id.text;
        req.Password = Password.text;

        m_Client.SendMsg(((int)CLIENT_CMD.ClientLoginReq), req);
    }

    private void OnLoginRsp(int cmd, IMessage msg)
    {
        PlayerLoginRsp rsp = (PlayerLoginRsp)msg;
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
    public Client m_Client;
    public BoardManage m_Board;
    public GameObject m_Login;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(SendLoginReq);
        EventModule.Instance.AddNetEvent(((int)SERVER_CMD.ServerLoginRsp), OnLoginRsp);
    }

}
