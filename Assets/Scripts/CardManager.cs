using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardType : byte
{
    CHARACTER,
    WEAPON,
    CHEATING
}

public enum CardStatus : byte
{
    ATTACK,
    DEFENSE
}

public class CardManager : MonoBehaviour
{
    public static CardManager instance;

    public Dictionary<int, CardData> card_datas = new Dictionary<int, CardData>();
    public Dictionary<int, Sprite> card_sprites = new Dictionary<int, Sprite>();
    public Dictionary<int, Sprite> card_back_sprites = new Dictionary<int, Sprite>();

    public void Start()
    {
        instance = this;
    }

    public Card GetCard(int i)
    {
        return new Card(i);
    }

    public CardData GetCardData(int i)
    {
        return card_datas[i];
    }

    public Sprite GetCardSprites(int i)
    {
        return card_sprites[i];
    }

    public Sprite GetCardBackSprites(int i)
    {
        return card_back_sprites[i];
    }

    public CardType GetCardType(int i)
    {
        if (i < 38)
        {
            return CardType.CHARACTER;
        }
        else if (i < 46)
        {
            return CardType.CHARACTER;
        }
        else
        {
            return CardType.CHEATING;
        }
    }
}

[SerializeField]
public class CardData
{
    public int attack;
    public int defense;
    public CardType type;

    public CardData(int a, int d, int t, int s)
    {
        attack = a;
        defense = d;
        type = (CardType)t;
    }
}
