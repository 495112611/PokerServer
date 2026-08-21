using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class CardManager
{
    /// <summary>
    /// 卡片集合
    /// </summary>
    public static List<Card> cards = new List<Card>();
    /// <summary>
    /// 卡牌类型
    /// </summary>
    public enum CardType
    {
        one,//单张
        two,//对子
        three,//三张
        threeWithOne,//三带一
        threeWithTwo,//三带一对
        airplane,//飞机 333 444
        airplaneWithOne,//飞机带单张  333 444 5 6
        airplaneWithTwo,//飞机带对 333 444 55 66
        chain,//顺子
        pairChain,//连对
        bomb,//炸弹 4444
        fourWithTwo,//四带二 4444 5 6
        jokerBomb,//王炸
        wrong//错误类型
    }
    /// <summary>
    /// 获取牌型
    /// </summary>
    /// <param name="cards"></param>
    /// <returns></returns>
    public static CardType GetCardType(Card[] cards)
    {
        int[] rank = new int[20];
        int len = cards.Length;
        for (int i = 0; i < len; i++)
        {
            rank[i] = (int)cards[i].rank;
        }
        Array.Sort(rank, 0, len);
        CardType cardType = CardType.wrong;
        if (len == 1)
        {
            cardType = CardType.one;
        }
        if (len == 2)
        {
            if (rank[0] == rank[1])
                cardType = CardType.two;
            if (rank[0] + rank[1] == 27)
                cardType = CardType.jokerBomb;
        }
        if (len == 3)
        {
            if (rank[0] == rank[1] && rank[1] == rank[2])
                cardType = CardType.three;
        }
        if (len == 4)
        {
            if (rank[0] == rank[1] && rank[0] == rank[2] && rank[0] == rank[3])
                cardType = CardType.bomb;
            else if (rank[0] == rank[1] && rank[0] == rank[2])
                cardType = CardType.threeWithOne;
            else if (rank[1] == rank[2] && rank[1] == rank[3])
                cardType = CardType.threeWithOne;
        }
        //不支持带对王的
        if (len == 5)
        {
            if (rank[0] == rank[1] && rank[0] == rank[2] && rank[3] == rank[4])
                cardType = CardType.threeWithTwo;
            else if (rank[0] == rank[1] && rank[2] == rank[3] && rank[2] == rank[4])
                cardType = CardType.threeWithTwo;
        }
        if (len >= 5 && len <= 12)
        {
            bool result = true;
            for (int i = 0; i < len - 1; i++)
            {
                if (rank[i] >= 12 || rank[i + 1] >= 12)
                    result = false;
                if (rank[i] - rank[i + 1] != -1)
                    result = false;
            }
            if (result)
                cardType = CardType.chain;
        }
        if (len >= 6 && len % 2 == 0)
        {
            bool result = true;
            for (int i = 0; i < len; i += 2)
            {
                if (rank[i] != rank[i + 1])
                    result = false;
                if (rank[i] == 12)
                    result = false;
            }
            for (int i = 0; i < len - 2; i += 2)
            {
                if (rank[i] - rank[i + 2] != -1)
                    result = false;
            }
            if (result)
                cardType = CardType.pairChain;
        }
        if (len >= 6 && len % 3 == 0)
        {
            bool result = true;
            for (int i = 0; i < len; i += 3)
            {
                if (rank[i] != rank[i + 1] || rank[i] != rank[i + 2])
                    result = false;
                if (rank[i] == 12)
                    result = false;
            }
            for (int i = 0; i < len - 3; i += 3)
            {
                if (rank[i] - rank[i + 3] != -1)
                    result = false;
            }
            if (result)
                cardType = CardType.airplane;
        }
        if (len >= 8 && len % 4 == 0)
        {
            bool result = true;
            int planeLen = len / 4;
            int[] arr = new int[planeLen];
            int index = 0;
            for (int i = 0; i < len - 2; i++)
            {
                if (rank[i] == rank[i + 1] && rank[i] == rank[i + 2])
                {
                    arr[index++] = rank[i];
                    i += 2;
                }
            }
            if (planeLen == index)
            {
                for (int i = 0; i < planeLen - 1; i++)
                {
                    if (arr[i] == 12 || arr[i + 1] == 12)
                        result = false;
                    if (arr[i] - arr[i + 1] != -1)
                        result = false;
                }
            }
            else
            {
                result = false;
            }
            if (result)
                cardType = CardType.airplaneWithOne;
        }
        if (len >= 10 && len % 5 == 0)
        {
            bool result = true;
            int planeLen = len / 5;
            int[] arr = new int[planeLen];
            int index = 0;
            for (int i = 0; i < len - 2; i++)
            {
                if (rank[i] == rank[i + 1] && rank[i] == rank[i + 2])
                {
                    arr[index++] = rank[i];
                    i += 2;
                }
            }
            if (planeLen == index)
            {
                for (int i = 0; i < planeLen - 1; i++)
                {
                    if (arr[i] == 12 || arr[i + 1] == 12)
                        result = false;
                    if (arr[i] - arr[i + 1] != -1)
                        result = false;
                }
            }
            else
            {
                result = false;
            }
            for (int i = 0; i < len - 1; i++)
            {
                if (!arr.Contains(rank[i]))
                {
                    if (rank[i] != rank[i + 1])
                        result = false;
                    else
                        i++;
                }
            }
            if (result)
                cardType = CardType.airplaneWithTwo;
        }
        if (len == 6)
        {
            for (int i = 0; i < 3; i++)
            {
                if (rank[i] == rank[i + 1] && rank[i] == rank[i + 2] && rank[i] == rank[i + 3])
                    cardType = CardType.fourWithTwo;
            }
        }
        return cardType;
    }
    public static bool Compare(Card[] preCards, Card[] cards)
    {
        Array.Sort(preCards, (Card c1, Card c2) => (int)c1.rank - (int)c2.rank);
        Array.Sort(cards, (Card c1, Card c2) => (int)c1.rank - (int)c2.rank);
        if (GetCardType(cards) == CardType.jokerBomb)
            return true;
        if (GetCardType(cards) == CardType.bomb && GetCardType(preCards) != CardType.bomb)
            return true;

        if (GetCardType(preCards) == GetCardType(cards))
        {
            switch (GetCardType(cards))
            {
                case CardType.one:
                    if (preCards[0].rank < cards[0].rank)
                        return true;
                    return false;
                case CardType.two:
                    if (preCards[0].rank < cards[0].rank)
                        return true;
                    return false;
                case CardType.three:
                    if (preCards[0].rank < cards[0].rank)
                        return true;
                    return false;
                case CardType.threeWithOne:
                    if (preCards[1].rank < cards[1].rank)
                        return true;
                    return false;
                case CardType.threeWithTwo:
                    if (preCards[2].rank < cards[2].rank)
                        return true;
                    return false;
                case CardType.airplane:
                    if (preCards.Length == cards.Length)
                        if (preCards[0].rank < cards[0].rank)
                            return true;
                    return false;
                case CardType.airplaneWithOne:
                case CardType.airplaneWithTwo:
                    if (preCards.Length == cards.Length)
                    {
                        int preIndex = 0;
                        for (int i = 0; i < cards.Length - 2; i++)
                        {
                            if (preCards[i].rank == preCards[i + 1].rank && preCards[i].rank == preCards[i + 2].rank)
                                preIndex = i;
                        }
                        int index = 0;
                        for (int i = 0; i < cards.Length - 2; i++)
                        {
                            if (cards[i].rank == cards[i + 1].rank && cards[i].rank == cards[i + 2].rank)
                                index = i;
                        }
                        if (preCards[preIndex].rank < cards[index].rank)
                            return true;
                    }
                    return false;
                case CardType.chain:
                    if (preCards.Length == cards.Length)
                    {
                        if (preCards[0].rank < cards[0].rank)
                            return true;
                    }
                    return false;
                case CardType.pairChain:
                    if (preCards.Length == cards.Length)
                    {
                        if (preCards[0].rank < cards[0].rank)
                            return true;
                    }
                    return false;
                case CardType.bomb:
                    if (preCards[0].rank < cards[0].rank)
                        return true;
                    return false;
                case CardType.fourWithTwo:
                    if (preCards[2].rank < cards[2].rank)
                        return true;
                    return false;
                case CardType.jokerBomb:
                case CardType.wrong:
                    break;
            }
        }
        return false;
    }
    /// <summary>
    /// 洗牌
    /// </summary>
    public static void Shuffle()
    {
        cards.Clear();
        for (int i = 1; i < 5; i++)
        {
            for (int j = 0; j < 13; j++)
            {
                Card card = new Card(i, j);
                cards.Add(card);
            }
        }

        cards.Add(new Card(Suit.None, Rank.SJoker));
        cards.Add(new Card(Suit.None, Rank.LJoker));

        Queue<Card> cardQueue = new Queue<Card>();
        for (int i = 0; i < 54; i++)
        {
            Random random = new Random();
            int index = random.Next(cards.Count);
            cardQueue.Enqueue(cards[index]);
            cards.RemoveAt(index);
        }

        for (int i = 0; i < 54; i++)
        {
            cards.Add(cardQueue.Dequeue());
        }
    }
    /// <summary>
    /// Card数组转CardInfo数组
    /// </summary>
    /// <param name="cards"></param>
    /// <returns></returns>
    public static CardInfo[] GetCardInfos(Card[] cards)
    {
        CardInfo[] infos = new CardInfo[cards.Length];
        for (int i = 0; i < infos.Length; i++)
        {
            infos[i] = cards[i].GetCardInfo();
        }
        return infos;
    }
    public static Card[] GetCards(CardInfo[] cardInfos)
    {
        Card[] cards = new Card[cardInfos.Length];
        for (int i = 0; i < cardInfos.Length; i++)
        {
            cards[i] = new Card(cardInfos[i].suit, cardInfos[i].rank);
        }
        return cards;
    }
}

