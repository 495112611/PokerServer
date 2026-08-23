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
        threeWithOne,//三代一
        threeWithTwo,//三带二
        airplane,//飞机
        airplaneWithOne,//飞机带一
        airplaneWithTwo,//飞机带二
        chain,//顺子
        pairChain,//连对
        bomb,//炸弹
        fourWithTwo,//四带二
        jokerBomb,//王炸
        wrong //错误类型
    }
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
        //判断四张三代一
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
        //判断三代二
        if (len == 5)
        {
            if (rank[0] == rank[1] && rank[0] == rank[2] && rank[3] == rank[4])
                cardType = CardType.threeWithTwo;
            else if (rank[0] == rank[1] && rank[2] == rank[3] && rank[2] == rank[4])
                cardType = CardType.threeWithTwo;
        }
        //判断顺子
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
        //判断连对
        if (len >= 6 && len % 2 == 0)
        {
            bool result = true;
            for (int i = 0; i < len; i += 2)
            {
                if (rank[i] != rank[i + 1])//第一张和第三张相减不等于-1说明不是连对
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
                if (rank[i] != rank[i + 1] || rank[i] != rank[i + 2])//判断前三张是不是不一样
                    result = false;
                if (rank[i] == 12)//除去2之后的
                    result = false;
            }
            for (int i = 0; i < len - 3; i += 3)
            {
                if (rank[i] - rank[i + 3] != -1)//第一张和第4张相减不等于-1说明不是连对
                    result = false;
            }
            if (result)
                cardType = CardType.airplane;
        }
        //     //判断飞机带一
        //     //思路：把4个看成一组，拿到一共有几组，用一个数组变量存起来有几组，然后判断存起来的数组相邻的差是不是-1
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
                    arr[index++] = rank[i];//每次拿到一组之后index需要++
                    i += 2;
                }
            }
            if (planeLen == index)
            {
                for (int i = 0; i < planeLen - 1; i++)
                {
                    if (arr[i] == 12 || arr[i + 1] == 12)//除去2之后的
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
        //判断飞机带二
        //思路：把4个看成一组，拿到一共有几组，用一个数组变量存起来有几组，然后判断存起来的数组相邻的差是不是-1
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
                    arr[index++] = rank[i];//每次拿到一组之后index需要++
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
                        i++;//如果一样的话 i在for里面+1了 对比下个对子的时候 需要让i再次+1 比如6677 i如果只+1的话 会第二个6和1去比较，所以需要再次+1
                }
            }
            if (result)
                cardType = CardType.airplaneWithTwo;
        }
        //判断4带2
        //思路：从1或者2或者3开始向后面开始算 满足四个相等
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

