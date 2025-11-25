using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    public GameObject upgradePanel;
    public UpgradeCardUI[] cardSlots;

    private List<BulletBuffNormal> targetBuffs;
    private List<UpgradeCard> currentOptions;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //tDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
        upgradePanel.SetActive(false);
    }

    public void ShowUpgradeCards(List<BulletBuffNormal> buffs)
    {
        targetBuffs = buffs;
        upgradePanel.SetActive(true);

        // 获取所有可用卡牌（从第一个buff获取，假设所有buff的升级状态相同）
        var availableCards = UpgradeCardDatabase.GetAvailableCards(buffs[0]);

        if (availableCards.Count == 0)
        {
            Debug.LogWarning("没有可用的升级卡牌！");
            upgradePanel.SetActive(false);
            Time.timeScale = 1f;
            return;
        }

        // 加权随机选择
        var weightedCards = new List<UpgradeCard>();
        foreach (var card in availableCards)
        {
            for (int i = 0; i < card.weight; i++)
            {
                weightedCards.Add(card);
            }
        }

        // 随机选择3张不重复的卡牌
        currentOptions = new List<UpgradeCard>();
        var tempWeightedCards = new List<UpgradeCard>(weightedCards);
        
        for (int i = 0; i < Mathf.Min(3, tempWeightedCards.Count); i++)
        {
            if (tempWeightedCards.Count == 0) break;
            
            int randomIndex = Random.Range(0, tempWeightedCards.Count);
            UpgradeCard selectedCard = tempWeightedCards[randomIndex];
            currentOptions.Add(selectedCard);
            
            // 移除所有相同卡牌的实例
            tempWeightedCards.RemoveAll(card => card.id == selectedCard.id);
        }

        // 设置UI
        for (int i = 0; i < cardSlots.Length; i++)
        {
            if (i < currentOptions.Count)
            {
                cardSlots[i].Setup(currentOptions[i], OnCardSelected);
                cardSlots[i].gameObject.SetActive(true);
            }
            else
            {
                cardSlots[i].gameObject.SetActive(false);
            }
        }

        Time.timeScale = 0f;
    }

    void OnCardSelected(UpgradeCard card)
    {
        foreach (var buff in targetBuffs)
        {
            // 检查该buff是否可以应用此升级
            var availableCards = UpgradeCardDatabase.GetAvailableCards(buff);
            if (availableCards.Any(c => c.id == card.id))
            {
                card.Apply(buff);
                Debug.Log($"应用升级: {card.title} 到 {buff.gameObject.name}");
            }
        }

        upgradePanel.SetActive(false);
        Time.timeScale = 1f;
        
        // 清除引用
        targetBuffs = null;
        currentOptions = null;
    }

    // 调试方法：显示所有可用升级
    public void DebugShowAvailableUpgrades()
    {
        if (targetBuffs != null && targetBuffs.Count > 0)
        {
            var available = UpgradeCardDatabase.GetAvailableCards(targetBuffs[0]);
            Debug.Log($"可用升级数量: {available.Count}");
            foreach (var card in available)
            {
                Debug.Log($"- {card.title} (ID: {card.id})");
            }
        }
    }
}