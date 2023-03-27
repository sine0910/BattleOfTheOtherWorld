using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Analytics;
using Firebase.Extensions;

public class FirebaseManager : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(checkTask =>
        {
            var dependencyStatus = checkTask.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                Auth();
            }
            else
            {
                UnityEngine.Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });
    }

    void Auth()
    {
        Debug.Log("Login_Auth");
        FirebaseAuth.DefaultInstance.SignInWithEmailAndPasswordAsync("", "");
    }


}
