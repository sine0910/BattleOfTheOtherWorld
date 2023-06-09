using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PROTOCOL : byte 
{
    READY_TO_START = 0, //C -> S
    GAME_START = 1, //S -> C

    PLAYER_REQ = 2, //C -> S
    PLAYER_ACK = 3, //S -> C

    TURN_END = 4, //C -> S
    TURN_RESULT = 5, //S -> C

    FINAL_RESULT = 6, //S -> C
}

public enum START : byte
{
    FIRST,
    START,
    RESTART,
}

public class GameManager : MonoBehaviour
{
    public Dictionary<byte, PROTOCOL> received_protocol = new Dictionary<byte, PROTOCOL>();

    public static GameManager instance;
    public GameEngine engine;

    public float game_time = 60f;

    private void Start()
    {
        if (MultiManager.instance.is_host)
        {
            instance = this;
            engine = new GameEngine();
        }
    }

    bool IsReceived(byte player_index, PROTOCOL protocol)
    {
        if (!received_protocol.ContainsKey(player_index))
        {
            return false;
        }
        return received_protocol[player_index] == protocol;
    }

    void CheckedProtocol(byte player_index, PROTOCOL protocol)
    {
        if (received_protocol.ContainsKey(player_index))
        {
            if (!received_protocol.ContainsValue(protocol))
            {
                return;
            }
            else
            {
                received_protocol.Remove(player_index);
            }
        }
        received_protocol.Add(player_index, protocol);
    }

    bool IsAllReceived(PROTOCOL protocol)
    {
        if (received_protocol.Count < engine.players.Count)
        {
            Debug.Log("all_received this.received_protocol.Count < this.players.Count");
            return false;
        }

        foreach (KeyValuePair<byte, PROTOCOL> kvp in received_protocol)
        {
            if (kvp.Value != protocol)
            {
                Debug.Log("kvp.Value != protocol");
                return false;
            }
        }

        ClearReceivedProtocol();
        return true;
    }

    void ClearReceivedProtocol()
    {
        received_protocol.Clear();
    }

    public void ReceivePecket(byte player_index, List<string> msg)
    {
        PROTOCOL protocol = (PROTOCOL)Convert.ToByte(PopAt(msg));

        if (IsReceived(player_index, protocol))
        {
            return;
        }

        CheckedProtocol(player_index, protocol);

        switch (protocol)
        {
            case PROTOCOL.READY_TO_START:
                {
                    Debug.Log("READY_TO_START");

                    START start = (START)Convert.ToByte(PopAt(msg));

                    switch (start)
                    {
                        case START.FIRST:
                            {
                                //처음 시작할 때 플레이어는 자신이 갖고있는 모든 카드 정보를 서버로 보내준다.
                                List<int> player_card_list = new List<int>();
                                for (int i = 0; i < msg.Count; i++)
                                {
                                    player_card_list.Add(Convert.ToInt32(PopAt(msg)));
                                }
                                //받은 카드 정보를 적용
                                engine.GetPlayerCardList(player_index, player_card_list);

                                if (IsAllReceived(PROTOCOL.READY_TO_START))
                                {
                                    GameStart();
                                }
                            }
                            break;

                        case START.START:
                            {
                                if (IsAllReceived(PROTOCOL.READY_TO_START))
                                {
                                    GameStart();
                                }
                            }
                            break;
                    }
                }
                break;

            case PROTOCOL.PLAYER_REQ:
                {
                    Debug.Log("PLAYER_ACK");

                    //플레이어가 카드를 내었을 때
                    //플레이어의 핸드 카드에서 -> 센터 카드로 이동
                    int card_index = Convert.ToInt32(PopAt(msg));

                    engine.SetToCenter(player_index, card_index);

                    //플레이어가 카드를 공격 또는 방어로 변환했을 때
                    //카드의 스테이터스를 공격 또는 방어로 변환
                    CardStatus change_status = (CardStatus)Convert.ToInt32(PopAt(msg));
                    engine.SetCardStatus(player_index, card_index, change_status);

                    SetToCenterACK(player_index, card_index, change_status);
                }
                break;

            case PROTOCOL.TURN_END:
                {
                    Debug.Log("TURN_END");
                    if (IsAllReceived(PROTOCOL.TURN_END))
                    {
                        //플레이어들의 최종 전투 포인트를 합산하여 비교
                        engine.GetPlayerBattlePoint();

                        engine.AddToUsedCard();

                        if (engine.IsEnd())
                        {
                            //전투 포인트가 높은 플레이어를 승리
                            byte win_player = engine.GetWinner();

                            GameEnd(win_player);

                            engine.Init();
                        }
                        else
                        {
                            GameTurnEnd();
                        }
                    }
                }
                break;
        }
    }

    void GameStart()
    {
        Debug.Log("GameStart");

        engine.GameStart();

        SendPlayerCardInfo();
    }

    void SendPlayerCardInfo()
    {
        for (int i = 0; i < engine.players.Count; i++)
        {
            List<string> msg = new List<string>();
            msg.Add(((short)PROTOCOL.GAME_START).ToString());

            //플레이어 인덱스
            msg.Add(i.ToString());

            //카드의 정보를 추가한다.
            for (int k = 0; k < engine.players[i].player_hand_cards.Count; k++)
            {
                msg.Add(engine.players[i].player_hand_cards[k].card_index.ToString());
            }

            Send(i, msg);
        }
    }

    void SetToCenterACK(byte player_index, int card_index, CardStatus change_status)
    {
        for (int i = 0; i < engine.players.Count; i++)
        {
            List<string> msg = new List<string>();
            msg.Add(((short)PROTOCOL.PLAYER_REQ).ToString());

            msg.Add(player_index.ToString());
            msg.Add(card_index.ToString());

            msg.Add(((short)change_status).ToString());

            Send(i, msg);
        }
    }

    //사용안함
    void SetChangeACK(byte player_index, int card_index)
    {
        for (int i = 0; i < engine.players.Count; i++)
        {
            List<string> msg = new List<string>();
            msg.Add(((short)PROTOCOL.PLAYER_REQ).ToString());

            msg.Add(player_index.ToString());
            msg.Add(card_index.ToString());

            Send(i, msg);
        }
    }

    void GameTurnEnd()
    {
        for (int i = 0; i < engine.players.Count; i++)
        {
            List<string> msg = new List<string>();
            msg.Add(((short)PROTOCOL.TURN_RESULT).ToString());

            msg.Add(0.ToString());
            msg.Add(engine.players[0].score.ToString());

            msg.Add(1.ToString());
            msg.Add(engine.players[1].score.ToString());

            Send(i, msg);
        }
    }

    void GameEnd(byte winner)
    {
        for (int i = 0; i < engine.players.Count; i++)
        {
            List<string> msg = new List<string>();
            msg.Add(((short)PROTOCOL.FINAL_RESULT).ToString());

            msg.Add(winner.ToString());

            Send(i, msg);
        }
    }

    void Send(int i, List<string> msg)
    {
        switch (i)
        {
            case 0:
                SendManager.SendToHost(msg);
                break;

            case 1:
                SendManager.SendToGuest(msg);
                break;
        }
    }

    public string PopAt(List<string> list)
    {
        string r = list[0];
        list.RemoveAt(0);
        return r;
    }
}
