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
        await DataManager.SaveData("coins", coins);
    }

    ///-///////////////////////////////////////////////////////////
    /// 
    private async void LoadCoins()
    {
        coins = await DataManager.LoadData<int>("coins");
        coinsDisplayText.text = coins.ToString();
    }
}
