using System;
using System.Linq;

public partial class Room
{
    public enum TurnPhase { Call, Rob, Play, Finished }
    public const int BidSeconds = 15;
    public const int PlaySeconds = 30;

    public TurnPhase Phase { get; private set; } = TurnPhase.Finished;
    public long TurnId { get; private set; }
    public long TurnDeadline { get; private set; }
    public bool TimerActive { get; private set; }
    private int lastTimerSecond = -1;
    // 单调时钟不受服务器系统时间校准影响；测试时可注入可控时钟。
    private readonly Func<long> clock;

    private void BeginTurn(TurnPhase phase, int seat)
    {
        Phase = phase;
        Index = seat;
        currentPlayer = playerList[Index];
        TurnId++;
        TimerActive = true;
        TurnDeadline = clock() + (phase == TurnPhase.Play ? PlaySeconds : BidSeconds) * 1000L;
        lastTimerSecond = -1;
    }

    public MsgTurnState GetTurnState()
    {
        int duration = Phase == TurnPhase.Play ? PlaySeconds : BidSeconds;
        return new MsgTurnState
        {
            id = currentPlayer ?? "",
            turnId = TurnId,
            phase = (int)Phase,
            duration = duration,
            active = TimerActive,
            secondsRemaining = TimerActive ? (int)Math.Max(0, (TurnDeadline - clock() + 999) / 1000) : 0,
            canNotPlay = Phase == TurnPhase.Play && (prePlay || prePrePlay)
        };
    }

    public void BroadcastTurnState()
    {
        MsgTurnState msg = GetTurnState();
        lastTimerSecond = msg.secondsRemaining;
        Send(msg);
    }

    public void StopTurnTimer()
    {
        TimerActive = false;
        Phase = TurnPhase.Finished;
        firstPlay = false;
        TurnId++;
        BroadcastTurnState();
    }

    // 在服务器网络主循环执行，和玩家操作串行，避免超时与点击同时推进两次。
    public void UpdateTurnTimer()
    {
        if (!TimerActive)
            return;
        if (playerList.Count != maxPlayer)
        {
            StopTurnTimer();
            return;
        }
        if (clock() >= TurnDeadline)
        {
            string actor = currentPlayer;
            switch (Phase)
            {
                case TurnPhase.Call:
                    ApplyCall(actor, new MsgCall { turnId = TurnId, call = false });
                    break;
                case TurnPhase.Rob:
                    ApplyRob(actor, new MsgRob { turnId = TurnId, rob = false });
                    break;
                case TurnPhase.Play:
                    bool mustLead = !prePlay && !prePrePlay;
                    Card[] cards = mustLead
                        ? new[] { playerCard[actor].OrderBy(c => c.rank).ThenBy(c => c.suit).First() }
                        : Array.Empty<Card>();
                    ApplyPlay(actor, new MsgPlayCards
                    {
                        turnId = TurnId, play = mustLead, cards = CardManager.GetCardInfos(cards)
                    });
                    break;
            }
            return;
        }
        if (GetTurnState().secondsRemaining != lastTimerSecond)
            BroadcastTurnState();
    }

    private bool AcceptAction(string actor, long turnId, TurnPhase phase)
    {
        // 先结算已到期回合，再核对编号；迟到的请求不会被当作新回合操作。
        UpdateTurnTimer();
        if (TimerActive && Phase == phase && currentPlayer == actor && TurnId == turnId)
            return true;
        PlayerManager.GetPlayer(actor)?.Send(GetTurnState());
        return false;
    }

    public void TryCall(string actor, MsgCall msg)
    {
        if (AcceptAction(actor, msg.turnId, TurnPhase.Call))
            ApplyCall(actor, msg);
    }

    private void ApplyCall(string actor, MsgCall msg)
    {
        msg.id = actor;
        if (msg.call)
        {
            callID = actor;
            landLordRank[actor] += 2;
            msg.result = CheckCall() ? 3 : 1;
            if (msg.result == 3)
                SetLandLord(actor);
        }
        else
        {
            landLordRank[actor]++;
            msg.result = CheckAllNotCall() ? 2 : 0;
        }
        Send(msg);
        if (msg.result == 2)
        {
            Redeal();
            return;
        }
        if (msg.result == 1)
            Send(new MsgStartRob());
        BeginTurn(landLord != "" ? TurnPhase.Play : msg.call ? TurnPhase.Rob : TurnPhase.Call,
            landLord != "" ? Index : Index + 1);
        BroadcastTurnState();
    }

