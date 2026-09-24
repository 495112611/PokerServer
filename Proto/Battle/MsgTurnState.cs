// 服务器广播的回合快照；客户端只显示，不自行决定超时或轮换。
public class MsgTurnState : MsgBase
{
    public MsgTurnState() { protoName = "MsgTurnState"; }
    public string id = "";
    public long turnId;
    public int phase; // 0叫地主，1抢地主，2出牌，3结束/停止
    public int secondsRemaining;
    public int duration;
    public bool active;
    public bool canNotPlay;
}
