using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEngine
{
    public List<Player> players;

    public int turn = 0;

    public GameEngine()
    {
        players = new List<Player>();

        for (int i = 0; i < 2; i++)
        {
            players.Add(new Player());
        }

        Init();
    }

    public void GetPlayerCardList(byte player_index, List<int> card_list)
    {
        players[player_index].player_cards = card_list;

        //���ӿ� �ʿ��� �� ī�� 15�� ���� ����
        Shuffle<int>(card_list);
        for (int i = 0; i < 15; i++)
        {
            players[player_index].player_deck_cards.Add(CardManager.instance.GetCard(card_list[i]));
        }
    }

    public void Init()
    {
        turn = 0;

        for (int i = 0; i < players.Count; i++)
        {
            players[i].Init();
        }
    }

    public void GameStart()
    {
        for (int i = 0; i < players.Count; i++)
        {
            players[i].TurnInit();
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
                Card c = players[i].player_deck_cards[k];

                Card cc = players[i].player_hand_cards.Find(x => CardManager.instance.GetCardType(x.card_index) == CardType.CHARACTER);
                //ĳ���� ī�尡 �ʼ������� �������� ĳ���� ī�尡 ���� �� ���� �ǵ�����.
                if (cc == null && k == 2)
                {
                    k--; 
                    Shuffle<Card>(players[i].player_deck_cards);
                }
                else
                {
                    players[i].player_hand_cards.Add(c);
                    players[i].player_deck_cards.Remove(c);
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

        players[player_index].player_type_cards[CardManager.instance.GetCardType(card_index)].Add(card);
    }

    //������ ī�带 ��� �Ǵ� �������� ��ȯ
    public void SetCardStatus(byte player_index, int card_index, CardStatus status)
    {
        players[player_index].player_center_cards.Find(x => x.card_index == card_index).SetStatus(status);
    }

    public void GetPlayerBattlePoint()
    {
        int bp = 0;
        int[] w_bp = new int[2];

        for (byte i = 0; i < players.Count; i++)
        {
            //ġ�� ī�带 �ı��ϴ� 26���� ���������� ���� ó���Ѵ�.
            if (players[i].player_type_cards[CardType.CHEATING].Find(x => x.card_index == 26) != null)
            {
                UsingCheatingCards(i, 26);
            }
            //�����ϰ� ���� ī�带 �ı��ϴ� 27���� ���� ó���Ѵ�.
            if (players[i].player_type_cards[CardType.CHEATING].Find(x => x.card_index == 27) != null)
            {
                UsingCheatingCards(i, 27);
            }
            //�����ϰ� ��� ī�带 �ı��ϴ� 39���� ���� ó���Ѵ�.
            if (players[i].player_type_cards[CardType.CHEATING].Find(x => x.card_index == 39) != null)
            {
                UsingCheatingCards(i, 39);
            }
        }

        //���� ġ�� ī����� ����� ����� ��� �ʿ�
        for (byte i = 0; i < players.Count; i++)
        {
            if (!players[i].destroy_cheating_card)
            {
                for (int k = 0; k < players[i].player_type_cards[CardType.CHEATING].Count; k++)
                {
                    CardData data = CardManager.instance.GetCardData(players[i].player_type_cards[CardType.CHEATING][k].card_index);

                    UsingCheatingCards(i, players[i].player_center_cards[k].card_index);
                }
            }
        }

        //���� ���� ī�� ����
        for (byte i = 0; i < players.Count; i++)
        {
            if (!players[i].destroy_weapon_card)
            {
                for (int k = 0; k < players[i].player_type_cards[CardType.WEAPON].Count; k++)
                {
                    CardData data = CardManager.instance.GetCardData(players[i].player_type_cards[CardType.WEAPON][k].card_index);

                    w_bp[0] += data.attack;
                    w_bp[1] += data.defense;
                }
            }
        }

        //ī��� ����
        for (byte i = 0; i < players.Count; i++)
        {
            if (!players[i].destroy_character_card)
            {
                for (int k = 0; k < players[i].player_type_cards[CardType.CHARACTER].Count; k++)
                {
                    CardData data = CardManager.instance.GetCardData(players[i].player_type_cards[CardType.CHARACTER][k].card_index);

                    switch (players[i].player_center_cards[k].status)
                    {
                        case CardStatus.ATTACK:
                            bp += data.attack;
                            bp += w_bp[0];
                            break;

                        case CardStatus.DEFENSE:
                            bp += data.defense;
                            bp += w_bp[1];
                            break;
                    }
                }
            }

            players[i].battle_point = bp;
        }
    }

    public void AddToUsedCard()
    {
        for (byte i = 0; i < players.Count; i++)
        {
            foreach (Card c in players[i].player_center_cards)
            {
                players[i].player_used_cards.Add(c);
            }
        }
    }

    public byte GetWinner()
    {
        if (players[0].draw || players[1].draw)
        {
            return byte.MaxValue;  
        }

        if (players[0].battle_point > players[1].battle_point)
        {
            players[0].score += 1;
            return 0;
        }
        else if (players[1].battle_point > players[0].battle_point)
        {
            players[0].score += 1;
            return 1;
        }
        else
        {
            //���ºν� ���ڸ� max������ �־��ش�.
            return byte.MaxValue;
        }
    }

    public void UsingCheatingCards(byte p, int i)
    {
        switch (i)
        {
            case 26:
                {
                    //����� ��� ġ�� ī�� �ı�
                    byte o_p = GetOtherPlayerIndex(p);
                    players[o_p].destroy_cheating_card = true;
                }
                break;

            case 27:
                {
                    //����� ��� ���� ī�� �ı�
                    byte o_p = GetOtherPlayerIndex(p);
                    players[o_p].destroy_weapon_card = true;
                }
                break;

            case 28:
                {
                    //������ ���� �տ� �ִ� ī�� 5���� �����ش�.

                }
                break;

            case 29:
                {
                    //�¸� Ƚ���� 2ȸ �߰��ϰ� ���� 2ȸ �����Ѵ�.
                    players[p].score += 2;
                    turn += 2;
                }
                break;

            case 30:
                {
                    //����� ī�� 1���� ���� �־��ش�.
                    int r = Random.Range(0, players[p].player_used_cards.Count);
                    Card card = players[p].player_used_cards[r];

                    players[p].player_used_cards.Remove(card);
                    players[p].player_deck_cards.Add(card);
                }
                break;

            case 31:
                {
                    //������ ���� �����.
                    players[p].draw = true;
                }
                break;

            case 32:
                {
                    //�ڽ��� ���� ��� 100�� ���
                    for (int k = 0; k < players[p].player_type_cards[CardType.CHARACTER].Count; k++)
                    {
                        players[p].battle_point += 100;
                    }
                }
                break;

            case 33:
                {
                    //����� ���� ��� 100�� ����
                    byte o_p = GetOtherPlayerIndex(p);
                    for (int k = 0; k < players[o_p].player_type_cards[CardType.CHARACTER].Count; k++)
                    {
                        players[o_p].battle_point -= 100;
                    }
                }
                break;

            case 39:
                {
                    //�� �ν�������
                    byte o_p = GetOtherPlayerIndex(p);
                    players[o_p].destroy_character_card = true;
                    players[o_p].destroy_cheating_card = true;
                    players[o_p].destroy_weapon_card = true;
                }
                break;
        }
    }

    public bool IsEnd()
    {
        if (turn >= 5)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    byte GetOtherPlayerIndex(byte p)
    {
        if (p == 0)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }
}
