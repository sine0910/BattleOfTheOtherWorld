using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GachaManager : MonoBehaviour
{
    public static GachaManager instance;

    public SupplyBoxAni sba;
    public GameObject sba_obj;

    public bool t = false;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        DontDestroyOnLoad(transform.gameObject);
    }

    public IEnumerator OnGacha(List<int> r)
    {
        t = false;
        sba_obj.SetActive(true);
        yield return StartCoroutine(sba.ShowGacha(r));

        t = false;
        yield return new WaitUntil(() => t);
        sba_obj.SetActive(false);
    }

    public void Click()
    {
        t = true;
    }
}
