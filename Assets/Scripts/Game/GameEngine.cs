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

        //게임에 필요한 덱 카드 15장 랜덤 생성
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
                players[i].player_hand_cards.Add(CardManager.instance.GetCard(players[i].player_deck_cards.Dequeue()));
                if (players[i].player_hand_cards.Find(x => CardManager.instance.GetCardType(x.card_index) != CardType.CHARACTER) == null)
                {
                    
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
    }

    //센터의 카드를 방어 또는 공격으로 변환
    public void SetCardStatus(byte player_index, int card_index, CardStatus status)
    {
        players[player_index].player_center_cards.Find(x => x.card_index == card_index).SetStatus(status);
    }

    public void GetPlayerBattlePoint()
    {
        for (int i = 0; i < players.Count; i++)
        {
            int bp = 0;

            //먼저 치팅 카드들의 기술을 사용한 결과 필요


            //이후 웨폰 카드 적용


            //캐릭터 카드 적용
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
