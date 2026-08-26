using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class MsgGetCardList : MsgBase
{
    public MsgGetCardList()
    {
        protoName = "MsgGetCardList";
    }
    public CardInfo[] cardInfos = new CardInfo[17];
    public CardInfo[] threeCards = new CardInfo[3];
}

