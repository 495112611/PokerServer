using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
#nullable disable
public class ClientState
{
    /// <summary>
    /// 客户端Socke
    /// </summary>
    public Socket socket;
    /// <summary>
    /// 客户端的字节数组
    /// </summary>
    public ByteArray readBuff = new ByteArray();
    /// <summary>
    /// 最后一次收到Ping的时间
    /// </summary>
    public long lastPingTime = 0;
    /// <summary>
    /// 玩家
    /// </summary>
    public Player player;
}
