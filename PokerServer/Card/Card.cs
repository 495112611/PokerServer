using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable disable
public enum Suit
{
    None,
    Club,
    Square,
    Heart,
    Spade,
}
public enum Rank
{
    Three,
    Four,
    Five,
    Six,
    Seven,
    Eight,
    Nine,
    Ten,
    Jack,
    Queen,
    King,
    One,
    Two,
    SJoker,
    LJoker,
}
public class Card
{
    public Suit suit;
    public Rank rank;

    public Card(Suit suit, Rank rank)
    {
        this.suit = suit;
        this.rank = rank;
    }
    public Card(int suit,int rank)
    {
        this.suit = (Suit)suit;
        this.rank = (Rank)rank;
    }
    public override bool Equals(object obj)
    {
        if (obj == null)
            return false;
        Card card = obj as Card;
        if (card == null)
            return false;
        return suit == card.suit && rank == card.rank;
    }
    public override int GetHashCode()
    {
        return Tuple.Create(suit, rank).GetHashCode();
    }
    public CardInfo GetCardInfo()
    {
        CardInfo cardInfo = new CardInfo();
        cardInfo.suit = (int)suit;
        cardInfo.rank = (int)rank;
        return cardInfo;
    }
}

