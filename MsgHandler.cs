using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable disable

public class MsgHandler
{
    #region Heartbeat
    public static void MsgPing(ClientState c, MsgBase msgBase)
    {
        Console.WriteLine("MsgPing");
        c.lastPingTime = NetManager.GetTimeStamp();
        MsgPong msgPong = new MsgPong();
        NetManager.Send(c, msgPong);
    }
    #endregion
    #region Login
    /// <summary>
    /// 注册
    /// </summary>
    /// <param name="c"></param>
    /// <param name="msgBase"></param>
    public static void MsgRegister(ClientState c, MsgBase msgBase)
    {
        MsgRegister msg = msgBase as MsgRegister;
        if (DbManager.Register(msg.id, msg.pw))
        {
            DbManager.CreadtePlayer(msg.id);
            msg.result = true;
        }
        else
        {
            msg.result = false;
        }
        NetManager.Send(c, msg);
    }
    /// <summary>
    /// 登录
    /// </summary>
    /// <param name="c"></param>
    /// <param name="msgBase"></param>
    public static void MsgLogin(ClientState c, MsgBase msgBase)
    {
        MsgLogin msg = msgBase as MsgLogin;
        //检验密码
        if (!DbManager.CheckPassword(msg.id, msg.pw))
        {
            msg.result = false;
            NetManager.Send(c, msg);
            return;
        }
        //检验是否登录
        if (c.player != null)
        {
            msg.result = false;
            NetManager.Send(c, msg);
            return;
        }
        //踢下线
        if (PlayerManager.IsOnline(msg.id))
        {
            Player otherPlayer = PlayerManager.GetPlayer(msg.id);
            MsgKick msgKick = new MsgKick();
            msgKick.isKick = true;
            otherPlayer.Send(msgKick);
            NetManager.Close(otherPlayer.state);
        }

        PlayerData playerData = DbManager.GetPlayerData(msg.id);
        if (playerData == null)
        {
            msg.result = false;
            NetManager.Send(c, msg);
            return;
        }
        //创建玩家
        Player player = new Player(c);
        player.id = msg.id;
        player.data = playerData;
        PlayerManager.AddPlayer(msg.id, player);
        c.player = player;
        msg.result = true;
        player.Send(msg);
    }
    #endregion
    #region Room
    /// <summary>
    /// 获取玩家数据信息
    /// </summary>
    /// <param name="c"></param>
    /// <param name="msgBase"></param>
    public static void MsgGetAchieve(ClientState c, MsgBase msgBase)
    {
        MsgGetAchieve msg = msgBase as MsgGetAchieve;
        Player player = c.player;
        if (player == null)
            return;
        msg.bean = player.data.bean;
        player.Send(msg);
    }
    /// <summary>
    /// 获取房间列表
    /// </summary>
    /// <param name="c"></param>
    /// <param name="msgBase"></param>
    public static void MsgGetRoomList(ClientState c, MsgBase msgBase)
    {
        MsgGetRoomList msg = msgBase as MsgGetRoomList;
        Player player = c.player;
        if (player == null)
            return;
        player.Send(RoomManager.ToMsg());
    }
    /// <summary>
    /// 创建房间
    /// </summary>
    /// <param name="c"></param>
    /// <param name="msgBase"></param>
    public static void MsgCreateRoom(ClientState c, MsgBase msgBase)
    {
        MsgCreateRoom msg = msgBase as MsgCreateRoom;
        Player player = c.player;
        if (player == null)
            return;
        if (player.roomId >= 0)
        {
            msg.result = false;
            player.Send(msg);
            return;
        }


        //创建房间
        Room room = RoomManager.AddRoom();
        room.AddPlayer(player.id);

        msg.result = true;
        player.Send(msg);
    }
    /// <summary>
    /// 进入房间
    /// </summary>
    /// <param name="c"></param>
    /// <param name="msgBase"></param>
    public static void MsgEnterRoom(ClientState c, MsgBase msgBase)
    {
        MsgEnterRoom msg = msgBase as MsgEnterRoom;
        Player player = c.player;
        if (player == null)
            return;

        if (player.roomId >= 0)
        {
            msg.result = false;
            player.Send(msg);
            return;
        }

        //进入房间
        Room room = RoomManager.GetRoom(msg.id);
        if (room == null)
        {
            msg.result = false;
            player.Send(msg);
            return;
        }
        if (!room.AddPlayer(player.id))
        {
            msg.result = false;
            player.Send(msg);
            return;
        }
        msg.result = true;
        player.Send(msg);
    }
    /// <summary>
    /// 获取房间信息
    /// </summary>
    /// <param name="c"></param>
    /// <param name="msgBase"></param>
    public static void MsgGetRoomInfo(ClientState c, MsgBase msgBase)
    {
        MsgGetRoomInfo msg = msgBase as MsgGetRoomInfo;
        Player player = c.player;
        if (player == null)
            return;

        Room room = RoomManager.GetRoom(player.roomId);
        if (room == null)
        {
            player.Send(msg);
            return;
        }

        player.Send(room.ToMsg());
    }
    /// <summary>
    /// 离开房间
    /// </summary>
    /// <param name="c"></param>
    /// <param name="msgBase"></param>
    public static void MsgLeaveRoom(ClientState c, MsgBase msgBase)
    {
        MsgLeaveRoom msg = msgBase as MsgLeaveRoom;
        Player player = c.player;
        if (player == null)
            return;

        Room room = RoomManager.GetRoom(player.roomId);
        if (room == null)
        {
            msg.result = false;
            player.Send(msg);
            return;
        }

        room.RemovePlayer(player.id);
        msg.result = true;
        player.Send(msg);
    }
    /// <summary>
    /// 准备
    /// </summary>
    /// <param name="c"></param>
    /// <param name="msgBase"></param>
    public static void MsgPrepare(ClientState c, MsgBase msgBase)
    {
        MsgPrepare msg = msgBase as MsgPrepare;
        Player player = c.player;
        if (player == null)
            return;

        Room room = RoomManager.GetRoom(player.roomId);
        if (room == null)
        {
            msg.isPrepare = false;
            player.Send(msg);
            return;
        }

        msg.isPrepare = room.Prepare(player.id);
        player.Send(msg);
    }
    /// <summary>
    /// 开始游戏
    /// </summary>
    /// <param name="c"></param>
    /// <param name="msgBase"></param>
    public static void MsgStartBattle(ClientState c, MsgBase msgBase)
    {
        MsgStartBattle msg = msgBase as MsgStartBattle;
        Player player = c.player;
        if (player == null)
            return;

        Room room = RoomManager.GetRoom(player.roomId);
        if (room == null)
        {
            msg.result = 3;
            player.Send(msg);
            return;
        }

        if (room.playerList.Count < 3)
        {
            msg.result = 1;
            player.Send(msg);
            return;
        }

        msg.result = 0;
        foreach (string id in room.playerList)
        {
            if (id == room.hostId)
                continue;
            //有未准备的玩家
            if (!room.playerDict.ContainsKey(id) || !room.playerDict[id])
            {
                msg.result = 2;
            }
        }
        if (msg.result == 2)
        {
            player.Send(msg);
            return;
        }
        //成功
        else
        {
            room.Start();
            foreach (string id in room.playerList)
            {
                Player p = PlayerManager.GetPlayer(id);
                p.Send(msg);
            }
        }

    }
    #endregion

