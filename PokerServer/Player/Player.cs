using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable disable

public class Player
{
    /// <summary>
    /// id
    /// </summary>
    public string id = "";
    /// <summary>
    /// 相应的客户端
    /// </summary>
    public ClientState state;
    /// <summary>
    /// 玩家数据
    /// </summary>
    public PlayerData data;
    /// <summary>
    /// 是否是房主
    /// </summary>
    public bool isHost=false;
    /// <summary>
    /// 房间号
    /// </summary>
    public int roomId = -1;
    /// <summary>
    /// 是否准备
    /// </summary>
    public bool isPrepare=false;
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="state">相应的客户端</param>
    public Player(ClientState state)
    {
        this.state = state;
    }
    /// <summary>
    /// 封装的发送消息
    /// </summary>
    /// <param name="msgBase">消息</param>
    public void Send(MsgBase msgBase)
    {
        NetManager.Send(state, msgBase);
    }
}

