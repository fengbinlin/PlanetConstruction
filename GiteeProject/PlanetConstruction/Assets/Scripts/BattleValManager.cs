using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleValManager : MonoBehaviour
{
    public GameObject MainUIObject;
    public GameObject GameWinObject;
    public GameObject GameFailObject;
    public static BattleValManager Instance;

    public int Level = 1;
    public int curExp = 0;
    public int LevelUpExp = 10;

    public Text textLevel;
    public Text textExp;
    public Slider expSlider; // 等级进度条

    public List<BulletBuffNormal> bulletBuffs; // 改为Buff列表

    // 玩家生命值
    public float playerHP = 100f;
    public float maxPlayerHP = 100f; // 最大血量
    public Slider playerHpSlider;    // ★ 新增：玩家血量条

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        UpdateUI();
    }
    // 添加Buff到列表的方法
    public void AddBulletBuff(BulletBuffNormal newBuff)
    {
        if (bulletBuffs == null)
        {
            bulletBuffs = new List<BulletBuffNormal>();
        }

        if (!bulletBuffs.Contains(newBuff))
        {
            bulletBuffs.Add(newBuff);
        }
    }

    // 从列表移除Buff的方法
    public void RemoveBulletBuff(BulletBuffNormal buffToRemove)
    {
        if (bulletBuffs != null && bulletBuffs.Contains(buffToRemove))
        {
            bulletBuffs.Remove(buffToRemove);
        }
    }

    public void GainExp(int amount)
    {
        curExp += amount;
        if (curExp >= LevelUpExp)
        {
            Level++;
            curExp = 0;
            LevelUpExp = Mathf.RoundToInt(LevelUpExp * 1.4f);
            UpdateUI();

            // 检查是否胜利
            if (Level >= 10)
            {
                //获胜，暂停时间，然后把胜利UI调出来,主UI关闭
                //FindObjectOfType<BattleSceneController>().OnWinButton();
                Time.timeScale = 0f;
                MainUIObject.SetActive(false);
                GameFailObject.SetActive(false);
                GameWinObject.SetActive(true);
                return;
            }

            UpgradeManager.Instance.ShowUpgradeCards(bulletBuffs);
        }
        else
        {
            UpdateUI();
        }
    }

    public void TakeDamage(float val)
    {
        playerHP -= val;
        if (playerHP <= 0)
        {
            playerHP = 0;
            UpdateUI();
            Time.timeScale = 0f;
            // 游戏失败，把失败UI显示出来
            MainUIObject.SetActive(false);
            GameFailObject.SetActive(true);
            GameWinObject.SetActive(false);
            //FindObjectOfType<BattleSceneController>().OnLoseButton();
        }
        else
        {
            UpdateUI();
        }
    }

    void UpdateUI()
    {
        // 等级
        if (textLevel != null) textLevel.text = "Lv." + Level;
        // 经验文本
        if (textExp != null) textExp.text = curExp + "/" + LevelUpExp;
        // 等级进度条
        if (expSlider != null)
        {
            expSlider.maxValue = LevelUpExp;
            expSlider.value = curExp;
        }
        // ★ 更新玩家血量条
        if (playerHpSlider != null)
        {
            playerHpSlider.maxValue = maxPlayerHP;
            playerHpSlider.value = playerHP;
        }
    }
}