    private void Redeal()
    {
        CardManager.Shuffle();
        cards = CardManager.cards;
        Start();
        Send(new MsgReStart());
        // 主动发牌，不依赖超时玩家的客户端继续发起请求。
        foreach (string playerId in playerList)
        {
            PlayerManager.GetPlayer(playerId).Send(new MsgGetCardList
            {
                cardInfos = CardManager.GetCardInfos(playerCard[playerId].ToArray()),
                threeCards = CardManager.GetCardInfos(playerCard[""].ToArray())
            });
        }
        BroadcastTurnState();
    }

    public void TryRob(string actor, MsgRob msg)
    {
        if (AcceptAction(actor, msg.turnId, TurnPhase.Rob))
            ApplyRob(actor, msg);
    }

    private void ApplyRob(string actor, MsgRob msg)
    {
        msg.id = actor;
        msg.landLord = "";
        if (msg.rob)
            landLordRank[actor] += robRank++;
        else
        {
            // 不抢不能增加叫地主者的竞争权值；否则可能和唯一抢过的人打平。
            if (actor != callID)
                landLordRank[actor]++;
            if (CheckCall())
                msg.landLord = callID;
        }
        if (actor == callID)
            msg.landLord = CheckLandLord();
        if (msg.landLord != "")
        {
            SetLandLord(msg.landLord);
            msg.needRob = false;
            Send(msg);
            BeginTurn(TurnPhase.Play, Index);
        }
        else
        {
            msg.needRob = landLordRank[playerList[(Index + 1) % maxPlayer]] != 0;
            Send(msg);
            BeginTurn(TurnPhase.Rob, Index + (msg.needRob ? 1 : 2));
        }
        BroadcastTurnState();
    }

    public void TryPlay(string actor, MsgPlayCards msg)
    {
        if (AcceptAction(actor, msg.turnId, TurnPhase.Play))
            ApplyPlay(actor, msg);
    }

    private void ApplyPlay(string actor, MsgPlayCards msg)
    {
        msg.id = actor;
        msg.result = false;
        msg.win = 0;
        bool following = prePlay || prePrePlay;
        if (msg.play)
        {
            // 超时可能已打出一张牌，不能再接受客户端旧手牌或重复的牌。
            if (msg.cards == null || msg.cards.Length == 0 || msg.cards.Length > 20 || msg.cards.Any(c => c == null))
            {
                RejectPlay(actor, msg);
                return;
            }
            Card[] played = CardManager.GetCards(msg.cards);
            if (played.Distinct().Count() != played.Length || played.Any(c => !playerCard[actor].Contains(c)))
            {
                RejectPlay(actor, msg);
                return;
            }
            msg.cardType = (int)CardManager.GetCardType(played);
            msg.result = msg.cardType != (int)CardManager.CardType.wrong &&
                (!following || CardManager.Compare(preCard.ToArray(), played));
            if (!msg.result)
            {
                RejectPlay(actor, msg);
                return;
            }
            firstPlay = false;
            DeleteCards(played, actor);
            msg.win = CheckWin();
            preCard = played.ToList();
            prePrePlay = prePlay;
            prePlay = true;
        }
        else
        {
            if (!following)
            {
                RejectPlay(actor, msg);
                return;
            }
            prePrePlay = prePlay;
            prePlay = false;
            msg.cards = Array.Empty<CardInfo>();
            msg.result = true;
        }
        msg.canNotPlay = prePlay || prePrePlay;
        Send(msg);
        if (msg.win != 0)
        {
            StopTurnTimer();
            return;
        }
        BeginTurn(TurnPhase.Play, Index + 1);
        BroadcastTurnState();
    }

    private void RejectPlay(string actor, MsgPlayCards msg)
    {
        msg.result = false;
        msg.canNotPlay = prePlay || prePrePlay;
        PlayerManager.GetPlayer(actor).Send(msg);
        PlayerManager.GetPlayer(actor).Send(GetTurnState());
    }
}
