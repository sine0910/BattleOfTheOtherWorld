using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using UnityEngine.Purchasing.Security;

using System;

public class HomeManager : MonoBehaviour, IStoreListener
{
    public static HomeManager instance;

    public GameObject CardCollection;
    public Transform collection_content;
    public GameObject card_image;
    public List<Image> card_image_pool = new List<Image>();

    public GameObject SupplyBox;
    private IStoreController controller;
    private IGooglePlayStoreExtensions googleExtensions;
    private IAppleExtensions appleExtensions;
    private ITransactionHistoryExtensions transactionExtensions;

    public Text free_sb_text;
    bool Is_aleady_opened;

    public GameObject Friend;
    public InputField friendId;
    public Button inviteButton;
    public Button acceptButton;

    public GameObject Leaderboard;
    public Transform rank_content;
    public GameObject rank_slot;
    public List<RankSlot> rank_slot_pool = new List<RankSlot>();

    public bool is_working = false;

    public string environment = "production";

    async void Start()
    {
        instance = this;

        try
        {
            var options = new InitializationOptions().SetEnvironmentName(environment);

            await UnityServices.InitializeAsync(options);

            var module = StandardPurchasingModule.Instance();
            var builder = ConfigurationBuilder.Instance(module);

            builder.AddProduct("SupplyBox", ProductType.Consumable, new IDs { { "SupplyBox", GooglePlay.Name, AppleAppStore.Name } });

            UnityPurchasing.Initialize(this, builder);
        }
        catch (Exception exception)
        {
            // An error occurred during services initialization.
        }
    }

    #region Collection
    public void ShowCardCollection()
    {
        is_working = true;

        CardCollection.SetActive(true);

        for (int i = 0; i < DataManager.instance.user_data.card_list.Count; i++)
        {
            Image im = null;

            if (card_image_pool.Count <= i)
            {
                im = Instantiate(card_image, collection_content).GetComponent<Image>();
                card_image_pool.Add(im);
            }
            else
            {
                card_image_pool[i].gameObject.SetActive(true);
                im = card_image_pool[i];
            }

            im.sprite = CardManager.instance.GetCardSprites(DataManager.instance.user_data.card_list[i]);
        }

        is_working = false;
    }

    public void CloseCardCollection()
    {
        if (is_working)
        {
            return;
        }

        is_working = true;

        for (int i = 0; i < DataManager.instance.user_data.card_list.Count; i++)
        {
            card_image_pool[i].gameObject.SetActive(false);
        }

        CardCollection.SetActive(false);

        is_working = false;
    }
    #endregion

    #region SupplyBox
    public void ShowSupplyBoxes()
    {
        is_working = true;

        Is_aleady_opened = true;

        SupplyBox.SetActive(true);

        FirebaseManager.instance.GetWhenOpenFreeBox(SetIsCanOpen);
    }

    public void SetIsCanOpen(long result)
    {
        DateTime t = FirebaseManager.instance.UnixTimeStampToUnixDateTime(result).AddDays(7);
        DateTime n = DateTime.UtcNow;

        if (t > n)
        {
            free_sb_text.text = "Already Opened\nSupply Box";
            Is_aleady_opened = true;
        }
        else
        {
            free_sb_text.text = "Free Weekly\nSupply Box";
            Is_aleady_opened = false;
        }

        is_working = false;
    }

    public void CloseSupplyBoxes()
    {
        SupplyBox.SetActive(false);
    }

    public void OpenFreeSupplyBoxes()
    {
        Debug.Log("OpenFreeSupplyBoxes");

        if (is_working || Is_aleady_opened)
        {
            return;
        }

        is_working = true;

        FirebaseManager.instance.UpdateWhenOpenFreeBox(FreeSupplyBoxesTimeUpdateResult);
    }

    void FreeSupplyBoxesTimeUpdateResult(byte r)
    {
        if (r == 0)
        {
            free_sb_text.text = "Already Opened\nSupply Box";
            Is_aleady_opened = true;

            OpenSupplyBox();
        }

        is_working = false;
    }

    public void BuySupplyBoxes()
    {
        Debug.Log("BuySupplyBoxes");
        controller.InitiatePurchase("SupplyBox");
    }

