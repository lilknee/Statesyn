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
    #region TCP����
    private Socket socket;
    private Message message;
    // �����IP��ַ
    private string serveIp = "127.0.0.1";
    private int port = 8888;
    public Dictionary<string, GameObject> playerDict=new Dictionary<string, GameObject>();



    //���̵߳�����
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
        // ���ӷ����TCP
        InitSocket();
        // �ȴ�������Ϣ
        StartRecevie();

        // ��ʼ��UDP����
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
            // ���ӷ����
            socket.Connect(serveIp, port);
        }
        catch (Exception e)
        {
            // ����ʧ�ܱ���
            Debug.LogException(e);
        }
    }

    /// <summary>
    /// �ر�socket
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
            // ������Ϣ �ص�������ִ�����첽�����
            message.ReadBuffer(len, HandleResponse);
            StartRecevie();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    /// <summary>
    /// �������յ���Ϣ��Ļص�����
    /// </summary>
    /// <param name="message"></param>
    private void HandleResponse(string message)
    {
        Debug.Log("Received: " + message);
        // �����ﴦ����յ����ַ�����Ϣ

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
            // ��ӡ������־�Ա����
            Debug.LogError("��������������Χ: " + ex.Message);
            return; // ���ʧ�ܣ�ֱ�ӷ���
        }
        catch (Exception ex)
        {
            // �����������͵��쳣
            Debug.LogError("�����˴���: " + ex.Message);
            return; // ���ʧ�ܣ�ֱ�ӷ���
        }


        // ��IP��ַ�Ͷ˿ں���ϳ�EndPoint


        int ports = int.Parse(port);
        IPEndPoint remoteIP = new IPEndPoint(IPAddress.Parse(ip), ports);
        ;

        // �����Ϣ������λ�ø��µ���Ϣ������ "position:x,y,z"
        string[] positionParts = content.Split(',');
        
        Vector3 newPosition =new Vector3();
        newPosition.x = float.Parse(positionParts[0]);
        newPosition.y = float.Parse(positionParts[1]);
        newPosition.z = float.Parse(positionParts[2]);


        // ����ֵ����Ƿ��������ͻ��˵�remoteIP
        if (!dict.ContainsKey(remoteIP))
        {
            // ����ֵ���û�����remoteIP���������������ɫ����
            dict.Add(remoteIP, content);
            lock (queueLock)
            {
                actions.Enqueue(() => {
                    // ���߳��е���Unity API
                    GameObject character;
                    Debug.Log(name);
                    string prefabName = name.Replace(" ", ""); // ȥ��name�еĿո�
                    GameObject prefab = Resources.Load<GameObject>(prefabName);
                    if (prefab != null)
                    {
                        character = Instantiate(prefab, newPosition, Quaternion.identity);
                        string ip=remoteIP.ToString();
                        
                        Debug.Log("���ɶ����߼�");
                        character.AddComponent<PlayerState>();
                        character.AddComponent<Rigidbody>();
                        character.AddComponent<BoxCollider>();
                        BoxCollider boxCollider = character.GetComponent<BoxCollider>();

                        boxCollider.center = new Vector3(0, 0.36f, 0);
                        boxCollider.size = new Vector3(0.3f, 0.5f, 0.5f);
                        PlayerState playerInfo = character.GetComponent<PlayerState>();
                        playerInfo.DeserializePlayerState(content);
                        playerInfo.SerializePlayerState();
                        Debug.Log("ͬ��λ��");
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
                    // ���߳��е���Unity API
                    // ����ֵ����Ѿ������remoteIP����������
                    dict[remoteIP] = content;
                    string remoteip = remoteIP.ToString();
                    Debug.Log(playerDict.Count);
                    GameObject character = playerDict[remoteip];
                    PlayerState playerInfo = character.GetComponent<PlayerState>();
                    playerInfo.DeserializePlayerState(content);
                    Debug.Log("ͬ��λ��");


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

    #region UDP����
    private Socket socketUDP;
    private IPEndPoint iPEndPoint;
    private EndPoint serverEndPoint;
    private byte[] buffer = new byte[1024];
    private Thread receiveThread;
    private readonly Queue<Action> actions = new Queue<Action>();
    private readonly object queueLock = new object();

    // ���Dictionary���洢�ͻ��˵�EndPoint����Ϣ
    private Dictionary<EndPoint, string> dict = new Dictionary<EndPoint, string>();

    /// <summary>
    /// UDP�����ʼ��
    /// </summary>
    private void InitUDP()
    {
        // ����UDP������ ��Ϣ����Ϊ���ݱ�(Dgram) Э��ΪUDP
        socketUDP = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        iPEndPoint = new IPEndPoint(IPAddress.Parse(serveIp), port);
        serverEndPoint = iPEndPoint;
       
        try
        {
            // ��UDP�׽�������Ĭ�ϵķ����ip��ַ�Ͷ˿ں�
            // �����Խ���һ�β���ͨ��
            SendUDPTo("fasong");
            
        }
        catch
        {
            Debug.Log("UDP����ʧ��!");
            return;
        }
        // ����һ���߳̽�����Ϣ
        receiveThread = new Thread(ReceiveMessage);
        receiveThread.Start();
    }
   

    public void SendUDPTo(string message)
    {
        try
        {
            byte[] buffer = Message.PackToDataUDP(message);
            socketUDP.SendTo(buffer, serverEndPoint); // ʹ�� SendTo ������ Send
            Debug.Log("������Ϣ");
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
                Debug.Log("������Ϣ");
                string receivedMessage = Encoding.UTF8.GetString(buffer, 0, len);

                // ���� HandleResponse ����������յ�����Ϣ
                HandleResponse(receivedMessage);
                
            }
            catch (SocketException e)
            {
                Debug.LogException(e); // ��׽����¼�쳣
            }
            catch (ThreadAbortException e)
            {
                Debug.Log("Thread aborted: " + e.Message);
                break; // �̱߳���ֹ���˳�ѭ��
            }
        }
    }


    /// <summary>
    /// ʹ��UDPЭ�鷢����Ϣ�������
    /// </summary>
    /// <param name="message"></param>
    
    #endregion
}
