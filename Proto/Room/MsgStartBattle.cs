using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class MsgStartBattle : MsgBase
{
    public MsgStartBattle()
    {
        protoName = "MsgStartBattle";
    }
    /// <summary>
    /// 0代表成功 1代表人数不足三人 2代表有玩家未准备 3代表房间为空
    /// </summary>
    public int result;
}

