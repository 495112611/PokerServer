using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
#nullable disable

public static class NetManager
{
    /// <summary>
    /// 服务端Socket
    /// </summary>
    public static Socket listenfd;
    /// <summary>
    /// 客户端字典
    /// </summary>
    public static Dictionary<Socket, ClientState> clients = new Dictionary<Socket, ClientState>();
    /// <summary>
    /// 用于检测的List
    /// </summary>
    private static List<Socket> sockets = new List<Socket>();
    /// <summary>
    /// 时间间隔
    /// </summary>
    public static long pingInterval = 30;


    /// <summary>
    /// 连接
    /// </summary>
    /// <param name="ip">IP地址</param>
    /// <param name="port">端口号</param>
    public static void Connect(string ip, int port)
    {
        listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPAddress iPAddress = IPAddress.Parse(ip);
        IPEndPoint iPEndPoint = new IPEndPoint(iPAddress, port);
        listenfd.Bind(iPEndPoint);
        listenfd.Listen(0);

        Console.WriteLine("[服务器]启动成功");
        while (true)
        {
            //填充List
            sockets.Clear();
            sockets.Add(listenfd);
            foreach (ClientState s in clients.Values)
            {
                sockets.Add(s.socket);
            }

            Socket.Select(sockets, null, null, 1000);
            for (int i = 0; i < sockets.Count; i++)
            {
                Socket s = sockets[i];
                if (s == listenfd)
                {
                    //有客户端连接需要Accept
                    Accept(s);
                }
                else
                {
                    //有客户端发过来消息需要Receive
                    Receive(s);
                }
            }
            Timer();
        }
    }
    /// <summary>
    /// 接收客户端Socket
    /// </summary>
    /// <param name="listenfd">服务端Socket</param>
    public static void Accept(Socket listenfd)
    {
        try
        {
            Socket clientfd = listenfd.Accept();
            Console.WriteLine("Accept " + clientfd.RemoteEndPoint.ToString());
            ClientState state = new ClientState();
            state.lastPingTime = GetTimeStamp();
            state.socket = clientfd;
            clients.Add(clientfd, state);
        }
        catch (SocketException ex)
        {
            Console.WriteLine("Accept fail " + ex.ToString());
        }
    }
    /// <summary>
    /// 接收消息
    /// </summary>
    /// <param name="clientfd">发信息的客户端</param>
    public static void Receive(Socket clientfd)
    {
        ClientState state = clients[clientfd];
        ByteArray readBuff = state.readBuff;


        int count = 0;
        if (readBuff.Remain <= 0)
        {
            readBuff.MoveBytes();
        }
        if (readBuff.Remain <= 0)
        {
            Console.WriteLine("Receive fail " + "数组长度不足");
            Close(state);
            return;
        }
        try
        {
            count = clientfd.Receive(readBuff.bytes, readBuff.writeIndex, readBuff.Remain, 0);
        }
        catch (SocketException ex)
        {
            Console.WriteLine("Receive fail " + ex.ToString());
            Close(state);
            return;
        }
        //关闭
        if (count <= 0)
        {
            Console.WriteLine("Socket Close " + clientfd.RemoteEndPoint.ToString());
            Close(state);
            return;
        }
        readBuff.writeIndex += count;
        //解码
        OnReceiveData(state);
        readBuff.MoveBytes();
    }
    /// <summary>
    /// 关闭
    /// </summary>
    /// <param name="state"></param>
    public static void Close(ClientState state)
    {
        //调用OnDisconnect
        MethodInfo mei = typeof(EventHandler).GetMethod("OnDisconnect");
        object[] ob = { state };
        mei.Invoke(null, ob);
        //关闭
        state.socket.Close();
        clients.Remove(state.socket);
    }
    /// <summary>
    /// 处理消息
    /// </summary>
    /// <param name="state"></param>
    public static void OnReceiveData(ClientState state)
    {
        ByteArray readBuff = state.readBuff;
        byte[] bytes = readBuff.bytes;
        if (readBuff.Length <= 2)
            return;
        //解析数字
        short bodyLength = (short)(bytes[readBuff.readIndex + 1] * 256 + bytes[readBuff.readIndex]);
        if (readBuff.Length < bodyLength)
            return;
        readBuff.readIndex += 2;
        //解析协议名
        int nameCount = 0;
        string protoName = MsgBase.DecodeName(readBuff.bytes, readBuff.readIndex, out nameCount);
        if (protoName == "")
        {
            Console.WriteLine("OnReceiveData fail", "解析协议名失败");
            Close(state);
            return;
        }
        readBuff.readIndex += nameCount;
        //解析协议体
        int bodyCount = bodyLength - nameCount;
        MsgBase msgBase = MsgBase.Decode(protoName, readBuff.bytes, readBuff.readIndex, bodyCount);
        readBuff.readIndex += bodyCount;
        readBuff.MoveBytes();
        //分发消息
        MethodInfo mi = typeof(MsgHandler).GetMethod(protoName);
        Console.WriteLine("Receive " + protoName);
        if (mi != null)
        {
            object[] o = { state, msgBase };
            mi.Invoke(null, o);
        }
        else
        {
            Console.WriteLine("OnReceiveData调用Msg函数失败");
        }
        //继续处理
        if (readBuff.Length > 2)
        {
            OnReceiveData(state);
        }
    }
    /// <summary>
    /// 发送数据
    /// </summary>
    /// <param name="cs">发给的客户端</param>
    /// <param name="msgBase">发送的数据</param>
    public static void Send(ClientState cs, MsgBase msgBase)
    {
        if (cs == null || !cs.socket.Connected)
            return;
        //编码
        byte[] nameBytes = MsgBase.EncodeName(msgBase);
        byte[] bodyBytes = MsgBase.Encode(msgBase);
        int len = nameBytes.Length + bodyBytes.Length;
        byte[] sendBytes = new byte[len + 2];
        sendBytes[0] = (byte)(len % 256);
        sendBytes[1] = (byte)(len / 256);
        //拷贝到sendBytes
        Array.Copy(nameBytes, 0, sendBytes, 2, nameBytes.Length);
        Array.Copy(bodyBytes, 0, sendBytes, 2 + nameBytes.Length, bodyBytes.Length);
        try
        {
            cs.socket.Send(sendBytes, 0, sendBytes.Length, 0);
        }
        catch (SocketException ex)
        {
            Console.WriteLine("Send fail " + ex.ToString());
        }

    }
    /// <summary>
    /// 计时器
    /// </summary>
    private static void Timer()
    {
        MethodInfo mei = typeof(EventHandler).GetMethod("OnTimer");
        object[] ob = { };
        mei.Invoke(null, ob);
    }
    /// <summary>
    /// 获取时间戳
    /// </summary>
    public static long GetTimeStamp()
    {
        TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return Convert.ToInt64(ts.TotalSeconds);
    }
}

