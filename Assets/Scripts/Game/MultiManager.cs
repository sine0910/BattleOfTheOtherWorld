using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiManager : MonoBehaviour
{
    public static MultiManager instance;

    public bool is_host = false;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        DontDestroyOnLoad(this.gameObject);
    }

    public void Init()
    {

    }

    public void ProtocolToServer(string msg)//ȣ��Ʈ���� ������ �޼���
    {

    }

    public void ProtocolToGuest(string msg)//�Խ�Ʈ���� ������ �޼���
    {

    }

    void GetValueChangedForServer()
    {

    }
}