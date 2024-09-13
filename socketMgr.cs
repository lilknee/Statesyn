using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class socketMgr : MonoBehaviour
{
    #region TCP服务
    private Socket socket;
    private Message message;
    // 服务端IP地址
    private string serveIp = "127.0.0.1";
    private int port = 8888;
    public Dictionary<string, GameObject> playerDict=new Dictionary<string, GameObject>();



    //主线程调度用
    void Update()
    {
        if (actions.Count > 0)
        {
            Action action = null;
            lock (queueLock)
            {
                if (actions.Count > 0)
                {
                    
                    action = actions.Dequeue();

                }
            }
            if (action != null)
            {
                action.Invoke();
            }
        }
    }




    public void OnInit()
    {
        message = new Message();
        // 连接服务端TCP
        InitSocket();
        // 等待接收消息
        StartRecevie();

        // 初始化UDP服务
        InitUDP();

    }

    public void OnDestroy()
    {
        CloseSocket();
    }

    private void InitSocket()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try
        {
            // 连接服务端
            socket.Connect(serveIp, port);
        }
        catch (Exception e)
        {
            // 连接失败报错
            Debug.LogException(e);
        }
    }

    /// <summary>
    /// 关闭socket
    /// </summary>
    private void CloseSocket()
    {
        if (socket != null && socket.Connected)
        {
            socket.Close();
            Debug.Log("Close");
        }
    }

    private void StartRecevie()
    {
        socket.BeginReceive(message.Buffer, message.StartIndex,
            message.RemSize, SocketFlags.None,
            RecevieCallback, null);
    }

    private void RecevieCallback(IAsyncResult iar)
    {
        try
        {
            if (socket == null || !socket.Connected)
                return;
            int len = socket.EndReceive(iar);
            if (len <= 0)
            {
                CloseSocket();
                return;
            }
            // 解析消息 回调函数的执行是异步处理的
            message.ReadBuffer(len, HandleResponse);
            StartRecevie();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    /// <summary>
    /// 解析接收到消息后的回调函数
    /// </summary>
    /// <param name="message"></param>
    private void HandleResponse(string message)
    {
        Debug.Log("Received: " + message);
        // 在这里处理接收到的字符串消息

        string[] parts = message.Split(':');
        string ip = "q";
        string port = "q";
        string name = "q";
        string content = "q";
        try
        {
            ip = parts[0];
            port = parts[1];
            name = parts[3];
            content = parts[4];
        }
        catch (IndexOutOfRangeException ex)
        {
            // 打印错误日志以便调试
            Debug.LogError("数组索引超出范围: " + ex.Message);
            return; // 如果失败，直接返回
        }
        catch (Exception ex)
        {
            // 捕获其他类型的异常
            Debug.LogError("发生了错误: " + ex.Message);
            return; // 如果失败，直接返回
        }


        // 将IP地址和端口号组合成EndPoint


        int ports = int.Parse(port);
        IPEndPoint remoteIP = new IPEndPoint(IPAddress.Parse(ip), ports);
        ;

        // 如果消息内容是位置更新的消息，比如 "position:x,y,z"
        string[] positionParts = content.Split(',');
        
        Vector3 newPosition =new Vector3();
        newPosition.x = float.Parse(positionParts[0]);
        newPosition.y = float.Parse(positionParts[1]);
        newPosition.z = float.Parse(positionParts[2]);


        // 检查字典中是否包含这个客户端的remoteIP
        if (!dict.ContainsKey(remoteIP))
        {
            // 如果字典中没有这个remoteIP，添加它并创建角色对象
            dict.Add(remoteIP, content);
            lock (queueLock)
            {
                actions.Enqueue(() => {
                    // 主线程中调用Unity API
                    GameObject character;
                    Debug.Log(name);
                    string prefabName = name.Replace(" ", ""); // 去掉name中的空格
                    GameObject prefab = Resources.Load<GameObject>(prefabName);
                    if (prefab != null)
                    {
                        character = Instantiate(prefab, newPosition, Quaternion.identity);
                        string ip=remoteIP.ToString();
                        
                        Debug.Log("生成对象逻辑");
                        character.AddComponent<PlayerState>();
                        character.AddComponent<Rigidbody>();
                        character.AddComponent<BoxCollider>();
                        BoxCollider boxCollider = character.GetComponent<BoxCollider>();

                        boxCollider.center = new Vector3(0, 0.36f, 0);
                        boxCollider.size = new Vector3(0.3f, 0.5f, 0.5f);
                        PlayerState playerInfo = character.GetComponent<PlayerState>();
                        playerInfo.DeserializePlayerState(content);
                        playerInfo.SerializePlayerState();
                        Debug.Log("同步位置");
                        playerDict.Add(remoteIP.ToString(), character);
                    }
                    else
                    {
                        Debug.LogError("Failed to load the prefab");
                    }
                    
                    
                });
            }
                 

        }
        else
        {

            lock (queueLock)
            {
                actions.Enqueue(() => {
                    // 主线程中调用Unity API
                    // 如果字典中已经有这个remoteIP，更新坐标
                    dict[remoteIP] = content;
                    string remoteip = remoteIP.ToString();
                    Debug.Log(playerDict.Count);
                    GameObject character = playerDict[remoteip];
                    PlayerState playerInfo = character.GetComponent<PlayerState>();
                    playerInfo.DeserializePlayerState(content);
                    Debug.Log("同步位置");


                });
            }


            




        }
    }


    public void Send(string message)
    {
        try
        {
            socket.Send(Message.PackToData(message));
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
    #endregion

    #region UDP服务
    private Socket socketUDP;
    private IPEndPoint iPEndPoint;
    private EndPoint serverEndPoint;
    private byte[] buffer = new byte[1024];
    private Thread receiveThread;
    private readonly Queue<Action> actions = new Queue<Action>();
    private readonly object queueLock = new object();

    // 添加Dictionary来存储客户端的EndPoint和消息
    private Dictionary<EndPoint, string> dict = new Dictionary<EndPoint, string>();

    /// <summary>
    /// UDP服务初始化
    /// </summary>
    private void InitUDP()
    {
        // 创建UDP接套字 消息类型为数据报(Dgram) 协议为UDP
        socketUDP = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        iPEndPoint = new IPEndPoint(IPAddress.Parse(serveIp), port);
        serverEndPoint = iPEndPoint;
       
        try
        {
            // 给UDP套接字设置默认的服务端ip地址和端口号
            // 并尝试进行一次测试通信
            SendUDPTo("fasong");
            
        }
        catch
        {
            Debug.Log("UDP连接失败!");
            return;
        }
        // 创建一个线程接收消息
        receiveThread = new Thread(ReceiveMessage);
        receiveThread.Start();
    }
   

    public void SendUDPTo(string message)
    {
        try
        {
            byte[] buffer = Message.PackToDataUDP(message);
            socketUDP.SendTo(buffer, serverEndPoint); // 使用 SendTo 而不是 Send
            Debug.Log("发送消息");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }



    private void ReceiveMessage()
    {
        while (true)
        {
            try
            {
                
                int len = socketUDP.ReceiveFrom(buffer, ref serverEndPoint);
                Debug.Log("接收消息");
                string receivedMessage = Encoding.UTF8.GetString(buffer, 0, len);

                // 调用 HandleResponse 方法处理接收到的消息
                HandleResponse(receivedMessage);
                
            }
            catch (SocketException e)
            {
                Debug.LogException(e); // 捕捉并记录异常
            }
            catch (ThreadAbortException e)
            {
                Debug.Log("Thread aborted: " + e.Message);
                break; // 线程被中止，退出循环
            }
        }
    }


    /// <summary>
    /// 使用UDP协议发送消息到服务端
    /// </summary>
    /// <param name="message"></param>
    
    #endregion
}
