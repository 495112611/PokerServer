using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class EventHandler
{
    /// <summary>
    /// 掉线处理
    /// </summary>
    /// <param name="c"></param>
    public static void OnDisconnect(ClientState c)
    {
        if (c.player != null)
        {
            int roomId = c.player.roomId;
            if (roomId >= 0)
            {
                Room room=RoomManager.GetRoom(roomId);
                room.RemovePlayer(c.player.id);
            }

            DbManager.UpdatePlayerData(c.player.id, c.player.data);
            PlayerManager.RemovePlayer(c.player.id);
        }
    }
    /// <summary>
    /// 超时处理
    /// </summary>
    public static void OnTimer()
    {
        CheckPing();
        foreach (Room room in RoomManager.rooms.Values.ToArray())
            room.UpdateTurnTimer();
    }
    /// <summary>
    /// 检测Ping是否超时
    /// </summary>
    public static void CheckPing()
    {
        foreach (ClientState s in NetManager.clients.Values)
        {
            if (NetManager.GetTimeStamp() - s.lastPingTime > NetManager.pingInterval * 4)
            {
                Console.WriteLine("心跳机制,断开连接");
                NetManager.Close(s);
                return;
            }
        }
    }
}

