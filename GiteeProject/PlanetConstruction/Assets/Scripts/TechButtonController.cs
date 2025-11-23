using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TechButtonController : MonoBehaviour
{
    [Header("UI组件")]
    public Button techButton;
    public Text techNameText;
    public Text costText;
    public Text descriptionText;
    public Image backgroundImage;
    public GameObject lockedOverlay;
    public GameObject researchedOverlay;
    
    [Header("状态颜色")]
    public Color availableColor = Color.green;
    public Color unavailableColor = Color.gray;
    public Color researchedColor = Color.blue;
    public Color lockedColor = Color.red;
    
    private Technology assignedTechnology;
    private TechResearchLab researchLab;
    
    public void Initialize(Technology tech, TechResearchLab lab)
    {
        assignedTechnology = tech;
        researchLab = lab;
        
        UpdateButtonUI();
        
        techButton.onClick.RemoveAllListeners();
        techButton.onClick.AddListener(OnTechButtonClicked);
    }
    
    void UpdateButtonUI()
    {
        if (assignedTechnology == null) return;
        
        techNameText.text = assignedTechnology.techName;
        costText.text = $"金: {assignedTechnology.costGold}\n科技: {assignedTechnology.costTechPoint}";
        descriptionText.text = assignedTechnology.description;
        
        if (assignedTechnology.isResearched)
        {
            // 已研究
            backgroundImage.color = researchedColor;
            lockedOverlay.SetActive(false);
            researchedOverlay.SetActive(true);
            techButton.interactable = false;
        }
        else if (assignedTechnology.isUnlocked)
        {
            // 已解锁可研究
            backgroundImage.color = availableColor;
            lockedOverlay.SetActive(false);
            researchedOverlay.SetActive(false);
            techButton.interactable = true;
        }
        else
        {
            // 未解锁
            backgroundImage.color = lockedColor;
            lockedOverlay.SetActive(true);
            researchedOverlay.SetActive(false);
            techButton.interactable = false;
        }
    }
    
    void OnTechButtonClicked()
    {
        if (assignedTechnology != null && researchLab != null)
        {
            bool success = researchLab.ResearchTechnology(assignedTechnology.id);
            if (success)
            {
                UpdateButtonUI();
            }
        }
    }
    
    public void RefreshUI()
    {
        UpdateButtonUI();
    }
    
    public int GetTechId()
    {
        return assignedTechnology != null ? assignedTechnology.id : -1;
    }
}