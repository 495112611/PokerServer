using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class MsgRegister : MsgBase
{
    public MsgRegister()
    {
        protoName = "MsgRegister";
    }
    public string id = "";
    public string pw = "";
    public bool result=true;
}

