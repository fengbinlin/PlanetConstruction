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
    
    [Header("UI引用")]
    public Transform techButtonParent; // 科技按钮的父对象
    public GameObject techButtonPrefab; // 科技按钮预制体
    
    // 事件：当科技研究完成时触发
    public static event Action<Technology> OnTechnologyResearched;
    
    void Start()
    {
        InitializeTechnologies();
        CreateTechButtons();
    }
    
    void InitializeTechnologies()
    {
        allTechnologies.Clear();
        
        // 根据你的科技树图初始化科技
        // 序号1：矿机1 - 默认解锁
        allTechnologies.Add(new Technology(1, "矿机1", "解锁第一台自动矿机", 
            new List<int>(), 0, 0, 0, 0, true));
        allTechnologies[0].isUnlocked = true;
        
        // 序号2：挖矿buff - 需要前置1
        allTechnologies.Add(new Technology(2, "初级挖矿强化", "提升手动采矿效率", 
            new List<int>{1}, 100, 0, 1.0f, 0));
        
        // 序号3：矿机buff - 需要前置1
        allTechnologies.Add(new Technology(3, "初级矿机加速", "提升矿机工作效率", 
            new List<int>{1}, 150, 0, 0, 0.5f));
        
        // 序号4：挖矿buff - 需要前置2
        allTechnologies.Add(new Technology(4, "中级挖矿强化", "进一步提升手动采矿效率", 
            new List<int>{2}, 300, 0, 2.0f, 0));
        
        // 序号5：矿机buff - 需要前置3
        allTechnologies.Add(new Technology(5, "中级矿机加速", "进一步提升矿机工作效率", 
            new List<int>{3}, 400, 0, 0, 1.0f));
        
        // 序号6：新矿机 - 需要前置4和5
        allTechnologies.Add(new Technology(6, "矿机2", "解锁第二台自动矿机", 
            new List<int>{4,5}, 800, 0, 0, 0, true));
    }
    
    void CreateTechButtons()
    {
        if (techButtonParent == null || techButtonPrefab == null) return;
        
        // 清空现有的按钮
        foreach (Transform child in techButtonParent)
        {
            Destroy(child.gameObject);
        }
        
        // 创建新的科技按钮
        foreach (Technology tech in allTechnologies)
        {
            GameObject buttonObj = Instantiate(techButtonPrefab, techButtonParent);
            TechButtonController buttonController = buttonObj.GetComponent<TechButtonController>();
            
            if (buttonController != null)
            {
                buttonController.Initialize(tech, this);
            }
        }
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
        
        // 解锁后续科技
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
                    
                    // 更新对应的UI按钮
                    UpdateTechButton(tech.id);
                }
            }
        }
    }
    
    // 更新科技按钮状态
    public void UpdateTechButton(int techId)
    {
        if (techButtonParent == null) return;
        
        TechButtonController[] buttons = techButtonParent.GetComponentsInChildren<TechButtonController>();
        foreach (TechButtonController button in buttons)
        {
            if (button.GetTechId() == techId)
            {
                button.RefreshUI();
                break;
            }
        }
    }
    
    // 根据ID获取科技 - 这里是我遗漏的方法
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