    public void OpenSupplyBox()
    {
        List<int> cards = new List<int>();

        for (int i = 0; i < 3; i++)
        {
            int r = UnityEngine.Random.Range(0, CardManager.instance.card_datas.Count);

            cards.Add(r);

            if (!DataManager.instance.user_data.card_list.Contains(r))
            {
                DataManager.instance.user_data.card_list.Add(r);
            }
        }
        DataManager.instance.SaveUserData();

        StartCoroutine(GachaManager.instance.OnGacha(cards));
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        throw new System.NotImplementedException();
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        throw new System.NotImplementedException();
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        throw new System.NotImplementedException();
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("OnInitialized Store");

        this.controller = controller;
        appleExtensions = extensions.GetExtension<IAppleExtensions>();
        googleExtensions = extensions.GetExtension<IGooglePlayStoreExtensions>();
        transactionExtensions = extensions.GetExtension<ITransactionHistoryExtensions>();
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
    {
        bool validPurchase = true;

#if UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX
        //var validator = new CrossPlatformValidator(GooglePlayTangle.Data(),  AppleTangle.Data(), Application.bundleIdentifier);

        //try
        //{
        //    var result = validator.Validate(purchaseEvent.purchasedProduct.receipt);

        //    Debug.Log("Receipt is valid. Contents:");
        //    foreach (IPurchaseReceipt productReceipt in result)
        //    {
        //        GooglePlayReceipt google = productReceipt as GooglePlayReceipt;
        //        if (null != google)
        //        {
        //            Debug.Log(google.transactionID);
        //            Debug.Log(google.purchaseState);
        //            Debug.Log(google.purchaseToken);
        //        }

        //        AppleInAppPurchaseReceipt apple = productReceipt as AppleInAppPurchaseReceipt;
        //        if (null != apple)
        //        {
        //            Debug.Log(apple.originalTransactionIdentifier);
        //            Debug.Log(apple.subscriptionExpirationDate);
        //            Debug.Log(apple.cancellationDate);
        //            Debug.Log(apple.quantity);
        //        }
        //    }
        //}
        //catch (IAPSecurityException)
        //{
        //    validPurchase = false;
        //}
#endif

        if (validPurchase)
        {
            OpenSupplyBox();
        }
        else
        {
          
        }

        return PurchaseProcessingResult.Complete;
    }
    #endregion

    #region Friend
    public void ShowFriendPage()
    {
        is_working = true;

        Friend.SetActive(true);
        inviteButton.gameObject.SetActive(true);
        acceptButton.gameObject.SetActive(false);

        ListeningStart();

        is_working = false;
    }

    public void CloseFriendPage()
    {
        if (is_working)
        {
            return;
        }

        ListeningCancle();

        Friend.SetActive(false);
    }

    public void InviteFriend()
    {
        if (is_working)
        {
            return;
        }

        if (friendId.text != "" && friendId.text != DataManager.instance.user_data.username)
        {
            is_working = true;

            MultiManager.instance.InitData();
            FirebaseManager.instance.InviteFriend(friendId.text, InviteFriendResult);
        }
    }

    public void InviteFriendResult(byte r)
    {
        is_working = false;

        if (r == 0)
        {
            ListeningStart();
            FirebaseManager.instance.db_ref.Child("Match").Child(MultiManager.instance.root).Child("msg").SetValueAsync("2/" + DataManager.instance.user_data.username);
        }
    }

    public void RecieveInvite()
    {
        inviteButton.gameObject.SetActive(false);
        acceptButton.gameObject.SetActive(true);
    }

    public void AcceptFriend()
    {
        acceptButton.gameObject.SetActive(false);

        FirebaseManager.instance.AcceptInvite();
    }

    public void ListeningStart()
    {
        if (play)
        {
            ListeningCancle();
        }

        MultiManager.instance.root = DataManager.instance.user_data.email;
        FirebaseManager.instance.ListenMatchStatus();
    }

    public void ListeningCancle()
    {
        FirebaseManager.instance.CancleMatch();
    }
    #endregion

    #region Ranking
    public void ShowRanking()
    {
        is_working = true;

        Leaderboard.SetActive(true);

        FirebaseManager.instance.GetRanking(SetRank);
    }

    void SetRank(Dictionary<byte, List<string>> r)
    {
        is_working = false;

        if (r.Count != 0)
        {
            int k = 0;
            for (int i = r[0].Count; i > 0; i--)
            {
                if (rank_slot_pool.Count > k)
                {
                    rank_slot_pool[k].gameObject.SetActive(true);
                }
                else
                {
                    RankSlot rs = Instantiate(rank_slot, rank_content).GetComponent<RankSlot>();
                    rank_slot_pool.Add(rs);
                }

                bool is_me = false;
                if (DataManager.instance.user_data.username == r[0][i - 1])
                {
                    is_me = true;
                }

                rank_slot_pool[k].Init(is_me, i, r[0][i - 1], r[1][i - 1]);
                k++;
            }
        }
    }

    public void CloseRanking()
    {
        if (is_working)
        {
            return;
        }

        for (int i = 0; i < rank_slot_pool.Count; i++)
        {
            rank_slot_pool[i].gameObject.SetActive(false);
        }

        Leaderboard.SetActive(false);
    }
    #endregion


    bool play = false;
    public void GameStart()
    {
        if (play && MultiManager.instance.is_host)
        {
            FirebaseManager.instance.CloseGameRoom();
        }

        play = true;
        FirebaseManager.instance.StartGame();
    }

    public void OnDestroy()
    {
        if (play && MultiManager.instance.is_host)
        {
            FirebaseManager.instance.CloseGameRoom();
        }
    }
}
