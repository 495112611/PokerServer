using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable disable

public class Room
{
    /// <summary>
    /// 房间号
    /// </summary>
    public int id = 0;
    /// <summary>
    /// 最大玩家数
    /// </summary>
    public int maxPlayer = 3;
    /// <summary>
    /// 玩家
    /// </summary>
    public List<string> playerList = new List<string>();
    /// <summary>
    /// 玩家准备状态(不包括房主)
    /// </summary>
    public Dictionary<string, bool> playerDict = new Dictionary<string, bool>();
    /// <summary>
    /// 房主
    /// </summary>
    public string hostId = "";
    /// <summary>
    /// 状态，表示是否开始
    /// </summary>
    public enum Status
    {
        Prepare,
        Start,
    }
    /// <summary>
    /// 状态
    /// </summary>
    public Status status = Status.Prepare;
    /// <summary>
    /// 当前房间的一套牌
    /// </summary>
    public List<Card> cards;
    /// <summary>
    /// 玩家和对应手牌
    /// </summary>
    public Dictionary<string, List<Card>> playerCard = new Dictionary<string, List<Card>>();
    /// <summary>
    /// 当前玩家id
    /// </summary>
    public string currentPlayer;
    /// <summary>
    /// 当前玩家的索引
    /// </summary>
    private int index = 0;
    /// <summary>
    /// 玩家叫地主抢地主的权值
    /// </summary>
    public Dictionary<string, int> landLordRank = new Dictionary<string, int>();
    /// <summary>
    /// 上一家或者上上家出的牌
    /// </summary>
    public List<Card> preCard = new List<Card>();

    /// <summary>
    /// 上上家出没出牌
    /// </summary>
    public bool prePrePlay;
    /// <summary>
    /// 上一家出没出牌
    /// </summary>
    public bool prePlay;

    /// <summary>
    /// 叫地主的玩家
    /// </summary>
    public string callID = "";
    public int robRank = 3;
    /// <summary>
    /// 地主id
    /// </summary>
    public string landLord = "";
    /// <summary>
    /// 地主已确定，等待地主首次成功出牌。
    /// </summary>
    public bool firstPlay = false;

    public int Index
    {
        get => index;
        set
        {
            index = value;
            if (index < 0)
                index += 3;
            else if (index > 2)
                index -= 3;
        }
    }

    public Room()
    {
        if (cards == null)
        {
            CardManager.Shuffle();
            cards = CardManager.cards;
        }
    }

