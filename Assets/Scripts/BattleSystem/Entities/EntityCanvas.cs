using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EntityCanvas : MonoBehaviour
{
    Entity Entity;
    [SerializeField] List<Text> EntityNameText;
    [SerializeField] List<Text> EntityLevelText;
    [SerializeField] List<SkillChargeProgress> SkillChargeDisplay;
    [SerializeField] List<ResourceDisplay> ResourceDisplays;
    Dictionary<string, List<ResourceDisplay>> ResourceDisplay;
    Camera Camera;

    public void Setup(Entity entity)
    {
        Entity = entity;
        Camera = Camera.main;

        ResourceDisplay = new Dictionary<string, List<ResourceDisplay>>();
        foreach (var display in ResourceDisplays)
        {
            AddResourceDisplay(display);
        }

        foreach (var text in EntityNameText)
        {
            SetEntityNameText(text);
        }

        foreach (var text in EntityLevelText)
        {
            SetEntityLevelText(text);
        }
    }

    void LateUpdate()
    {
        transform.LookAt(transform.position + Camera.transform.rotation * Vector3.forward, Camera.transform.rotation * Vector3.up);
    }

    public void StartSkillCharge(SkillChargeData skillChargeData, string skillID)
    {
        foreach (var display in SkillChargeDisplay)
        {
            display.gameObject.SetActive(true);
            display.StartCharge(skillChargeData.RequiredChargeTime, skillChargeData.FullChargeTime, skillID);
        }
    }

    public void StopSkillCharge()
    {
        foreach (var display in SkillChargeDisplay)
        {
            display.gameObject.SetActive(false);
        }
    }

    public void AddSkillChargeDisplay(SkillChargeProgress display)
    {
        if (Entity.Skills.SkillState == EntitySkills.eSkillState.SkillCharge)
        {
            var data = Entity.Skills.SkillCharge;
            display.StartCharge(data.RequiredChargeTime, data.FullChargeTime, Entity.Skills.CurrentSkill.SkillID, Entity.Skills.SkillStartTime);
            display.gameObject.SetActive(true);
        }
        SkillChargeDisplay.Add(display);
    }

    public void RemoveSkillChargeDisplay(SkillChargeProgress display)
    {
        display.gameObject.SetActive(false);
        SkillChargeDisplay.Remove(display);
    }

    public void SetupResourceDisplay(string resourceName)
    {
        if (ResourceDisplay.ContainsKey(resourceName) && ResourceDisplay[resourceName].Count > 0)
        {
            var current = Entity.ResourcesCurrent.ContainsKey(resourceName) ? Entity.ResourcesCurrent[resourceName] : 0.0f;
            var max = Entity.ResourcesMax.ContainsKey(resourceName) ? Entity.ResourcesMax[resourceName] : 0.0f;

            foreach (var display in ResourceDisplay[resourceName])
            {
                display.SetValues(current, max);
            }
        }
    }

    public void UpdateResourceDisplay(string resourceName)
    {
        if (ResourceDisplay.ContainsKey(resourceName) && ResourceDisplay[resourceName].Count > 0)
        {
            var current = Entity.ResourcesCurrent.ContainsKey(resourceName) ? Entity.ResourcesCurrent[resourceName] : 0.0f;
            var max = Entity.ResourcesMax.ContainsKey(resourceName) ? Entity.ResourcesMax[resourceName] : 0.0f;

            foreach (var display in ResourceDisplay[resourceName])
            {
                display.UpdateValues(current, max);
            }
        }
    }

    public void AddResourceDisplay(ResourceDisplay display)
    {
        if (!ResourceDisplay.ContainsKey(display.ResourceName))
        {
            ResourceDisplay.Add(display.ResourceName, new List<ResourceDisplay>());
        }
        ResourceDisplay[display.ResourceName].Add(display);

        var current = Entity.ResourcesCurrent.ContainsKey(display.ResourceName) ? Entity.ResourcesCurrent[display.ResourceName] : 0.0f;
        var max = Entity.ResourcesMax.ContainsKey(display.ResourceName) ? Entity.ResourcesMax[display.ResourceName] : 0.0f;

        display.SetValues(current, max);
    }

    public void RemoveResourceDisplay(ResourceDisplay display)
    {
        ResourceDisplay[display.ResourceName].Remove(display);
    }

    public void SetEntityNameText(Text text)
    {
        text.text = NamesAndText.EntityName(Entity);
    }

    public void SetEntityLevelText(Text text)
    {
        text.text = NamesAndText.EntityLevel(Entity);
    }
}
