using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card
{
    public CardStatus status;
    public CardType type;
    public int card_index;

    public Card(int i)
    {
        card_index = i;
    }

    public void SetType(CardType t)
    {
        type = t;
    }

    public void SetStatus(CardStatus s)
    {
        status = s;
    }
}

public class CardUI : MonoBehaviour
{
    /// <summary>
    /// 자신의 덱에 있을 시 0
    /// 센터에 있을 시 1
    /// </summary>
    public byte status;

    public byte dreg_status;

    public Card card;

    public SpriteRenderer sprite;

    void Start()
    {
        card = new Card(0);
    }

    // Start is called before the first frame update
    public void Init(int i)
    {
        card.card_index = i;
    }

    public void SetSprite(Sprite s)
    {
        sprite.sprite = s;
    }

    private void OnMouseDown()
    {
        switch (status)
        {
            case 0:
                {
                    dreg_status = 1;
                }
                break;

            default:

                break;
        }
    }

    public void OnMouseDrag()
    {
        switch (dreg_status)
        {
            case 1:
                {
                    transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                }
                break;
        }
    }

    public void OnMouseUp()
    {
        switch (dreg_status)
        {
            case 1:
                {
                    dreg_status = 0;
                    GameClientManager.instance.SetCard(this);
                }
                break;

            default:

                break;
        }
    }

    public void RotateCard()
    {
        if (transform.rotation.x != 90)
        {
            transform.localRotation = new Quaternion(0, 0, 90, 0);
        }
        else
        {
            transform.localRotation = new Quaternion(0, 0, 0, 0);
        }
    }
}
