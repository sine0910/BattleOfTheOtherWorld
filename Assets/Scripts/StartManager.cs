using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartManager : MonoBehaviour
{
    public GameObject main;
    public GameObject createAccount;
    public GameObject loginAccount;
    public GameObject findPassword;

    public InputField usernameInput;
    public InputField emailInput;
    public InputField passwordInput;
    public InputField repeatepwInput;

    bool isWrongWithPw;

    public InputField login_emailInput;
    public InputField login_passwordInput;

    public InputField find_emailInput;
    public Text find_pw_txt;

    public bool is_can_act = true;

    private void Awake()
    {
        main.SetActive(true);
        loginAccount.SetActive(false);
        createAccount.SetActive(false);
        findPassword.SetActive(false);
    }

    private void Start()
    {
        if (DataManager.instance.user_data.email != null && DataManager.instance.user_data.email != "")
        {
            FirebaseManager.instance.AutoLogin(Callback);
        }
    }

    public void ShowCreateAccountPage()
    {
        main.SetActive(false);
        createAccount.SetActive(true);

        isWrongWithPw = false;
        usernameInput.text = "";
        emailInput.text = "";
        passwordInput.text = "";
        repeatepwInput.text = "";
    }

    public void CloseCreateAccountPage()
    {
        main.SetActive(true);
        createAccount.SetActive(false);
    }

    public void CheckRepeatePassword()
    {
        if (passwordInput.text != repeatepwInput.text)
        {
            isWrongWithPw = true;
        }
        else
        {
            isWrongWithPw = false;
        }    
    }

    public void OnCreateAccount()
    {
        if (!is_can_act)
        {
            return;
        }

        if (!isWrongWithPw)
        {
            string user_name = usernameInput.text;
            string email = emailInput.text;
            string pass = passwordInput.text;

            if (user_name.Length > 0 && email.Length > 0 && email.Contains(".com") && pass.Length > 0)
            {
                is_can_act = false;
                email = email.Replace(".com", "");
                FirebaseManager.instance.CreateAccount(user_name, email, pass, Callback);
            }
        }
    }

    public void ShowLoginPage()
    {
        main.SetActive(false);
        loginAccount.SetActive(true);

        login_emailInput.text = "";
        login_passwordInput.text = "";
    }

    public void CloseLoginPage()
    {
        main.SetActive(true);
        loginAccount.SetActive(false);
    }

    public void Login()
    {
        if (!is_can_act)
        {
            return;
        }

        string email = login_emailInput.text;
        string pass = login_passwordInput.text;

        if (email.Length > 0 && email.Contains(".com") && pass.Length > 0)
        {
            is_can_act = false;

            email = email.Replace(".com", "");
            FirebaseManager.instance.Login(email, pass, Callback);
        }
    }

    public void GuestLogin()
    {
        if (!is_can_act)
        {
            return;
        }

        is_can_act = false;

        FirebaseManager.instance.CreateGuestAccount(Callback);
    }

    public void Callback(byte r)
    {
        if (r == 0)
        {
            if (DataManager.instance.user_data.card_list.Count < 15)
            {
                StartCoroutine(FirstOpenSupplyBox());
            }
            else
            {
                MoveScene();
            }
        }
        else
        {
            is_can_act = true;
        }
    }

    public IEnumerator FirstOpenSupplyBox()
    {
        List<int> player_card = new List<int>();

        for (int i = 0; i < 15; i++)
        {
            //중복 없이 15장의 카드를 뽑아야 한다.
            while (true)
            {
                int r = Random.Range(0, CardManager.instance.card_datas.Count);

                if (!player_card.Contains(r))
                {
                    player_card.Add(r);
                    DataManager.instance.user_data.card_list.Add(r);
                    break;
                }
            }
            yield return 0;
        }
        DataManager.instance.SaveUserData();

        //중복 없이 뽑은 15장의 카드를 상자 5개를 여는 방식으로 보여준다.
        int k = 0;
        for (int i = 0; i < 5; i++)
        {
            yield return GachaManager.instance.OnGacha(player_card.GetRange(k, 3).ToList());

            k += 3;
        }

        MoveScene();
    }

    public void MoveScene()
    {
        SceneManager.LoadScene("HomeScene");
    }

    public void ShowFindPWPage()
    {
        main.SetActive(false);
        findPassword.SetActive(true);

        find_emailInput.text = "";
        find_pw_txt.text = "";
    }

    public void CloseFindPWPage()
    {
        main.SetActive(true);
        findPassword.SetActive(false);
    }

    public void FindPw()
    {
        if (!is_can_act)
        {
            return;
        }

        string email = find_emailInput.text;

        if (email.Length > 0 && email.Contains(".com"))
        {
            email = email.Replace(".com", "");
            FirebaseManager.instance.GetPassword(email, GetPw);
        }
    }

    public void GetPw(string pw)
    {
        is_can_act = true;

        find_pw_txt.text = pw;
    }
}
