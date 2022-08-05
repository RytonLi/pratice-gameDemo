using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

using System.Threading;
using Google.Protobuf;
using TCCamp;
using UnityEngine;

public class Client : MonoBehaviour
{
    //将协议号与返回的消息类型对应上
    private readonly Dictionary<int,Type> _responseMsgDic = new Dictionary<int, Type>()
    {
        {(int)SERVER_CMD.ServerAddRsp,typeof(AddRsp)},
        {(int)SERVER_CMD.ServerLoginRsp,typeof(PlayerLoginRsp)},
        {(int)SERVER_CMD.ServerCreateRsp,typeof(PlayerCreateRsp)},
        {(int)SERVER_CMD.ServerShopcompleteReq,typeof(ShopCompleteReq)},
        {(int)SERVER_CMD.ServerBuyRsp,typeof(PlayerBuyRsp)},
        {(int)SERVER_CMD.ServerBagupdateRsp,typeof(PlayerBagUpdateRsp)},
        {(int)SERVER_CMD.ServerMoneyRsp,typeof(PlayerMoneyRsp)},
    };
    
    public struct NetMsg
    {
        public int cmd;
        public IMessage msg;
    }

    private Queue<NetMsg> receiveQueue; //服务器消息接收队列
    
    public string staInfo = "NULL";             //状态信息
    public string ip = "127.0.0.1";   //输入ip地址
    public string port = "8086";           //输入端口号
    private int _recTimes = 0;                    //接收到信息的次数
    private string _recMes = "NULL";              //接收到的消息
    private Socket _socketSend;                   //客户端套接字，用来链接远端服务器
    
    private byte[] _headBytes;

    void Start()
    {
        if (_headBytes == null)
        {
            char[] head = new[] {'T', 'C'};
            _headBytes=Encoding.Default.GetBytes(head); 
        }
        
        //监听服务器消息
        EventModule.Instance.AddNetEvent((int)SERVER_CMD.ServerAddRsp,OnAddRsp);
        
        ConnectToServer();
    }

    //建立链接
    private void ConnectToServer()
    {
        try
        {
            int _port = Convert.ToInt32(port);             //获取端口号
            string _ip = this.ip;                               //获取ip地址

            //创建客户端Socket，获得远程ip和端口号
            _socketSend = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ip = IPAddress.Parse(_ip);
            IPEndPoint point = new IPEndPoint(ip, _port);

            _socketSend.Connect(point);
            Debug.Log("连接成功 , " + " ip = " + ip + " port = " + _port);
            staInfo = ip + ":" + _port + "  连接成功";
            
            receiveQueue = new Queue<NetMsg>();

            Thread r_thread = new Thread(Received);             //开启新的线程，不停的接收服务器发来的消息，不能放在Update里面接收，会卡死主线程
            r_thread.IsBackground = true;
            r_thread.Start();
        }
        catch (Exception)
        {
            Debug.Log("IP或者端口号错误......");
            staInfo = "IP或者端口号错误......";
        }
    }

    private void Update()
    {
        while (receiveQueue != null && receiveQueue.Count>0)
        {
            NetMsg mgs = receiveQueue.Dequeue();

            EventModule.Instance.Dispatch(mgs.cmd,mgs.msg);
        }
        
    }

    public void SendMsg(int cmd, IMessage msg)
    {
        byte[] body = msg.ToByteArray();

        Int16 length = (Int16)(body.Length + 2);
        byte[] lengthByte = BitConverter.GetBytes(length);
        
        byte[] cmdByte = BitConverter.GetBytes((Int16)cmd);
        
        int packageLength = 4 + length;
        byte[] package = new byte[packageLength];
        Buffer.BlockCopy(_headBytes, 0, package, 0, _headBytes.Length);
        Buffer.BlockCopy(lengthByte, 0, package, 2, lengthByte.Length);
        Buffer.BlockCopy(cmdByte, 0, package, 4, cmdByte.Length);
        Buffer.BlockCopy(body, 0, package, 6, body.Length);
        _socketSend.Send(package);
    }

    byte[] buffer = new byte[1024];
    int pos = 0;

    int CheckPack(int offset, int len)
    {
        if (len - offset < 4) return 0;

        byte[] headBytes = new byte[2];
        byte[] lengthBytes = new byte[2];

        Buffer.BlockCopy(buffer, offset, headBytes, 0, 2);
        if (headBytes[0] != this._headBytes[0] || headBytes[1] != this._headBytes[1]) return -1;
        
        Buffer.BlockCopy(buffer, offset + 2, lengthBytes, 0, 2);
        int msgLen = bytesToInt16(lengthBytes, 0);
        if (offset + 4 + msgLen > len) return 0;

        return offset + 4 + msgLen;
    }

    public void EnqueueMsg(int offset, int packPos)
    {
        byte[] cmdBytes = new byte[2];
        byte[] body = new byte[packPos - offset - 6];

        Buffer.BlockCopy(buffer, offset + 4, cmdBytes, 0, 2);
        int cmd = bytesToInt16(cmdBytes, 0);

        Buffer.BlockCopy(buffer, offset + 6, body, 0, body.Length);

        Type tp;
        if (_responseMsgDic.TryGetValue(cmd, out tp))
        {
            IMessage msg = (IMessage)Activator.CreateInstance(tp);
            msg.MergeFrom(body);
            NetMsg netMsg;
            netMsg.cmd = cmd;
            netMsg.msg = msg;
            receiveQueue.Enqueue(netMsg); //不能直接处理消息，要放到主线程处理消息，在C#线程无法使用Unity对象
        }
    }

    /// <summary>
    /// 接收服务端返回的消息
    /// </summary>
    void Received()
    {
        while (true)
        {
            try
            {
                //实际接收到的有效字节数
                int len = _socketSend.Receive(buffer, pos, 1024 - pos, SocketFlags.None);
                if (len == 0)
                {
                    break;
                }
                else
                {
                    int start = 0;
                    while (true)
                    {
                        int packPos = CheckPack(start, pos + len);
                        if(packPos > 0)
                        {
                            EnqueueMsg(start, packPos);
                        }
                        else if(packPos == 0)
                        {
                            break;
                        }
                        else
                        {
                            Debug.Log("服务器包解析错误...");
                            break;
                        }
                        start = packPos;
                    }
                    if(start != 0)
                    {
                        Buffer.BlockCopy(buffer, start, buffer, 0, pos + len - start);
                    }
                }

                
            }
            catch { }
        }
    }


    private void OnDisable()
    {
        if (_socketSend.Connected)
        {
            try
            {
                _socketSend.Shutdown(SocketShutdown.Both);    //禁用Socket的发送和接收功能
                _socketSend.Close();                          //关闭Socket连接并释放所有相关资源
            }
            catch (Exception e)
            {
                print(e.Message);
            }
        }
    }
    
    
    public static Int16 bytesToInt16(byte[] src, int offset) {  
        Int16 value;    
        value = (Int16) ((src[offset] & 0xFF)   
                         | ((src[offset+1] & 0xFF)<<8));  
        return value;  
    }


    private void OnAddRsp(int cmd, IMessage msg)
    {
        AddRsp rsp = (AddRsp) msg;
        Debug.Log(rsp.Result);
    }
}