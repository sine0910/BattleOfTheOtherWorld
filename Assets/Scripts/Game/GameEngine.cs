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

        //게임에 필요한 덱 카드 15장 랜덤 생성
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

        //카드중 3장을 랜덤적으로 플레이어에게 나누어 준다.
        GenerateCards();
    }

    //카드 생성 함수
    public void GenerateCards()
    {
        for (int i = 0; i < players.Count; i++)
        {
            //생성한 덱 카드에서 핸드 카드 3장을 랜덤으로 추출
            for (int k = 0; k < 3; k++)
            {
                Card c = players[i].player_deck_cards[k];

                Card cc = players[i].player_hand_cards.Find(x => CardManager.instance.GetCardType(x.card_index) == CardType.CHARACTER);
                //캐릭터 카드가 필수적으로 나오도록 캐릭터 카드가 나올 때 까지 되돌린다.
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
    
    //카드를 섞기 위한 함수
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

    //카드를 핸드에서 센터로 옮기는 함수
    public void SetToCenter(byte player_index, int card_index)
    {
        Card card = players[player_index].player_hand_cards.Find(x => x.card_index == card_index);

        players[player_index].player_hand_cards.Remove(card);
        players[player_index].player_center_cards.Add(card);

        players[player_index].player_type_cards[CardManager.instance.GetCardType(card_index)].Add(card);
    }

    //센터의 카드를 방어 또는 공격으로 변환
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
            //치팅 카드를 파괴하는 26번은 예외적으로 먼저 처리한다.
            if (players[i].player_type_cards[CardType.CHEATING].Find(x => x.card_index == 26) != null)
            {
                UsingCheatingCards(i, 26);
            }
            //동일하게 웨폰 카드를 파괴하는 27번도 먼저 처리한다.
            if (players[i].player_type_cards[CardType.CHEATING].Find(x => x.card_index == 27) != null)
            {
                UsingCheatingCards(i, 27);
            }
            //동일하게 모든 카드를 파괴하는 39번도 먼저 처리한다.
            if (players[i].player_type_cards[CardType.CHEATING].Find(x => x.card_index == 39) != null)
            {
                UsingCheatingCards(i, 39);
            }
        }

        //먼저 치팅 카드들의 기술을 사용한 결과 필요
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

        //이후 웨폰 카드 적용
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

        //카드들 적용
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
            //무승부시 승자를 max벨류로 넣어준다.
            return byte.MaxValue;
        }
    }

    public void UsingCheatingCards(byte p, int i)
    {
        switch (i)
        {
            case 26:
                {
                    //상대의 모든 치팅 카드 파괴
                    byte o_p = GetOtherPlayerIndex(p);
                    players[o_p].destroy_cheating_card = true;
                }
                break;

            case 27:
                {
                    //상대의 모든 웨폰 카드 파괴
                    byte o_p = GetOtherPlayerIndex(p);
                    players[o_p].destroy_weapon_card = true;
                }
                break;

            case 28:
                {
                    //상대방의 덱과 손에 있는 카드 5장을 보여준다.

                }
                break;

            case 29:
                {
                    //승리 횟수를 2회 추가하고 턴을 2회 종료한다.
                    players[p].score += 2;
                    turn += 2;
                }
                break;

            case 30:
                {
                    //사용한 카드 1장을 덱에 넣어준다.
                    int r = Random.Range(0, players[p].player_used_cards.Count);
                    Card card = players[p].player_used_cards[r];

                    players[p].player_used_cards.Remove(card);
                    players[p].player_deck_cards.Add(card);
                }
                break;

            case 31:
                {
                    //강제로 비기게 만든다.
                    players[p].draw = true;
                }
                break;

            case 32:
                {
                    //자신의 공격 방어 100씩 상승
                    for (int k = 0; k < players[p].player_type_cards[CardType.CHARACTER].Count; k++)
                    {
                        players[p].battle_point += 100;
                    }
                }
                break;

            case 33:
                {
                    //상대의 공격 방어 100씩 깍음
                    byte o_p = GetOtherPlayerIndex(p);
                    for (int k = 0; k < players[o_p].player_type_cards[CardType.CHARACTER].Count; k++)
                    {
                        players[o_p].battle_point -= 100;
                    }
                }
                break;

            case 39:
                {
                    //다 부숴버려랑
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
