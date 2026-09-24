using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class MsgRob : MsgBase
{
    public MsgRob()
    {
        protoName = "MsgRob";
    }
    public string id = "";
    public long turnId; // 必须与服务器当前回合编号一致
    public bool rob;
    public bool needRob = true;
    public string landLord = "";
}

