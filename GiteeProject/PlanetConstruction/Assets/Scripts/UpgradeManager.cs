using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    public GameObject upgradePanel; // UI面板（包含3个按钮）
    public UpgradeCardUI[] cardSlots; // 卡牌按钮UI脚本

    private BulletBuffNormal targetBuff;
    private List<BulletBuffNormal> targetBuffs;
    void Awake()
    {
        Instance = this;
        upgradePanel.SetActive(false);
    }

    // 显示升级选项
    public void ShowUpgradeCards(List<BulletBuffNormal> buffs)
    {
        targetBuffs = buffs;
        upgradePanel.SetActive(true);

        // 获取全部候选卡牌
        List<UpgradeCard> allCards = UpgradeCardDatabase.GetAllCards();

        // 打乱顺序
        for (int i = 0; i < allCards.Count; i++)
        {
            UpgradeCard temp = allCards[i];
            int randomIndex = Random.Range(i, allCards.Count);
            allCards[i] = allCards[randomIndex];
            allCards[randomIndex] = temp;
        }

        // 取前3张，不重复
        for (int i = 0; i < cardSlots.Length; i++)
        {
            UpgradeCard card = allCards[i];
            cardSlots[i].Setup(card, OnCardSelected);
        }

        // 暂停游戏
        Time.timeScale = 0f;
    }

    // 选择一张卡牌
    void OnCardSelected(UpgradeCard card)
    {
        // 对列表中的所有Buff应用升级
        foreach (var buff in targetBuffs)
        {
            card.Apply(buff);
        }
        upgradePanel.SetActive(false);

        // 恢复游戏
        Time.timeScale = 1f;
    }
}