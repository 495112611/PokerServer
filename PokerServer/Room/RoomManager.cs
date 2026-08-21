using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable disable

public class RoomManager
{
    /// <summary>
    /// 最大房间号
    /// </summary>
    private static int maxId = 1;
    /// <summary>
    /// 房间列表
    /// </summary>
    public static Dictionary<int, Room> rooms = new Dictionary<int, Room>();
    /// <summary>
    /// 获取房间
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static Room GetRoom(int id)
    {
        if (rooms.ContainsKey(id))
            return rooms[id];
        return null;
    }
    /// <summary>
    /// 添加房间
    /// </summary>
    /// <returns>添加的房间</returns>
    public static Room AddRoom()
    {
        maxId++;
        Room room = new Room();
        room.id = maxId;
        rooms.Add(room.id, room);
        return room;
    }
    /// <summary>
    /// 删除房间
    /// </summary>
    /// <param name="id">房间id</param>
    public static void RemoveRoom(int id)
    {
        rooms.Remove(id);
    }
    /// <summary>
    /// 转成消息
    /// </summary>
    /// <returns></returns>
    public static MsgBase ToMsg()
    {
        MsgGetRoomList msg=new MsgGetRoomList();
        int count=rooms.Count;
        msg.rooms = new RoomInfo[count];
        int i = 0;
        foreach (Room room in rooms.Values)
        {
            RoomInfo roomInfo = new RoomInfo();
            roomInfo.id = room.id;
            roomInfo.count=room.playerList.Count;
            if (room.status == Room.Status.Prepare)
            {
                roomInfo.isPrepare = true;
            }
            else
            {
                roomInfo.isPrepare = false;
            }
            msg.rooms[i] = roomInfo;
            i++;
        }
        return msg;
    }
}