    /// <summary>
    /// 添加玩家
    /// </summary>
    /// <param name="id">玩家id</param>
    /// <returns>是否执行成功</returns>
    public bool AddPlayer(string id)
    {
        Player player = PlayerManager.GetPlayer(id);
        if (player == null)
        {
            Console.WriteLine("Room.AddPlayer错误,玩家为空");
            return false;
        }
        if (playerList.Count >= maxPlayer)
        {
            Console.WriteLine("Room.AddPlayer错误,达到最大玩家数");
            return false;
        }
        if (status == Status.Start)
        {
            Console.WriteLine("Room.AddPlayer错误,已经开始游戏");
            return false;
        }
        if (playerList.Contains(id))
        {
            Console.WriteLine("Room.AddPlayer错误,玩家已在房间里");
            return false;
        }
        playerList.Add(id);
        player.roomId = this.id;
        if (hostId == "")
        {
            hostId = player.id;
            player.isHost = true;
        }

        Broadcast(ToMsg());
        return true;
    }
    /// <summary>
    /// 删除玩家
    /// </summary>
    /// <param name="id">玩家id</param>
    /// <returns>是否删除成功</returns>
    public bool RemovePlayer(string id)
    {
        Player player = PlayerManager.GetPlayer(id);
        if (player == null)
        {
            Console.WriteLine("Room.RemovePlayer错误,玩家为空");
            return false;
        }
        if (!playerList.Contains(id))
        {
            Console.WriteLine("Room.RemovePlayer错误,玩家不在当前房间");
            return false;
        }
        playerList.Remove(id);

        //判断是否以准备
        if (playerDict.ContainsKey(id))
        {
            playerDict.Remove(id);
            player.isPrepare = false;
        }

        player.roomId = -1;
        //如果他是房主，重新选择房主
        if (player.isHost)
        {
            player.isHost = false;
            //重新选择房主
            foreach (string playerId in playerList)
            {
                hostId = playerId;
                PlayerManager.GetPlayer(playerId).isHost = true;
                break;
            }
        }
        if (playerList.Count == 0)
        {
            hostId = "";
            //移除该房间
            RoomManager.RemoveRoom(this.id);
        }

        Broadcast(ToMsg());
        return true;
    }
    /// <summary>
    /// 广播
    /// </summary>
    /// <param name="msgBase"></param>
    public void Broadcast(MsgBase msgBase)
    {
        foreach (string id in playerList)
        {
            Player player = PlayerManager.GetPlayer(id);
            player.Send(msgBase);
        }
    }
    /// <summary>
    /// 转成消息
    /// </summary>
    /// <returns></returns>
    public MsgBase ToMsg()
    {
        MsgGetRoomInfo msg = new MsgGetRoomInfo();
        int count = playerList.Count;
        msg.players = new PlayerInfo[count];


        int i = 0;
        foreach (string id in playerList)
        {
            Player player = PlayerManager.GetPlayer(id);
            PlayerInfo playerInfo = new PlayerInfo();

            playerInfo.id = player.id;
            playerInfo.bean = player.data.bean;
            playerInfo.isPrepare = player.isPrepare;
            playerInfo.isHost = player.isHost;

            msg.players[i] = playerInfo;
            i++;
        }
        return msg;
    }
    /// <summary>
    /// 准备
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public bool Prepare(string id)
    {
        Player player = PlayerManager.GetPlayer(id);
        if (player == null)
        {
            Console.WriteLine("Room.RemovePlayer错误,玩家为空");
            return false;
        }
        if (!playerList.Contains(id))
        {
            Console.WriteLine("Room.RemovePlayer错误,玩家不在当前房间");
            return false;
        }
        if (!playerDict.ContainsKey(id))
        {
            playerDict.Add(id, true);
        }
        else
        {
            playerDict[id] = true;
        }
        player.isPrepare = true;

        Broadcast(ToMsg());

        return true;
    }
    /// <summary>
    /// 在成功开始游戏的时候调用
    /// </summary>
    public void Start()
    {
        Random random = new Random();
        Index = random.Next(3);
        currentPlayer = playerList[Index];

        robRank = 3;
        callID = "";
        landLord = "";
        firstPlay = false;
        preCard.Clear();
        prePlay = false;
        prePrePlay = false;

        playerCard.Clear();
        landLordRank.Clear();

        //分配玩家手牌
        for (int i = 0; i < 3; i++)
        {
            List<Card> c = new List<Card>();
            for (int j = i * 17; j < i * 17 + 17; j++)//0 17     17 34    34 51
            {
                c.Add(cards[j]);
            }
            playerCard.Add(playerList[i], c);
        }
        List<Card> ca = new List<Card>();
        for (int i = 51; i < 54; i++)
        {
            ca.Add(cards[i]);
        }
        //空字符串表示底牌
        playerCard.Add("", ca);

        for (int i = 0; i < playerList.Count; i++)
        {
            landLordRank.Add(playerList[i], -1);
        }
    }
    /// <summary>
    /// 给当前房间所以玩家发送消息
    /// </summary>
    /// <param name="msgBase"></param>
    public void Send(MsgBase msgBase)
    {
        foreach (string id in playerList)
        {
            PlayerManager.GetPlayer(id).Send(msgBase);
        }
    }
    /// <summary>
    /// 判断玩家是不是不需要抢地主了
    /// </summary>
    /// <returns>如果不需要，返回true</returns>
    public bool CheckCall()
    {
        int count = 0;
        foreach (int i in landLordRank.Values)
        {
            if (i == 0)
            {
                count++;
            }
        }
        if (count == 2)
            return true;
        return false;
    }
    /// <summary>
    /// 检测所有玩家是不是都没有叫
    /// </summary>
    /// <returns>如果都没有叫，返回true</returns>
    public bool CheckAllNotCall()
    {
        bool result = true;
        foreach (int i in landLordRank.Values)
        {
            if (i != 0)
            {
                result = false;
            }
        }
        return result;
    }
    /// <summary>
    /// 检测地主
    /// </summary>
    /// <returns></returns>
    public string CheckLandLord()
    {

        (string s, int i) result = ("", -1);
        foreach (string id in landLordRank.Keys)
        {
            if (landLordRank[id] > result.i)
            {
                result = (id, landLordRank[id]);
            }
        }

        return result.s;
    }
    public void SetLandLord(string id)
    {
        landLord = id;
        Index = playerList.IndexOf(id);
        currentPlayer = id;
        firstPlay = true;
        playerCard[id].AddRange(playerCard[""]);
    }

    public void DeleteCards(Card[] cards, string id)
    {
        for (int i = 0; i < cards.Length; i++)
        {
            for (int j = playerCard[id].Count - 1; j >= 0; j--)
            {
                if (playerCard[id][j].Equals(cards[i]))
                {
                    playerCard[id].RemoveAt(j);
                }
            }
        }
    }
    /// <summary>
    /// 检测结束
    /// </summary>
    /// <returns>0没有结束 1农民胜利 2地主胜利</returns>
    public int CheckWin()
    {
        foreach (string id in playerCard.Keys)
        {
            if (id == "")
                continue;
            Console.WriteLine(landLord);
            if (playerCard[id].Count == 0)
            {
                if (id == landLord)
                {
                    return 2;
                }
                return 1;
            }
        }
        return 0;
    }
}

