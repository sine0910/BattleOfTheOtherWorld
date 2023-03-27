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

    public byte player_index;

    public string player_name;

    public int score;
    public int battle_point;

    //0,1 Àº °ø°Ý ½½·Ô 2´Â ¹æ¾î ½½·Ô
    public Dictionary<CardType, List<Card>> player_type_cards = new Dictionary<CardType, List<Card>>();

    public List<Card> player_center_cards = new List<Card>();
    public List<Card> player_hand_cards = new List<Card>();
    public Queue<int> player_deck_cards = new Queue<int>();

    public List<int> player_cards = new List<int>();

    public Player()
    {

    }

    public void Init()
    {
        player_type_cards.Clear();
        player_center_cards.Clear();
        player_hand_cards.Clear();
        battle_point = 0;
    }

    public void send(List<string> msg)
    {
        List<string> clone = msg.ToList();
        this.send_function(msg);
    }
}
