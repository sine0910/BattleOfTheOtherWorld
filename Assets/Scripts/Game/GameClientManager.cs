using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameClientManager : MonoBehaviour
{
    public static GameClientManager instance;
    Queue<List<string>> waiting_packets;

    public Text other_player_info_txt;
    public Text total_score_txt;
    public GameObject ready;

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

    public GameObject show_five_cards;
    public SpriteRenderer[] five_cards_sprites;

    //플레이어가 여러개의 동작을 할 수 없도록 통제하는 변수
    int act_count = 2;
    bool is_can_act;

    void Start()
    {
        instance = this;

        MultiManager.instance.ServerInit();

        StartCoroutine(PaketHandler());

        SendFirstData();
    }

    void SendFirstData()
    {
        List<string> msg = new List<string>();
        msg.Add(((short)PROTOCOL.GAME_START).ToString());

        msg.Add(((short)START.FIRST).ToString());

        for (int i = 0; i < DataManager.instance.user_data.card_list.Count; i++)
        {
            msg.Add(DataManager.instance.user_data.card_list[i].ToString());
        }

        Send(msg);
    }

    public void Init()
    {
        ready.SetActive(true);

        foreach (CardUI c in my_hand_cards)
        {
            c.gameObject.SetActive(false);
        }

        foreach (CardUI c in my_center_cards)
        {
            c.gameObject.SetActive(false);
        }

        foreach (CardUI c in other_center_cards)
        {
            c.gameObject.SetActive(false);
        }

        my_hand_cards.Clear();
        my_center_cards.Clear();
        other_center_cards.Clear();
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
                        if (game_result.activeSelf)
                        {
                            game_result.SetActive(false);
                        }

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
                        SetScore(msg_list);

                        ShowCenterCards();

                        yield return new WaitForSeconds(1.5f);

                        DestroyCards();
                    }
                    break;

                case PROTOCOL.FINAL_RESULT:
                    {
                        DestroyCards();

                        int w = Convert.ToInt32(PopAt(msg_list));

                        game_result.SetActive(true);
                        if (w == player_index)
                        {
                            //승리시 점수 +25
                            result_image.sprite = result_sprites[0];
                            DataManager.instance.user_data.battle_point += 25;
                        }
                        else
                        {
                            //패배시 -25
                            result_image.sprite = result_sprites[1];
                            DataManager.instance.user_data.battle_point -= 25;
                        }

                        DataManager.instance.SaveUserData();
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

        //레디
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

        //카드를 방어 혹은 공격으로 둘 수 있는 팝업을 띄운다.
        card_popup.SetActive(true);
        card_popup_obj.transform.position = Camera.main.WorldToViewportPoint(card.transform.position);
    }

    public void GetPlayerChoose(int i)
    {
        if (!player_select_card)
            return;

        card_popup.SetActive(false);
        //i == 0 공격, i == 1 방어

        //플레이어가 행동한 것을 서버에 보낸다.
        List<string> msg = new List<string>();
        msg.Add(((short)PROTOCOL.PLAYER_ACK).ToString());

        msg.Add(player_select_card.card.card_index.ToString());
        msg.Add(i.ToString());

        Send(msg);
    }

    public void CancleSetCard()
    {
        is_can_act = true;

        player_select_card = null;
        card_popup.SetActive(false);
    }

    public void SetCenterCard(List<string>msg)
    {
        //플레이어 인덱스를 받는다.
        byte p = Convert.ToByte(PopAt(msg));

        //카드 인덱스를 받는다.
        int c = Convert.ToByte(PopAt(msg));
        CardUI card = FindCardAtHand(p, c);

        card.status = 1;

        //위치시킬 슬롯 인덱스를 받는다.
        byte slot = Convert.ToByte(PopAt(msg));

        //카드를 알맞은 슬롯으로 이동
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

        //카드 상태에 맞게 로테이션.
        byte status = Convert.ToByte(PopAt(msg));
        if (status != 0)
        {
            card.RotateCard();
        }

        is_can_act = true;
    }

    public void ShowCenterCards()
    {
        for (int i = 0; i < other_center_cards.Count; i++)
        {
            other_center_cards[i].SetSprite(CardManager.instance.GetCardSprites(other_center_cards[i].card.card_index));
        }
    }

    public void DestroyCards()
    {
        for (int i = 0; i < my_center_cards.Count; i++)
        {
            my_hand_cards[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < my_center_cards.Count; i++)
        {
            my_center_cards[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < other_center_cards.Count; i++)
        {
            other_center_cards[i].gameObject.SetActive(false);
        }

        my_hand_cards.Clear();
        my_center_cards.Clear();
        other_center_cards.Clear();
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

    void SetScore(List<string> msg)
    {
        byte pi = Convert.ToByte(PopAt(msg));
        byte pi_s = Convert.ToByte(PopAt(msg));

        byte pit = Convert.ToByte(PopAt(msg));
        byte pit_s = Convert.ToByte(PopAt(msg));

        //자신의 스코어가 앞으로 보여지도록 한다.
        if (pi == player_index)
        {
            total_score_txt.text = string.Format("SCORE: {0} - {1}", pi_s, pit_s);
        }
        else
        {
            total_score_txt.text = string.Format("SCORE: {0} - {1}", pit_s, pi_s);
        }
    }

    public void ReGame()
    {
        if (!is_can_act)
        {
            return;
        }

        is_can_act = false;
        Init();

        SendFirstData();
    }

    public void ExitGame()
    {
        MultiManager.instance.Cancle();
        SceneManager.LoadScene("HomeScene");
    }

    public void OnReady()
    {
        ready.SetActive(false);
        PlayerReady();
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
