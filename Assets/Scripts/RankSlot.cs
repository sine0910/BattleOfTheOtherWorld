using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RankSlot : MonoBehaviour
{
    public RectTransform rect;

    public Image image;
    public Text name_txt;
    public Text bp_txt;

    public void Init(bool is_me, int c, string n, string b)
    {
        if (is_me)
        {
            image.color = Color.blue;
            rect.sizeDelta = new Vector2(1600, 70);
        }
        else
        {
            image.color = Color.black;
            rect.sizeDelta = new Vector2(1600, 70);
        }

        name_txt.text = c + ". " + n;
        bp_txt.text = b;
    }
}
