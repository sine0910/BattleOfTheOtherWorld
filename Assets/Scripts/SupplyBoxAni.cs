using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SupplyBoxAni : MonoBehaviour
{
    public List<Transform> card_slots = new List<Transform>();
    public List<GameObject> cards = new List<GameObject>();

    public IEnumerator ShowGacha(List<int> card_results)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < card_results.Count; i++)
        {
            cards[i].gameObject.SetActive(true);
            cards[i].GetComponent<Image>().sprite = CardManager.instance.GetCardSprites(card_results[i]);

            cards[i].transform.position = new Vector3(card_slots[i].position.x, card_slots[i].position.y);

            if (!GachaManager.instance.t)
            {
                yield return new WaitForSeconds(1f);
            }
        }

        yield return 0;
    }

    public void Destroy()
    {
        gameObject.SetActive(false);
    }
}
