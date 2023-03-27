using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartManager : MonoBehaviour
{
    public GameObject main;
    public GameObject createAccount;

    public InputField nicknameInput;
    public InputField emailInput;
    public InputField passwordInput;
    public InputField repeatepwInput;

    bool isWrongWithPw;

    private void Awake()
    {
        main.SetActive(true);
        createAccount.SetActive(false);
    }

    public void ShowCreateAccountPage()
    {
        main.SetActive(false);
        createAccount.SetActive(true);

        isWrongWithPw = false;
        nicknameInput.text = "";
        emailInput.text = "";
        passwordInput.text = "";
        repeatepwInput.text = "";
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
        if (!isWrongWithPw)
        {
            //
        }
    }

    public void Login()
    {
        
    }

    public void GuestLogin()
    {
        
    }
}
