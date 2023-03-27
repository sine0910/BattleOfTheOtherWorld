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

    FINAL_RESULT = 6 //S -> C
}

public enum REQ_TYPE : byte
{
    CARD,
    CHANGE
}

public class GameManager : MonoBehaviour
{
    public Dictionary<byte, PROTOCOL> received_protocol = new Dictionary<byte, PROTOCOL>();

    public static GameManager instance;
    public GameEngine engine;

    public float game_time = 60f;

    bool is_first;

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

                    if (is_first)
                    {
                        List<int> player_card_list = new List<int>();
                        for (int i = 0; i < msg.Count; i++)
                        {
                            player_card_list.Add(Convert.ToInt32(PopAt(msg)));
                        }
                        engine.GetPlayerCardList(player_index, player_card_list);
                    }

                    if (IsAllReceived(PROTOCOL.READY_TO_START))
                    {
                        is_first = false;

                        GameStart();
                    }
                }
                break;

            case PROTOCOL.PLAYER_REQ:
                {
                    Debug.Log("PLAYER_ACK");
                    REQ_TYPE t = (REQ_TYPE)Convert.ToByte(PopAt(msg));

                    switch (t)
                    {
                        case REQ_TYPE.CARD:
                            {
                                //�÷��̾ ī�带 ������ ��
                                //�÷��̾��� �ڵ� ī�忡�� -> ���� ī��� �̵�
                                int card_index = Convert.ToInt32(PopAt(msg));

                                engine.SetToCenter(player_index, card_index);

                                SetToCenterACK(player_index, card_index);
                            }
                            break;

                        case REQ_TYPE.CHANGE:
                            {
                                //�÷��̾ ī�带 ���� �Ǵ� ���� ��ȯ���� ��
                                //ī���� �������ͽ��� ���� �Ǵ� ���� ��ȯ
                                int card_index = Convert.ToInt32(PopAt(msg));
                                CardStatus change_status = (CardStatus)Convert.ToInt32(PopAt(msg));
                                engine.SetCardStatus(player_index, card_index, change_status);

                                SetChangeACK(player_index, card_index);
                            }
                            break;
                    }
                }
                break;

            case PROTOCOL.TURN_END:
                {
                    Debug.Log("TURN_END");
                    if (IsAllReceived(PROTOCOL.TURN_END))
                    {
                        //�÷��̾���� ���� ���� ����Ʈ�� �ջ��Ͽ� ��
                        engine.GetPlayerBattlePoint();
                        //���� ����Ʈ�� ���� �÷��̾ �¸�
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

            //�÷��̾� �ε���
            msg.Add(i.ToString());

            //ī�� 3���� ������ �߰��Ѵ�.
            for (int k = 0; k < engine.players[i].player_hand_cards.Count; k++)
            {
                msg.Add(engine.players[i].player_hand_cards[k].card_index.ToString());
            }

            Send(i, msg);
        }
    }

    void SetToCenterACK(byte player_index, int card_index)
    {
        for (int i = 0; i < engine.players.Count; i++)
        {
            List<string> msg = new List<string>();
            msg.Add(((short)PROTOCOL.PLAYER_REQ).ToString());

            msg.Add(((short)REQ_TYPE.CARD).ToString());

            msg.Add(player_index.ToString());
            msg.Add(card_index.ToString());

            Send(i, msg);
        }
    }

    void SetChangeACK(byte player_index, int card_index)
    {
        for (int i = 0; i < engine.players.Count; i++)
        {
            List<string> msg = new List<string>();
            msg.Add(((short)PROTOCOL.PLAYER_REQ).ToString());

            msg.Add(((short)REQ_TYPE.CARD).ToString());

            msg.Add(player_index.ToString());
            msg.Add(card_index.ToString());

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
