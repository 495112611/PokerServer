using System.Net;
using System.Net.Sockets;

static class Program
{
    private static long now;
    private static int checks;

    static void Main()
    {
        TextWriter output = Console.Out;
        Console.SetOut(TextWriter.Null);
        try
        {
            TimingAndStaleRequests();
            BiddingCombinations();
            AutomaticPlayAndStop();
            BroadcastRoundTrip();
        }
        finally { Console.SetOut(output); }
        Console.WriteLine($"PASS: {checks} assertions; 72 bidding combinations, deadlines, stale/duplicate requests, automatic play/pass, redeal, disconnect, win and serialized broadcasts.");
    }

    static void Check(bool result, string message)
    {
        checks++;
        if (!result) throw new Exception(message);
    }

    static Room Create(int seat = 0)
    {
        now = 100000;
        RoomManager.rooms.Clear();
        Room room = new Room(() => now) { id = 1, hostId = "P1" };
        foreach (string id in new[] { "P1", "P2", "P3" })
        {
            PlayerManager.RemovePlayer(id);
            PlayerManager.AddPlayer(id, new Player(null) { id = id, roomId = room.id, data = new PlayerData() });
            room.playerList.Add(id);
        }
        RoomManager.rooms.Add(room.id, room);
        room.Start();
        room.Index = seat;
        room.currentPlayer = room.playerList[seat];
        return room;
    }

    static void Call(Room r, bool value) => r.TryCall(r.currentPlayer, new MsgCall { call = value, turnId = r.TurnId });
    static void Rob(Room r, bool value) => r.TryRob(r.currentPlayer, new MsgRob { rob = value, turnId = r.TurnId });
    static void Expire(Room r) { now = r.TurnDeadline; EventHandler.OnTimer(); }

    static Room PlayRoom()
    {
        Room r = Create();
        Call(r, true); Rob(r, false); Rob(r, false);
        Check(r.Phase == Room.TurnPhase.Play && r.currentPlayer == "P1", "Landlord must lead");
        return r;
    }

    static void TimingAndStaleRequests()
    {
        Room r = Create();
        Check(r.GetTurnState().secondsRemaining == 15, "Initial bid duration");
        now += 1000; r.UpdateTurnTimer();
        Check(r.GetTurnState().secondsRemaining == 14, "One second must expire on server");
        long turn = r.TurnId, deadline = r.TurnDeadline;
        r.TryCall("P2", new MsgCall { call = true, turnId = turn });
        Check(r.TurnId == turn && r.callID == "", "Wrong player cannot act");
        ClientState state = new ClientState { player = PlayerManager.GetPlayer("P1") };
        MsgHandler.MsgSwitchTurn(state, new MsgSwitchTurn { round = 2 });
        MsgHandler.MsgStartRob(state, new MsgStartRob());
        MsgHandler.MsgReStart(state, new MsgReStart());
        MsgHandler.MsgGetStartPlayer(state, new MsgGetStartPlayer());
        Check(r.TurnId == turn && r.TurnDeadline == deadline, "Client queries cannot reset or advance timer");
        now = deadline - 1; r.UpdateTurnTimer();
        Check(r.TurnId == turn && r.GetTurnState().secondsRemaining == 1, "Cannot expire early");
        now = deadline;
        r.TryCall("P1", new MsgCall { call = true, turnId = turn });
        Check(r.currentPlayer == "P2" && r.callID == "", "At deadline timeout wins over late click");
        long next = r.TurnId;
        r.UpdateTurnTimer(); r.UpdateTurnTimer();
        Check(r.TurnId == next && r.GetTurnState().secondsRemaining == 15, "Timeout must advance once");
        Expire(r); Expire(r);
        Check(r.Phase == Room.TurnPhase.Call && r.landLordRank.Values.All(x => x == -1), "All decline must redeal automatically");
        Check(r.playerCard.Values.Sum(x => x.Count) == 54, "Redeal must produce 54 cards");
        string actor = r.currentPlayer;
        r.TryCall(actor, new MsgCall { call = true, turnId = turn });
        Check(r.callID == "", "Old deal's action must be ignored");
    }

