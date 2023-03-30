using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MULTI_PLAYER_TYPE : byte
{
    HOST,
    GUEST
}

public class Player
{
    public delegate void SendFn(List<string> msg);
    SendFn send_function;

    public string player_name;

    public int score;
    public int battle_point;

    //0,1 Àº °ø°Ý ½½·Ô 2´Â ¹æ¾î ½½·Ô
    public Dictionary<CardType, List<Card>> player_type_cards = new Dictionary<CardType, List<Card>>();

    public List<Card> player_center_cards = new List<Card>();
    public List<Card> player_hand_cards = new List<Card>();
    public List<Card> player_deck_cards = new List<Card>();

    public List<Card> player_used_cards = new List<Card>();

    public List<int> player_cards = new List<int>();

    public bool destroy_cheating_card = false;
    public bool destroy_weapon_card = false;
    public bool destroy_character_card = false;

    public bool draw = false;

    public Player()
    {
        player_type_cards.Add(CardType.CHARACTER, new List<Card>());
        player_type_cards.Add(CardType.WEAPON, new List<Card>());
        player_type_cards.Add(CardType.CHEATING, new List<Card>());
    }

    public void Init()
    {
        player_type_cards[CardType.CHARACTER].Clear();
        player_type_cards[CardType.WEAPON].Clear();
        player_type_cards[CardType.CHEATING].Clear();
        player_center_cards.Clear();
        player_hand_cards.Clear();
        player_deck_cards.Clear();
        player_used_cards.Clear();
        score = 0;
        battle_point = 0;

        destroy_cheating_card = false;
        destroy_weapon_card = false;
        destroy_character_card = false;

        draw = false;
    }

    public void TurnInit()
    {
        player_type_cards[CardType.CHARACTER].Clear();
        player_type_cards[CardType.WEAPON].Clear();
        player_type_cards[CardType.CHEATING].Clear();
        player_center_cards.Clear();
        player_hand_cards.Clear();
        battle_point = 0;

        destroy_cheating_card = false;
        destroy_weapon_card = false;
        destroy_character_card = false;

        draw = false;
    }

    public void send(List<string> msg)
    {
        List<string> clone = msg.ToList();
        this.send_function(msg);
    }
}
