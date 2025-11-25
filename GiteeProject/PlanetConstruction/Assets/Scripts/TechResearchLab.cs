using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class Technology
{
    public int id;
    public string techName;
    public string description;
    public List<int> prerequisites; // 前置科技ID列表
    public int costGold;
    public int costTechPoint;
    public bool isUnlocked;
    public bool isResearched;
    public float manualMiningBonus;
    public float minerRateBonus;
    public bool unlocksNewMiner;

    

    public Technology(int id, string name, string desc, List<int> prerequisites,
                     int costGold, int costTechPoint, float manualBonus = 0, float minerBonus = 0, bool unlocksMiner = false)
    {
        this.id = id;
        this.techName = name;
        this.description = desc;
        this.prerequisites = prerequisites;
        this.costGold = costGold;
        this.costTechPoint = costTechPoint;
        this.manualMiningBonus = manualBonus;
        this.minerRateBonus = minerBonus;
        this.unlocksNewMiner = unlocksMiner;
        this.isUnlocked = false;
        this.isResearched = false;
    }
}


public class TechResearchLab : MonoBehaviour
{
    [Header("科技列表")]
    public List<Technology> allTechnologies = new List<Technology>();
    public GameObject TechPanel;
    // 事件：当科技研究完成时触发
    public event Action<Technology> OnTechnologyResearched;

    void Awake()
    {
        Debug.Log("TechResearchLab Awake called");
        InitializeTechnologies();
    }

    public void CloseTechPanel()
    {
        TechPanel.gameObject.SetActive(false);
    }

    public void InitializeTechnologies()
    {
        Debug.Log("初始化科技数据");
        allTechnologies.Clear();

        // 科技数据初始化
        allTechnologies.Add(new Technology(1, "矿机1", "解锁第一台自动矿机",
            new List<int>(), 0, 0, 0, 0, true));
        allTechnologies[0].isUnlocked = true;

        allTechnologies.Add(new Technology(2, "初级挖矿强化", "提升手动采矿效率",
            new List<int> { 1 }, 100, 0, 1.0f, 0));

        allTechnologies.Add(new Technology(3, "初级矿机加速", "提升矿机工作效率",
            new List<int> { 1 }, 150, 0, 0, 0.5f));

        allTechnologies.Add(new Technology(4, "中级挖矿强化", "进一步提升手动采矿效率",
            new List<int> { 2 }, 300, 0, 2.0f, 0));

        allTechnologies.Add(new Technology(5, "中级矿机加速", "进一步提升矿机工作效率",
            new List<int> { 3 }, 400, 0, 0, 1.0f));

        allTechnologies.Add(new Technology(6, "矿机2", "解锁第二台自动矿机",
            new List<int> { 4, 5 }, 800, 0, 0, 0, true));

        Debug.Log($"科技初始化完成，共 {allTechnologies.Count} 个科技");
    }

    // 检查科技是否可以研究
    public bool CanResearchTechnology(int techId)
    {
        Technology tech = GetTechnologyById(techId);
        if (tech == null) return false;

        if (tech.isResearched) return false;
        if (!tech.isUnlocked) return false;

        GameValManager manager = GameValManager.gameValManager;
        if (manager == null) return false;

        return manager.HasEnoughResources(tech.costGold, tech.costTechPoint);
    }

    // 研究科技
    public bool ResearchTechnology(int techId)
    {
        Technology tech = GetTechnologyById(techId);
        if (tech == null || !CanResearchTechnology(techId)) return false;

        GameValManager manager = GameValManager.gameValManager;
        if (manager == null || !manager.SpendResources(tech.costGold, tech.costTechPoint))
            return false;

        // 研究科技
        tech.isResearched = true;

        // 应用科技效果
        ApplyTechnologyEffects(tech);

        // 【关键】解锁后续科技
        UnlockSubsequentTechnologies(techId);

        // 触发事件
        OnTechnologyResearched?.Invoke(tech);

        Debug.Log($"科技研究成功: {tech.techName}");
        return true;
    }

    // 应用科技效果
    private void ApplyTechnologyEffects(Technology tech)
    {
        GameValManager manager = GameValManager.gameValManager;
        if (manager == null) return;

        if (tech.manualMiningBonus > 0)
        {
            manager.AddManualMiningBonus(tech.manualMiningBonus);
        }

        if (tech.minerRateBonus > 0)
        {
            manager.AddMinerRateBonus(tech.minerRateBonus);
        }

        if (tech.unlocksNewMiner)
        {
            manager.UnlockNewMiner();
        }
    }

    // 解锁后续科技
    private void UnlockSubsequentTechnologies(int researchedTechId)
    {
        foreach (Technology tech in allTechnologies)
        {
            if (!tech.isUnlocked && tech.prerequisites.Contains(researchedTechId))
            {
                bool allPrerequisitesMet = true;
                foreach (int preReq in tech.prerequisites)
                {
                    Technology preReqTech = GetTechnologyById(preReq);
                    if (preReqTech == null || !preReqTech.isResearched)
                    {
                        allPrerequisitesMet = false;
                        break;
                    }
                }

                if (allPrerequisitesMet)
                {
                    tech.isUnlocked = true;
                    Debug.Log($"科技已解锁: {tech.techName}");
                }
            }
        }
    }

    // 根据ID获取科技
    public Technology GetTechnologyById(int techId)
    {
        foreach (Technology tech in allTechnologies)
        {
            if (tech.id == techId)
            {
                return tech;
            }
        }
        return null;
    }

    // 获取可研究的科技列表
    public List<Technology> GetResearchableTechnologies()
    {
        List<Technology> researchable = new List<Technology>();
        foreach (Technology tech in allTechnologies)
        {
            if (CanResearchTechnology(tech.id))
            {
                researchable.Add(tech);
            }
        }
        return researchable;
    }

    // 获取已解锁但未研究的科技列表
    public List<Technology> GetUnlockedButNotResearchedTechnologies()
    {
        List<Technology> unlocked = new List<Technology>();
        foreach (Technology tech in allTechnologies)
        {
            if (tech.isUnlocked && !tech.isResearched)
            {
                unlocked.Add(tech);
            }
        }
        return unlocked;
    }
}