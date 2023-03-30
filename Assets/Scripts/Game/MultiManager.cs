using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Database;

public class MultiManager : MonoBehaviour
{
    public static MultiManager instance;

    public bool is_host = false;
    public string root = "";
    public string other_user_name = "";

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;

        DontDestroyOnLoad(this.gameObject);
    }

    public void MoveGameScene()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void InitData()
    {
        is_host = false;
        root = "";
        other_user_name = "";
    }

    public void ServerInit()
    {
        if (is_host)
        {
            FirebaseManager.instance.db_ref.Child("Multi").Child(root).Child("Server").ValueChanged += GetValueChangedForServer;
        }
        else
        {
            FirebaseManager.instance.db_ref.Child("Multi").Child(root).Child("Guest").ValueChanged += GetValueChangedForServer;
        }
    }

    public void ProtocolToServer(string msg)//호스트에게 보내는 메세지
    {
        FirebaseManager.instance.db_ref.Child("Multi").Child(root).Child("Server").SetValueAsync(msg);
    }

    public void ProtocolToGuest(string msg)//게스트에게 보내는 메세지
    {
        FirebaseManager.instance.db_ref.Child("Multi").Child(root).Child("Guest").SetValueAsync(msg);
    }

    void GetValueChangedForServer(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            if (args.Snapshot.Exists)
            {
                string value = args.Snapshot.Value.ToString();

                List<string> msg = value.Split(new string[] { "/" }, StringSplitOptions.None).ToList();

                byte player_index = Convert.ToByte(PopAtLast(msg));

                SendManager.ReceiveServer(player_index, msg);

                return;
            }
        }
    }

    void GetValueChangedForGuest(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            if (args.Snapshot.Exists)
            {
                string value = args.Snapshot.Value.ToString();

                List<string> msg = value.Split(new string[] { "/" }, StringSplitOptions.None).ToList();

                SendManager.ReceiveClient(msg);

                return;
            }
        }
    }

    public void Cancle()
    {
        if (is_host)
        {
            FirebaseManager.instance.db_ref.Child("Multi").Child(root).Child("Server").ValueChanged -= GetValueChangedForServer;
        }
        else
        {
            FirebaseManager.instance.db_ref.Child("Multi").Child(root).Child("Guest").ValueChanged -= GetValueChangedForServer;
        }
    }

    string PopAtLast(List<string> list)
    {
        string r = list[(list.Count - 1)];
        list.RemoveAt(list.Count - 1);
        return r;
    }
}