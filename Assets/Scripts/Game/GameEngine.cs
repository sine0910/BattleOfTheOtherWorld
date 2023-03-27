using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEngine
{
    public List<Player> players;

    public GameEngine()
    {
        players = new List<Player>();

        for (int i = 0; i < 2; i++)
        {
            players.Add(new Player());
        }
    }

    public void GetPlayerCardList(byte player_index, List<int> card_list)
    {
        players[player_index].player_cards = card_list;

        //���ӿ� �ʿ��� �� ī�� 15�� ���� ����
        Shuffle<int>(card_list);
        for (int i = 0; i < 15; i++)
        {
            players[player_index].player_deck_cards.Enqueue(card_list[i]);
        }
    }

    public void GameStart()
    {
        for (int i = 0; i < players.Count; i++)
        {
            players[i].Init();
        }

        //ī���� 3���� ���������� �÷��̾�� ������ �ش�.
        GenerateCards();
    }

    //ī�� ���� �Լ�
    public void GenerateCards()
    {
        for (int i = 0; i < players.Count; i++)
        {
            //������ �� ī�忡�� �ڵ� ī�� 3���� �������� ����
            for (int k = 0; k < 3; k++)
            {
                players[i].player_hand_cards.Add(CardManager.instance.GetCard(players[i].player_deck_cards.Dequeue()));
                if (players[i].player_hand_cards.Find(x => CardManager.instance.GetCardType(x.card_index) != CardType.CHARACTER) == null)
                {
                    
                }
            }
        }
    }    
    
    //ī�带 ���� ���� �Լ�
    public static void Shuffle<T>(List<T> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    //ī�带 �ڵ忡�� ���ͷ� �ű�� �Լ�
    public void SetToCenter(byte player_index, int card_index)
    {
        Card card = players[player_index].player_hand_cards.Find(x => x.card_index == card_index);

        players[player_index].player_hand_cards.Remove(card);
        players[player_index].player_center_cards.Add(card);
    }

    //������ ī�带 ��� �Ǵ� �������� ��ȯ
    public void SetCardStatus(byte player_index, int card_index, CardStatus status)
    {
        players[player_index].player_center_cards.Find(x => x.card_index == card_index).SetStatus(status);
    }

    public void GetPlayerBattlePoint()
    {
        for (int i = 0; i < players.Count; i++)
        {
            int bp = 0;

            //���� ġ�� ī����� ����� ����� ��� �ʿ�


            //���� ���� ī�� ����


            //ĳ���� ī�� ����
            for (int k = 0; k < players[i].player_center_cards.Count; k++)
            {
                CardData data = CardManager.instance.GetCardData(players[i].player_center_cards[k].card_index);

                switch (data.type)
                {
                    case CardType.CHARACTER:
                        {
                            switch (players[i].player_center_cards[k].status)
                            {
                                case CardStatus.ATTACK:
                                    bp += data.attack;
                                    break;

                                case CardStatus.DEFENSE:
                                    bp += data.defense;
                                    break;
                            }
                        }
                        break;
                }
                
            }

            players[i].battle_point = bp;
        }
    }
}
