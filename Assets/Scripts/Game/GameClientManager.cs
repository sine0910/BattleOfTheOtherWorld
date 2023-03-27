using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameClientManager : MonoBehaviour
{
    public static GameClientManager instance;
    Queue<List<string>> waiting_packets;

    public Text other_player_info_txt;
    public Text total_score_txt;

    public GameObject game_result;
    public Image result_image;
    public Sprite[] result_sprites;

    public List<Transform> my_hand_slot;
    public List<CardUI> my_hand_cards;
    public List<Transform> my_center_slot;
    public List<CardUI> my_center_cards;

    public List<Transform> other_center_slot;
    public List<CardUI> other_center_cards;

    byte player_index = byte.MaxValue;

    public GameObject card_ui;
    public List<CardUI> card_pool = new List<CardUI>();

    public GameObject card_popup;
    public GameObject card_popup_obj;

    //�÷��̾ �������� ������ �� �� ������ �����ϴ� ����
    int act_count = 2;
    bool is_can_act;

    void Start()
    {
        instance = this;

        StartCoroutine(PaketHandler());
    }

    public void ReceivePacket(List<string> msg)
    {
        waiting_packets.Enqueue(msg);
    }

    IEnumerator PaketHandler()
    {
        while (true)
        {
            if (waiting_packets.Count <= 0)
            {
                yield return 0;
                continue;
            }

            List<string> msg_list = waiting_packets.Dequeue();
            PROTOCOL protocol = (PROTOCOL)Convert.ToInt32(PopAt(msg_list));

            switch (protocol)
            {
                case PROTOCOL.GAME_START:
                    {
                        byte i = Convert.ToByte(PopAt(msg_list));
                        if (player_index == byte.MaxValue)
                        {
                            player_index = i;
                        }

                        for (int k = 0; k < 3; k++)
                        {
                            int c = Convert.ToInt32(PopAt(msg_list));
                            CardUI card = MakeHandCard(c);

                            card.transform.position = my_hand_slot[i].position;
                            my_hand_cards.Add(card);
                        }

                        StartCoroutine(Timer());
                    }
                    break;

                case PROTOCOL.PLAYER_REQ:
                    {
                        SetCenterCard(msg_list);
                    }
                    break;

                case PROTOCOL.TURN_RESULT:
                    {
                        
                    }
                    break;

                case PROTOCOL.FINAL_RESULT:
                    {

                    }
                    break;
            }
        }
    }

    public CardUI MakeHandCard(int c)
    {
        CardUI card = GetCardUI();
        card.card = CardManager.instance.GetCard(c);
        card.SetSprite(CardManager.instance.GetCardSprites(c));

        return card;
    }

    bool timer = true;
    IEnumerator Timer()
    {
        float t = 0;
        timer = true;
        act_count = 2;

        while (timer)
        {
            t += Time.fixedDeltaTime;
            if (t > 60)
            {
                break;
            }
            yield return new WaitForFixedUpdate();    
        }

        //����
        PlayerReady();
    }

    void PlayerReady()
    {
        if (!is_can_act)
        {
            return;
        }

        is_can_act = false;
        act_count = 0;

        timer = false;

        List<string> msg = new List<string>();
        msg.Add(((short)PROTOCOL.TURN_END).ToString());

        Send(msg);
    }

    CardUI player_select_card;
    public void SetCard(CardUI card)
    {
        if (!is_can_act || act_count < 1)
        {
            return;
        }

        is_can_act = false;
        act_count--;

        player_select_card = card;

        //ī�带 ��� Ȥ�� �������� �� �� �ִ� �˾��� ����.
        card_popup.SetActive(true);
        card_popup_obj.transform.position = Camera.main.WorldToViewportPoint(card.transform.position);
    }

    public void GetPlayerChoose(int i)
    {
        card_popup.SetActive(false);
        //i == 0 ����, i == 1 ���

        //�÷��̾ �ൿ�� ���� ������ ������.
    }

    public void CancleSetCard()
    {
        is_can_act = true;

        player_select_card = null;
        card_popup.SetActive(false);
    }

    public void SetCenterCard(List<string>msg)
    {
        //�÷��̾� �ε����� �޴´�.
        byte p = Convert.ToByte(PopAt(msg));

        //ī�� �ε����� �޴´�.
        int c = Convert.ToByte(PopAt(msg));
        CardUI card = FindCardAtHand(p, c);

        card.status = 1;

        //��ġ��ų ���� �ε����� �޴´�.
        byte slot = Convert.ToByte(PopAt(msg));

        //ī�带 �˸��� �������� �̵�
        if (p == player_index)
        {
            card.transform.position = my_center_slot[slot].position;
            my_center_cards.Add(card);
        }
        else
        {
            card.transform.position = other_center_slot[slot].position;
            other_center_cards.Add(card);
        }

        //ī�� ���¿� �°� �����̼�.
        byte status = Convert.ToByte(PopAt(msg));
        if (status != 0)
        {
            card.RotateCard();
        }

        is_can_act = true;
    }

    CardUI FindCardAtHand(int p, int c)
    {
        CardUI card = null;

        if (p == player_index)
        {
            card = my_hand_cards.Find(x => x.card.card_index == c);
            my_hand_cards.Remove(card);
        }
        else
        {
            card = GetCardUI();
            card.card = CardManager.instance.GetCard(c);
            card.SetSprite(CardManager.instance.GetCardBackSprites(0));
        }

        return card;
    }

    CardUI FindCardAtCenter(int p, int c)
    {
        CardUI card = null;

        if(p == player_index)
        {
            card = my_center_cards.Find(x => x.card.card_index == c);
        }
        else
        {
            card = other_center_cards.Find(x => x.card.card_index == c);
        }

        return card;
    }

    public void ReGame()
    {
        
    }

    public void ExitGame()
    {

    }

    public void OnReady()
    {

    }

    public void Send(List<string> msg)
    {
        SendManager.SendToServer(player_index, msg);
    }

    public CardUI GetCardUI()
    {
        CardUI c = null;

        foreach (CardUI i in card_pool)
        {
            if (!i.gameObject.activeSelf)
            {
                c = i;
                c.gameObject.SetActive(true);

                break;
            }
        }

        if (!c)
        {
            c = Instantiate(card_ui, transform).GetComponent<CardUI>();
            card_pool.Add(c);
        }

        return c;
    }

    public string PopAt(List<string> list)
    {
        string r = list[0];
        list.RemoveAt(0);
        return r;
    }
}