    #region Battle
    /// <summary>
    /// 获取卡牌
    /// </summary>
    /// <param name="c"></param>
    /// <param name="msgBase"></param>
    public static void MsgGetCardList(ClientState c, MsgBase msgBase)
    {
        MsgGetCardList msg = msgBase as MsgGetCardList;
        Player player = c.player;
        if (player == null)
            return;
        Room room = RoomManager.GetRoom(player.roomId);
        if (room == null)
            return;

        Card[] cards = room.playerCard[player.id].ToArray();
        msg.cardInfos = CardManager.GetCardInfos(cards);
        Card[] threeCards = room.playerCard[""].ToArray();
        msg.threeCards = CardManager.GetCardInfos(threeCards);
        player.Send(msg);
    }
    /// <summary>
    /// 获取开始玩家
    /// </summary>
    /// <param name="c"></param>
    /// <param name="msgBase"></param>
    public static void MsgGetStartPlayer(ClientState c, MsgBase msgBase)
    {
        MsgGetStartPlayer msg = msgBase as MsgGetStartPlayer;
        Player player = c.player;
        if (player == null)
            return;
        Room room = RoomManager.GetRoom(player.roomId);
        if (room == null)
            return;

        msg.id = room.currentPlayer;
        foreach (string id in room.playerList)
        {
            PlayerManager.GetPlayer(id).Send(msg);
        }
    }
    /// <summary>
    /// 轮换下一个玩家
    /// </summary>
    /// <param name="c"></param>
    /// <param name="msgBase"></param>
    public static void MsgSwitchTurn(ClientState c, MsgBase msgBase)
    {
        MsgSwitchTurn msg = msgBase as MsgSwitchTurn;
        Player player = c.player;
        if (player == null)
            return;
        Room room = RoomManager.GetRoom(player.roomId);
        if (room == null)
            return;

        Console.WriteLine(room.Index);
        room.Index += msg.round;
        Console.WriteLine(room.Index);

        room.currentPlayer = room.playerList[room.Index];

        msg.id = room.currentPlayer;
        foreach (string id in room.playerList)
        {
            PlayerManager.GetPlayer(id).Send(msg);
        }
    }
    /// <summary>
    /// 获取上一家和下一家
    /// </summary>
    /// <param name="c"></param>
    /// <param name="msgBase"></param>
    public static void MsgGetPlayer(ClientState c, MsgBase msgBase)
    {
        MsgGetPlayer msg = msgBase as MsgGetPlayer;
        Player player = c.player;
        if (player == null)
            return;
        Room room = RoomManager.GetRoom(player.roomId);
        if (room == null)
            return;

        msg.id = player.id;
        for (int i = 0; i < room.playerList.Count; i++)
        {
            if (room.playerList[i] == msg.id)
            {
                msg.leftId = room.playerList[i - 1 < 0 ? 2 : i - 1];
                msg.rightId = room.playerList[i + 1 > 2 ? 0 : i + 1];
            }
        }
        player.Send(msg);
    }
    /// <summary>
    /// 处理叫地主逻辑
    /// </summary>
    /// <param name="c"></param>
    /// <param name="msgBase"></param>
    public static void MsgCall(ClientState c, MsgBase msgBase)
    {
        MsgCall msg = msgBase as MsgCall;
        Player player = c.player;
        if (player == null)
            return;
        msg.id = player.id;
        Room room = RoomManager.GetRoom(player.roomId);
        if (room == null)
            return;

        if (msg.call)
        {
            room.callID = player.id;
            room.landLordRank[player.id] += 2;
            if (room.CheckCall())
            {
                msg.result = 3;
                room.landLord = player.id;
                foreach (Card card in room.playerCard[""])
                {
                    room.playerCard[room.landLord].Add(card);
                }
            }
            else
            {
                msg.result = 1;
            }
            room.Send(msg);
            return;
        }
        else
        {
            room.landLordRank[player.id] += 1;
            if (room.CheckAllNotCall())
            {
                msg.result = 2;
            }
            else
            {
                msg.result = 0;
            }
            room.Send(msg);
            return;
        }
    }
    /// <summary>
    /// 都没有叫地主，重新开始
    /// </summary>
    /// <param name="c"></param>
    /// <param name="msgBase"></param>
    public static void MsgReStart(ClientState c, MsgBase msgBase)
    {
        MsgReStart msg = msgBase as MsgReStart;
        Player player = c.player;
        if (player == null)
            return;
        Room room = RoomManager.GetRoom(player.roomId);
        if (room == null)
            return;

        CardManager.Shuffle();
        room.cards = CardManager.cards;
        room.Start();
        room.Send(msg);
    }
    /// <summary>
    /// 开始抢地主
    /// </summary>
    /// <param name="c"></param>
    /// <param name="msgBase"></param>
    public static void MsgStartRob(ClientState c, MsgBase msgBase)
    {
        MsgStartRob msg = msgBase as MsgStartRob;
        Player player = c.player;
        if (player == null)
            return;
        Room room = RoomManager.GetRoom(player.roomId);
        if (room == null)
            return;

        room.Send(msg);
    }
    /// <summary>
    /// 抢地主
    /// </summary>
    /// <param name="c"></param>
    /// <param name="msgBase"></param>
    public static void MsgRob(ClientState c, MsgBase msgBase)
    {
        MsgRob msg = msgBase as MsgRob;
        Player player = c.player;
        if (player == null)
            return;
        msg.id = player.id;
        Room room = RoomManager.GetRoom(player.roomId);
        if (room == null)
            return;


        if (msg.rob)
        {
            room.landLordRank[player.id] += room.robRank++;
        }
        else
        {
            room.landLordRank[player.id]++;
            if (room.CheckCall())
            {
                msg.landLord = room.callID;
                room.landLord = msg.landLord;
                foreach (Card card in room.playerCard[""])
                {
                    room.playerCard[room.landLord].Add(card);
                }
            }
        }
        if (player.id == room.callID)
        {
            //检测谁是地主
            msg.landLord = room.CheckLandLord();
            room.landLord = msg.landLord;
            foreach (Card card in room.playerCard[""])
            {
                room.playerCard[room.landLord].Add(card);
            }
        }
        if (room.landLordRank[room.playerList[room.Index + 1 >= 3 ? 0 : room.Index + 1]] == 0)
        {
            msg.needRob = false;
        }
        else
        {
            msg.needRob = true;
        }
        room.Send(msg);

    }
    public static void MsgPlayCards(ClientState c, MsgBase msgBase)
    {
        MsgPlayCards msg = msgBase as MsgPlayCards;
        Player player = c.player;
        if (player == null)
            return;
        msg.id = player.id;
        Room room = RoomManager.GetRoom(player.roomId);
        if (room == null)
            return;

        Card[] cards = CardManager.GetCards(msg.cards);
        if (msg.play)
        {
            msg.cardType = (int)CardManager.GetCardType(cards);
            //前两家出牌了，和他们出的牌比较
            if (room.prePrePlay || room.prePlay)
            {
                msg.result = CardManager.Compare(room.preCard.ToArray(), cards);
            }
            //前两家要不起，自己开始出
            else
            {
                msg.result = CardManager.GetCardType(cards) != CardManager.CardType.wrong;
            }
            //出牌成功
            if (msg.result)
            {
                //删除卡牌
                room.DeleteCards(cards, msg.id);
                //判断输赢
                msg.win = room.CheckWin();
                room.preCard = cards.ToList();
                room.prePrePlay = room.prePlay;
                room.prePlay = true;
            }
            room.Send(msg);
            return;
        }
        //玩家点击不出
        else
        {
            room.prePrePlay = room.prePlay;
            room.prePlay = false;
            if (!room.prePrePlay)
                msg.canNotPlay = false;
            msg.result = true;
            room.Send(msg);
            return;
        }
    }
    #endregion
}