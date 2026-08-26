using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class MsgSwitchTurn : MsgBase
{
    public MsgSwitchTurn()
    {
        protoName = "MsgSwitchTurn";
    }
    public string id = "";
    public int round = 1;
}
