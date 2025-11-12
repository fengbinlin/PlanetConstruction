using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameValManager : MonoBehaviour
{
    public static GameValManager gameValManager;
    public int valMoney = 0;
    public Text moneyValUI;
    
    void Awake()
    {
        gameValManager = this;
    }
    
    void Start()
    {
        UpdateMoneyUI();
    }

    void Update()
    {

    }
    
    public void GetMoney(int val)
    {
        valMoney += val;
        UpdateMoneyUI();
    }
    
    void UpdateMoneyUI()
    {
        if (moneyValUI != null)
        {
            moneyValUI.text = valMoney.ToString();
        }
    }
}