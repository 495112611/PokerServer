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
    public bool rob;
    public bool needRob = true;
    public string landLord = "";
}

