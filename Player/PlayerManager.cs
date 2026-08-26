using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable disable

public class PlayerManager
{
    /// <summary>
    /// 在线玩家字典
    /// </summary>
    private static Dictionary<string, Player> players = new Dictionary<string, Player>();
    /// <summary>
    /// 玩家是否在线
    /// </summary>
    /// <param name="id">玩家id</param>
    /// <returns></returns>
    public static bool IsOnline(string id)
    {
        return players.ContainsKey(id);
    }
    /// <summary>
    /// 根据id获取对应的玩家
    /// </summary>
    /// <param name="id">玩家id</param>
    /// <returns></returns>
    public static Player GetPlayer(string id)
    {
        if (players.ContainsKey(id))
        {
            return players[id];
        }
        return null;
    }
    /// <summary>
    /// 增添玩家
    /// </summary>
    /// <param name="id">玩家id</param>
    /// <param name="player">玩家</param>
    public static void AddPlayer(string id,Player player)
    {
        players.Add(id, player);
    }
    /// <summary>
    /// 删除玩家
    /// </summary>
    /// <param name="id"></param>
    public static void RemovePlayer(string id)
    {
        players.Remove(id);
    }
}

