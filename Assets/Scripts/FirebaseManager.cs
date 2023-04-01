using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Analytics;
using Firebase.Database;
using Firebase.Extensions;
using System.Linq;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager instance;

    public DatabaseReference db_ref;
    FirebaseUser user;

    public delegate void Callback(byte success);

    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);

        instance = this;

        db_ref = FirebaseDatabase.DefaultInstance.RootReference;

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(checkTask =>
        {
            var dependencyStatus = checkTask.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                Auth();
            }
            else
            {
                Debug.LogError(string.Format("Could not resolve all Firebase dependencies: {0}", dependencyStatus));
            }
        });
    }

    void Auth()
    {
        Debug.Log("Auth");

        FirebaseAuth auth = FirebaseAuth.DefaultInstance;
        auth.SignInAnonymouslyAsync().ContinueWith(task => 
        {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInAnonymouslyAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInAnonymouslyAsync encountered an error: " + task.Exception);
                return;
            }

            user = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})", user.DisplayName, user.UserId);
        });
    }

    public void CreateAccount(string n, string e, string p, Callback c)
    {
        DataManager.instance.SaveAccountData(e);

        UserData data = new UserData(n, e, p, 1200);

        DataManager.instance.user_data = data;

        Dictionary<string, object> childUpdates = new Dictionary<string, object>();
        childUpdates["/Users/" + e + "/username"] = data.username;
        childUpdates["/Users/" + e + "/email"] = data.email;
        childUpdates["/Users/" + e + "/password"] = data.password;
        childUpdates["/Users/" + e + "/battle_point"] = data.battle_point;
        childUpdates["/Users/" + e + "/card_list"] = data.card_list;

        db_ref.UpdateChildrenAsync(childUpdates).ContinueWithOnMainThread(t => 
        {
            if (t.IsFaulted)
            {
                Debug.Log("Faulted CreateAccount" + t.Exception);
                c(1);
            }
            else if (t.IsCompletedSuccessfully)
            {
                c(0);
            }
        });
    }

    public void CreateGuestAccount(Callback c)
    {
        string key = db_ref.Child("Users").Push().Key;

        DataManager.instance.SaveAccountData(key);

        UserData data = new UserData("user_" + key.Substring(1, key.Length < 4 ? key.Length : 4), key, "", 1200);

        DataManager.instance.user_data = data;

        Dictionary<string, object> childUpdates = new Dictionary<string, object>();
        childUpdates["/Users/" + key + "/username"] = data.username;
        childUpdates["/Users/" + key + "/email"] = data.email;
        childUpdates["/Users/" + key + "/password"] = data.password;
        childUpdates["/Users/" + key + "/battle_point"] = data.battle_point;
        childUpdates["/Users/" + key + "/card_list"] = data.card_list;


        db_ref.UpdateChildrenAsync(childUpdates).ContinueWithOnMainThread(t =>
        {
            if (t.IsFaulted)
            {
                c(1);
            }
            else if (t.IsCompleted)
            {
                c(0);
            }
        });
    }

    public void Login(string e, string p, Callback c)
    {
        FirebaseDatabase.DefaultInstance.GetReference("Users").Child(e).GetValueAsync().ContinueWithOnMainThread(t =>
        {
            if (t.IsFaulted)
            {
                c(1);
            }
            else if (t.IsCompleted)
            {
                DataSnapshot snapshot = t.Result;

                if (snapshot.Exists)
                {
                    UserData data = new UserData();

                    data.username = snapshot.Child("username").Value.ToString();
                    data.email = snapshot.Child("email").Value.ToString();
                    data.password = snapshot.Child("password").Value.ToString();
                    data.battle_point = Convert.ToInt32(snapshot.Child("battle_point").Value);

                    if (snapshot.HasChild("card_list"))
                    {
                        List<int> l = new List<int>();
                        foreach (DataSnapshot obj in snapshot.Child("card_list").Children)
                        {
                            l.Add(Convert.ToInt32(obj.Value));
                        }

                        data.card_list = l;
                    }

                    DataManager.instance.SaveAccountData(e);
                    DataManager.instance.ApplyUserData(data);

                    c(0);
                }
            }
        });
    }

    public void AutoLogin(Callback c)
    {
        FirebaseDatabase.DefaultInstance.GetReference("Users").Child(DataManager.instance.user_data.email).GetValueAsync().ContinueWithOnMainThread(t =>
        {
            if (t.IsFaulted)
            {
                c(1);
            }
            else if (t.IsCompleted)
            {
                DataSnapshot snapshot = t.Result;

                if (snapshot.Exists)
                {
                    UserData data = new UserData();

                    data.username = snapshot.Child("username").Value.ToString();
                    data.email = snapshot.Child("email").Value.ToString();
                    data.password = snapshot.Child("password").Value.ToString();
                    data.battle_point = Convert.ToInt32(snapshot.Child("battle_point").Value);

                    List<int> l = new List<int>();
                    foreach (DataSnapshot obj in snapshot.Child("card_list").Children)
                    {
                        l.Add(Convert.ToInt32(obj.Value));
                    }

                    data.card_list = l;

                    DataManager.instance.ApplyUserData(data);

                    c(0);
                }
            }
        });
    }

    public void SaveData(Callback c)
    {
        Dictionary<string, object> childUpdates = new Dictionary<string, object>();
        childUpdates["/Users/" + DataManager.instance.user_data.email + "/username"] = DataManager.instance.user_data.username;
        childUpdates["/Users/" + DataManager.instance.user_data.email + "/email"] = DataManager.instance.user_data.email;
        childUpdates["/Users/" + DataManager.instance.user_data.email + "/password"] = DataManager.instance.user_data.password;
        childUpdates["/Users/" + DataManager.instance.user_data.email + "/battle_point"] = DataManager.instance.user_data.battle_point;
        childUpdates["/Users/" + DataManager.instance.user_data.email + "/card_list"] = DataManager.instance.user_data.card_list;

        if (c != null)
        {
            db_ref.UpdateChildrenAsync(childUpdates).ContinueWithOnMainThread(t =>
            {
                if (t.IsFaulted)
                {
                    c(1);
                }
                else if (t.IsCompleted)
                {
                    c(0);
                }
            });
        }
        else
        {
            db_ref.UpdateChildrenAsync(childUpdates);
        }
    }

    public delegate void PwCallback(string result);
    public void GetPassword(string e, PwCallback c)
    {
        FirebaseDatabase.DefaultInstance.GetReference("Users").Child(e).GetValueAsync().ContinueWithOnMainThread(t =>
        {
            if (t.IsFaulted)
            {
                c("");
            }
            else if (t.IsCompleted)
            {
                DataSnapshot snapshot = t.Result;

                if (snapshot.Exists)
                {
                    c(snapshot.Child("password").Value.ToString());
                }
            }
        });
    }

    public delegate void TimeCallback(long result);
    public void GetWhenOpenFreeBox(TimeCallback c)
    {
        FirebaseDatabase.DefaultInstance.GetReference("Users").Child(DataManager.instance.user_data.email).GetValueAsync().ContinueWithOnMainThread(t =>
        {
            if (t.IsFaulted)
            {
                c(0);
            }
            else if (t.IsCompleted)
            {
                DataSnapshot snapshot = t.Result;

                if (snapshot.Exists && snapshot.HasChild("freebox"))
                {
                    c(Convert.ToInt64(snapshot.Child("freebox").Value));
                }
                else
                {
                    c(0);
                }
            }
        });
    }

    public void UpdateWhenOpenFreeBox(Callback c)
    {
        FirebaseDatabase.DefaultInstance.GetReference("Users").Child(DataManager.instance.user_data.email).Child("freebox").SetValueAsync(GetUnixTimeStamp()).ContinueWithOnMainThread(t =>
        {
            if (t.IsFaulted)
            {
                c(1);
            }
            else if (t.IsCompleted)
            {
                c(0);
            }
        });
    }

    public void InviteFriend(string name, Callback c)
    {
        FirebaseDatabase.DefaultInstance.GetReference("Users").OrderByChild("username").EqualTo(name).GetValueAsync().ContinueWithOnMainThread(t =>
        {
            if (t.IsFaulted)
            {
                c(1);
            }
            else if (t.IsCompleted)
            {
                DataSnapshot snapshot = t.Result;

                if (snapshot.Exists)
                {
                    foreach (DataSnapshot data in snapshot.Children)
                    {
                        MultiManager.instance.is_host = true;
                        MultiManager.instance.root = data.Child("email").Value.ToString();
                        MultiManager.instance.other_user_name = data.Child("username").Value.ToString();

                        c(0);
                    }
                }
            }
        });
    }

    public void AcceptInvite()
    {
        db_ref.Child("Match").Child(MultiManager.instance.root).Child("msg").SetValueAsync("3");
    }

    public delegate void RankingCallback(Dictionary<byte, List<string>> result);
    public void GetRanking(RankingCallback r)
    {
        FirebaseDatabase.DefaultInstance.GetReference("Users").OrderByChild("battle_point").LimitToLast(18).GetValueAsync().ContinueWithOnMainThread(t =>
        {
            if (t.IsFaulted)
            {
                Dictionary<byte, List<string>> result = new Dictionary<byte, List<string>>();
                r(result);
            }
            else if (t.IsCompleted)
            {
                DataSnapshot snapshot = t.Result;

                Dictionary<byte, List<string>> result = new Dictionary<byte, List<string>>();
                if (snapshot.Exists)
                {
                    result.Add(0, new List<string>());
                    result.Add(1, new List<string>());

                    foreach (DataSnapshot data in snapshot.Children)
                    {
                        result[0].Add(data.Child("username").Value.ToString());
                        result[1].Add(data.Child("battle_point").Value.ToString());
                    }
                }

                r(result);
            }
        });
    }

    public void StartGame()
    {
        MultiManager.instance.InitData();

        FirebaseDatabase.DefaultInstance.GetReference("Match").OrderByChild("time").StartAt(1).LimitToFirst(1).GetValueAsync().ContinueWithOnMainThread(t =>
        {
            if (t.IsFaulted)
            {

            }
            else if (t.IsCompleted)
            {
                if (!MultiManager.instance.is_host && MultiManager.instance.root == "" && MultiManager.instance.other_user_name == "")
                {
                    DataSnapshot snapshot = t.Result;

                    //먼저 대전 신청을 한 사람이 있을 경우 그 사람 방으로 입장
                    if (snapshot.Exists)
                    {
                        foreach (DataSnapshot data in snapshot.Children)
                        {
                            MultiManager.instance.is_host = false;
                            MultiManager.instance.root = data.Child("email").Value.ToString();
                            MultiManager.instance.other_user_name = data.Child("user_name").Value.ToString();

                            db_ref.Child("Match").Child(MultiManager.instance.root).Child("time").RemoveValueAsync();

                            db_ref.Child("Match").Child(MultiManager.instance.root).Child("msg").SetValueAsync("1/" + DataManager.instance.user_data.username);

                            ListenMatchStatus();
                        }
                    }
                    else//대전 신청한 사람이 없는 경우 자신이 호스트로 방을 생성한다.
                    {
                        MakeGameRoom();
                    }
                }
            }
        });
    }

    public void MakeGameRoom()
    {
        MultiManager.instance.is_host = true;
        MultiManager.instance.root = DataManager.instance.user_data.email;

        Dictionary<string, object> childUpdates = new Dictionary<string, object>();
        childUpdates["Match/" + MultiManager.instance.root + "/time"] = GetUnixTimeStamp();
        childUpdates["Match/" + MultiManager.instance.root + "/email"] = DataManager.instance.user_data.email;
        childUpdates["Match/" + MultiManager.instance.root + "/user_name"] = DataManager.instance.user_data.username;
        childUpdates["Match/" + MultiManager.instance.root + "/msg"] = "";

        db_ref.UpdateChildrenAsync(childUpdates);

        ListenMatchStatus();
    }

    public void CloseGameRoom()
    {
        db_ref.Child("Match").Child(MultiManager.instance.root).RemoveValueAsync();

        MultiManager.instance.InitData();
    }

    public void ListenMatchStatus()
    {
        Debug.Log("ListenMatchStatus root => " + MultiManager.instance.root);
        db_ref.Child("Match").Child(MultiManager.instance.root).Child("msg").ValueChanged += GetValueChangedForMatch;
    }

    void GetValueChangedForMatch(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            return;
        }

        if (args.Snapshot.Exists)
        {
            string value = args.Snapshot.Value.ToString();
            List<string> msg = value.Split(new string[] { "/" }, StringSplitOptions.None).ToList();

            if (msg[0] == "1" && MultiManager.instance.is_host)
            {
                MultiManager.instance.other_user_name = msg[1];

                db_ref.Child("Match").Child(MultiManager.instance.root).Child("msg").SetValueAsync("3");
            }
            else if (msg[0] == "2")
            {
                MultiManager.instance.other_user_name = msg[1];
                HomeManager.instance.RecieveInvite();
            }
            else if (msg[0] == "3")
            {
                //
                if (MultiManager.instance.is_host)
                {
                    db_ref.Child("Match").Child(MultiManager.instance.root).RemoveValueAsync();
                }
                CancleMatch();
                MultiManager.instance.MoveGameScene();
            }
        }
    }

    public void CancleMatch()
    {
        db_ref.Child("Match").Child(MultiManager.instance.root).Child("msg").ValueChanged -= GetValueChangedForMatch;
    }

    public static long GetUnixTimeStamp()
    {
        DateTime dt = DateTime.Now;
        return ((DateTimeOffset)dt).ToUnixTimeSeconds();
    }

    public DateTime UnixTimeStampToUnixDateTime(long value)
    {
        DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dt = dt.AddSeconds(value);
        return dt;
    }
}
