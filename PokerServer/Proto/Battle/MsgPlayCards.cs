using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class MsgPlayCards : MsgBase
{
    public MsgPlayCards()
    {
        protoName = "MsgPlayCards";
    }
    public string id = "";
    public bool play;
    public CardInfo[] cards = new CardInfo[20];
    public int cardType;
    public bool canNotPlay = true;
    /// <summary>
    /// 0继续游戏 1农民胜利 2地主胜利
    /// </summary>
    public int win;
    //是否处理完成
    public bool result;
}

