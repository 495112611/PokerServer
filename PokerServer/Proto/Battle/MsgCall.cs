using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class MsgCall : MsgBase
{
    public MsgCall()
    {
        protoName = "MsgCall";
    }
    public string id = "";
    public bool call;
    /// <summary>
    /// 0表示继续叫地主 1表示抢地主 2表示重新洗牌 3不需要抢地主
    /// </summary>
    public int result;
}

