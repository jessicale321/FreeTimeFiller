using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.CloudSave;
using UnityEngine;

///-///////////////////////////////////////////////////////////
/// 
public class CoinManager : MonoBehaviour
{
    public static CoinManager instance { get; private set; }
    public int coins { get; private set; }
    [SerializeField] private TMP_Text coinsDisplayText;

    ///-///////////////////////////////////////////////////////////
    /// 
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    ///-///////////////////////////////////////////////////////////
    /// 
    private void OnEnable()
    {
        LoadCoins();
        coinsDisplayText.text = coins.ToString();
    }

    ///-///////////////////////////////////////////////////////////
    /// 
    private void Update()
    {
        // get more coins
        if (Input.GetKeyDown("2"))
        {
            EarnCoins(100);
        }
        if (Input.GetKeyDown("1"))
        {
            SpendCoins(100);
        }
    }

    ///-///////////////////////////////////////////////////////////
    /// 
    public void SpendCoins(int amt)
    {
        coins -= amt;
        coinsDisplayText.text = coins.ToString();
        Debug.Log("coins: " + coins);
        SaveCoins();
    }

    ///-///////////////////////////////////////////////////////////
    /// 
    public void EarnCoins(int amt)
    {
        coins += amt;
        coinsDisplayText.text = coins.ToString();
        Debug.Log("coins: " + coins);
        SaveCoins();
    }

    ///-///////////////////////////////////////////////////////////
    /// 
    private async void SaveCoins()
    {
        var data = new Dictionary<string, object> { { "coins", coins } };
        await CloudSaveService.Instance.Data.Player.SaveAsync(data);
    }

    ///-///////////////////////////////////////////////////////////
    /// 
    private async void LoadCoins()
    {
        var data = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { "coins" });
        if (data.TryGetValue("coins", out var coinValue))
        {
            coins = coinValue.Value.GetAs<int>();
        }
    }
}
