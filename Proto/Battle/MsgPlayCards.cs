using System.Collections;
using System.Collections.Generic;

public class MsgPlayCards : MsgBase
{
    public MsgPlayCards()
    {
        protoName = "MsgPlayCards";
    }
    public string id = "";
    public long turnId; // 必须与服务器当前回合编号一致
    public bool play;
    public CardInfo[] cards = new CardInfo[20];
    public int cardType;
    public bool result;
    public bool canNotPlay = true;
    /// <summary>
    /// 0继续游戏 1农民胜利 2地主胜利
    /// </summary>
    public int win;
}
