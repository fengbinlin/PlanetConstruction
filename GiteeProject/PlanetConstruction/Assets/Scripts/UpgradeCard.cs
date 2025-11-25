using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class UpgradeCard
{
    public string id;
    public string title;
    public string desc;
    public System.Action<BulletBuffNormal> applyEffect;
    public string[] prerequisites; // 前置升级ID
    public string[] excludes;      // 互斥升级ID
    public int weight = 10;       // 出现权重

    public UpgradeCard(string id, string t, string d, System.Action<BulletBuffNormal> effect, 
                      string[] prereqs = null, string[] excl = null, int weight = 10)
    {
        this.id = id;
        title = t;
        desc = d;
        applyEffect = effect;
        prerequisites = prereqs ?? new string[0];
        excludes = excl ?? new string[0];
        this.weight = weight;
    }

    public void Apply(BulletBuffNormal buff)
    {
        if (applyEffect != null) 
        {
            applyEffect.Invoke(buff);
            buff.acquiredUpgrades.Add(id);
        }
    }

    // 检查是否满足前置条件
    public bool CheckPrerequisites(BulletBuffNormal buff)
    {
        foreach (string prereq in prerequisites)
        {
            if (!buff.acquiredUpgrades.Contains(prereq))
                return false;
        }
        return true;
    }

    // 检查是否有互斥升级
    public bool CheckExcludes(BulletBuffNormal buff)
    {
        foreach (string excl in excludes)
        {
            if (buff.acquiredUpgrades.Contains(excl))
                return false;
        }
        return true;
    }
}

public static class UpgradeCardDatabase
{
    private static Dictionary<string, UpgradeCard> allCards;

    static UpgradeCardDatabase()
    {
        allCards = new Dictionary<string, UpgradeCard>();
        InitializeCards();
    }

    private static void InitializeCards()
    {
        // 基础升级（无前置）
        AddCard(new UpgradeCard("damage_1", "增强伤害", "主子弹伤害+5", 
            buff => buff.damage += 5f));
        
        AddCard(new UpgradeCard("trajectory_1", "增加弹道", "弹道数+1", 
            buff => buff.trajectoryCount += 1));
        
        AddCard(new UpgradeCard("speed_1", "加快射速", "冷却时间减少20%", 
            buff => buff.fireCooldown *= 0.8f));
        
        AddCard(new UpgradeCard("bounce_1", "弹射强化", "弹射次数+1", 
            buff => buff.bounceTimes += 1, new[]{"subbullet_1"}));
        
        AddCard(new UpgradeCard("subbullet_1", "副子弹数量增加", "副子弹数量+1", 
            buff => buff.subBulletCount += 1));
        
        AddCard(new UpgradeCard("subdamage_1", "副子弹伤害提升", "副子弹伤害+3", 
            buff => buff.subBulletDamage += 3f, new[]{"subbullet_1"}));

        // 进阶升级（有前置关系）
        AddCard(new UpgradeCard("damage_2", "强力射击", "主子弹伤害+8（需要增强伤害）", 
            buff => buff.damage += 8f, new[]{"damage_1"}));
        
        // AddCard(new UpgradeCard("crit_1", "暴击几率", "获得10%暴击几率", 
        //     buff => buff.critChance += 0.1f, new[]{"damage_2"}));
        
        // AddCard(new UpgradeCard("crit_2", "致命一击", "暴击倍率提升至3倍", 
        //     buff => buff.critMultiplier = 3f, new[]{"crit_1"}));
        
        AddCard(new UpgradeCard("penetration_1", "穿透强化", "穿透次数+2", 
            buff => buff.penetration += 2, new[]{"bounce_1"}));
        
        AddCard(new UpgradeCard("rapid_fire", "快速连射", "每次射击连续发射2发子弹", 
            buff => { 
                buff.hasRapidFire = true; 
                buff.rapidFireCount = 2;
                buff.burstInterval = 0.08f; // 设置连发间隔
            }, new[]{"speed_1"}));
        
        AddCard(new UpgradeCard("spread_shot", "散射", "弹道呈扇形散射", 
            buff => { 
                buff.hasSpreadShot = true; 
                buff.spreadAngle = 15f; 
            }, new[]{"trajectory_1"}));

        // 特殊升级（高权重，强效果）
        AddCard(new UpgradeCard("triple_shot", "三重射击", "弹道数+2，冷却减少15%", 
            buff => { 
                buff.trajectoryCount += 2; 
                buff.fireCooldown *= 0.85f; 
            }, 
            new[]{"trajectory_1", "speed_1"}, weight: 5));
        
        AddCard(new UpgradeCard("bullet_hell", "弹幕地狱", "弹道数+3，副子弹数量+2", 
            buff => { 
                buff.trajectoryCount += 3; 
                buff.subBulletCount += 2; 
            }, 
            new[]{"triple_shot", "subbullet_1"}, weight: 3));
        
        // AddCard(new UpgradeCard("ultimate_crit", "终极暴击", "暴击几率+20%，暴击倍率4倍", 
        //     buff => { 
        //         buff.critChance += 0.2f; 
        //         buff.critMultiplier = 4f; 
        //     }, 
        //     new[]{"crit_2"}, weight: 2));
        
        // 更多基础升级
        AddCard(new UpgradeCard("burst_1", "连发强化", "连发次数+1", 
            buff => {                
                buff.hasRapidFire = true; 
                buff.rapidFireCount = 1;// 设置连发间隔
                buff.burstInterval = 0.08f; }));
        
        AddCard(new UpgradeCard("speed_2", "极限射速", "冷却时间再减少25%", 
            buff => buff.fireCooldown *= 0.75f, new[]{"speed_1"}));
        
        AddCard(new UpgradeCard("bullet_speed", "子弹加速", "子弹速度提升50%", 
            buff => buff.bulletSpeed *= 1.5f));
        
        AddCard(new UpgradeCard("rapid_fire_2", "急速连射", "连射数量+2，间隔减少", 
            buff => { 
                buff.hasRapidFire = true; 
                buff.rapidFireCount += 2;
                buff.burstInterval *= 0.7f; // 减少间隔
            }, new[]{"rapid_fire"}));
        
        AddCard(new UpgradeCard("wide_shot", "宽幅散射", "散射角度增加20度", 
            buff => buff.spreadAngle += 20f, new[]{"spread_shot"}));
    }

    private static void AddCard(UpgradeCard card)
    {
        allCards[card.id] = card;
    }

    public static List<UpgradeCard> GetAvailableCards(BulletBuffNormal buff)
    {
        var availableCards = new List<UpgradeCard>();
        
        foreach (var card in allCards.Values)
        {
            // 检查是否已获得
            if (buff.acquiredUpgrades.Contains(card.id))
                continue;
                
            // 检查前置条件
            if (!card.CheckPrerequisites(buff))
                continue;
                
            // 检查互斥升级
            if (!card.CheckExcludes(buff))
                continue;
                
            availableCards.Add(card);
        }
        
        return availableCards;
    }

    public static UpgradeCard GetCardById(string id)
    {
        return allCards.ContainsKey(id) ? allCards[id] : null;
    }
}