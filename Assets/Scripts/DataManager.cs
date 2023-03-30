using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager instance;

    public UserData user_data = new UserData();

    public void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);

        instance = this;

        LoadAccountData();
    }

    #region LOCAL DATA
    public void LoadAccountData()
    {
        if (File.Exists(Application.persistentDataPath + "/DataFile.data"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/DataFile.data", FileMode.OpenOrCreate);
            if (file != null && file.Length > 0)
            {
                Data data = (Data)bf.Deserialize(file);

                user_data.email = data.account;
            }
            file.Close();
        }
    }

    public void SaveAccountData(string account)
    {
        user_data.email = account;

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/DataFile.data");

        Data data = new Data();
        data.account = user_data.email;

        bf.Serialize(file, data);
        file.Close();
    }

    [System.Serializable]
    public class Data
    {
        public string account;
    }
    #endregion

    public void ApplyUserData(UserData d)
    {
        user_data = d;
    }

    public void SaveUserData()
    {
        FirebaseManager.instance.SaveData(null);
    }
}

public class UserData
{
    public string username;
    public string email;
    public string password;

    public int battle_point;

    public List<int> card_list = new List<int>();

    public UserData()
    {
    }

    public UserData(string u, string e, string p, int b = 1200)
    {
        username = u;
        email = e;
        password = p;
        battle_point = b;
    }
}
