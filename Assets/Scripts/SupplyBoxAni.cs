using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SupplyBoxAni : MonoBehaviour
{
    public List<Vector3> card_slots = new List<Vector3>();
    public List<GameObject> cards = new List<GameObject>();

    bool on_touch = false;

    public void Init(List<int> card_results)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].gameObject.SetActive(false);
        }

        StartCoroutine(ShowGacha(card_results));
    }

    IEnumerator ShowGacha(List<int> card_results)
    {
        for (int i = 0; i < card_results.Count; i++)
        {
            cards[i].GetComponent<SpriteRenderer>().sprite = CardManager.instance.GetCardSprites(card_results[i]);
            cards[i].gameObject.SetActive(true);

            cards[i].transform.position = new Vector3(card_slots[i].x, card_slots[i].y);
        }

        yield return 0;
    }
}
