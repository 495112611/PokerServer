using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class MsgGetPlayer : MsgBase
{
    public MsgGetPlayer()
    {
        protoName = "MsgGetPlayer";
    }
    public string id = "";
    public string leftId = "";
    public string rightId = "";
}