    static void BiddingCombinations()
    {
        for (int seat = 0; seat < 3; seat++)
        for (int declines = 0; declines < 3; declines++)
        for (int choices = 0; choices < 8; choices++)
        {
            Room r = Create(seat);
            for (int i = 0; i < declines; i++) Call(r, false);
            string expected = r.currentPlayer;
            Call(r, true);
            int step = 0;
            while (r.Phase == Room.TurnPhase.Rob && step < 3)
            {
                bool rob = (choices & (1 << step++)) != 0;
                if (rob) expected = r.currentPlayer;
                Rob(r, rob);
            }
            Check(r.landLord == expected && r.currentPlayer == expected, "Last willing bidder must be landlord and first player");
            Check(r.Phase == Room.TurnPhase.Play && r.GetTurnState().secondsRemaining == 30, "Play gets a fresh 30 seconds");
            foreach (string id in r.playerList)
                Check(r.playerCard[id].Count == (id == expected ? 20 : 17), "Bottom cards granted once");
            long turn = r.TurnId;
            r.TryRob(expected, new MsgRob { rob = true, turnId = turn - 1 });
            Check(r.TurnId == turn && r.playerCard[expected].Count == 20, "Late rob cannot duplicate bottom cards");
        }
        Room timeout = Create();
        Call(timeout, true); Rob(timeout, true); Rob(timeout, false); Expire(timeout);
        Check(timeout.landLord == "P2", "Caller timing out must not win a tie with sole robber");
    }

    static void AutomaticPlayAndStop()
    {
        Room r = PlayRoom();
        long turn = r.TurnId, deadline = r.TurnDeadline;
        r.TryPlay("P1", new MsgPlayCards { turnId = turn, play = false });
        r.TryPlay("P1", new MsgPlayCards { turnId = turn, play = true, cards = Array.Empty<CardInfo>() });
        Card unowned = r.playerCard["P2"][0];
        r.TryPlay("P1", new MsgPlayCards { turnId = turn, play = true, cards = CardManager.GetCardInfos(new[] { unowned }) });
        Check(r.TurnId == turn && r.TurnDeadline == deadline, "Invalid play/pass must not reset timer");
        Card smallest = r.playerCard["P1"].OrderBy(c => c.rank).ThenBy(c => c.suit).First();
        Expire(r);
        Check(r.currentPlayer == "P2" && r.preCard.Count == 1 && r.preCard[0].Equals(smallest), "Timeout lead must play smallest single");
        Check(r.playerCard["P1"].Count == 19, "Timeout updates server hand");
        long next = r.TurnId;
        r.TryPlay("P1", new MsgPlayCards { turnId = turn, play = true, cards = CardManager.GetCardInfos(new[] { smallest }) });
        Check(r.TurnId == next && r.playerCard["P1"].Count == 19, "Late manual lead cannot play twice");
        Expire(r);
        Check(r.currentPlayer == "P3" && r.playerCard["P2"].Count == 17, "Follow timeout passes");
        Expire(r);
        Check(r.currentPlayer == "P1" && !r.GetTurnState().canNotPlay, "Two passes give leader a mandatory play");
        Expire(r);
        Check(r.playerCard["P1"].Count == 18, "Later mandatory lead also plays smallest single");
        int loops = 0;
        while (r.TimerActive && loops++ < 100) Expire(r);
        Check(!r.TimerActive && r.playerCard["P1"].Count == 0, "Fully automatic hand must reach win");
        turn = r.TurnId; now += 100000; r.UpdateTurnTimer();
        Check(r.TurnId == turn && !r.GetTurnState().active, "Timer must stay stopped after win");
        r = PlayRoom(); r.RemovePlayer("P2");
        turn = r.TurnId; now += 100000; r.UpdateTurnTimer();
        Check(!r.TimerActive && r.TurnId == turn, "Leaving room stops timer");
    }

    static void BroadcastRoundTrip()
    {
        Room r = Create();
        using Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        listener.Bind(new IPEndPoint(IPAddress.Loopback, 0)); listener.Listen(1);
        using Socket receiver = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        receiver.Connect(listener.LocalEndPoint);
        using Socket sender = listener.Accept();
        receiver.ReceiveTimeout = 2000;
        PlayerManager.GetPlayer("P1").state = new ClientState { socket = sender };
        r.BroadcastTurnState();
        MsgTurnState state = (MsgTurnState)ReadMessage(receiver);
        Check(state.active && state.secondsRemaining == 15 && state.turnId == r.TurnId && state.id == r.currentPlayer, "Serialized turn state");
        now += 1000; r.UpdateTurnTimer();
        state = (MsgTurnState)ReadMessage(receiver);
        Check(state.secondsRemaining == 14 && state.turnId == r.TurnId, "Timer broadcasts new second without new turn");
        r.UpdateTurnTimer();
        Check(!receiver.Poll(10000, SelectMode.SelectRead), "Do not broadcast same second repeatedly");
        PlayerManager.GetPlayer("P1").state = null;
    }

    static MsgBase ReadMessage(Socket socket)
    {
        byte[] Read(int count)
        {
            byte[] bytes = new byte[count]; int offset = 0;
            while (offset < count) offset += socket.Receive(bytes, offset, count - offset, SocketFlags.None);
            return bytes;
        }
        byte[] length = Read(2);
        byte[] body = Read(length[0] + length[1] * 256);
        string name = MsgBase.DecodeName(body, 0, out int nameCount);
        return MsgBase.Decode(name, body, nameCount, body.Length - nameCount);
    }
}
