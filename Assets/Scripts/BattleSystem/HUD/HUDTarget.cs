using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDTarget : MonoBehaviour
{
    [SerializeField] Canvas HUD;
    [SerializeField] GameObject TargetHUDCanvas;
    [SerializeField] SkillChargeProgress SkillChargeDisplay;
    [SerializeField] List<ResourceDisplay> ResourceDisplay;
    [SerializeField] Text TargetLevelText;
    [SerializeField] Text TargetNameText;

    void Awake()
    {
        SkillChargeDisplay.gameObject.SetActive(false);
        TargetHUDCanvas.SetActive(false);
    }

    public void SelectTarget(EntityCanvas entityCanvas)
    {
        entityCanvas.AddSkillChargeDisplay(SkillChargeDisplay);
        foreach (var display in ResourceDisplay)
        {
            entityCanvas.AddResourceDisplay(display);
        }

        entityCanvas.SetEntityNameText(TargetNameText);
        entityCanvas.SetEntityLevelText(TargetLevelText);

        TargetHUDCanvas.SetActive(true);
    }

    public void ClearSelection(EntityCanvas entityCanvas)
    {
        entityCanvas.RemoveSkillChargeDisplay(SkillChargeDisplay);
        foreach (var display in ResourceDisplay)
        {
            entityCanvas.RemoveResourceDisplay(display);
        }

        TargetHUDCanvas.SetActive(false);
    }
}
