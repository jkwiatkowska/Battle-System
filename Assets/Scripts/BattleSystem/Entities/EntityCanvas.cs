using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EntityCanvas : MonoBehaviour
{
    Entity Entity;
    [SerializeField] List<Text> EntityNameText;
    [SerializeField] List<Text> EntityLevelText;
    [SerializeField] List<DepletableDisplay> DepletableDisplays;
    Dictionary<string, List<DepletableDisplay>> DepletableDisplay;
    Camera Camera;
    
    public void Setup(Entity entity)
    {
        Entity = entity;
        Camera = Camera.main;

        DepletableDisplay = new Dictionary<string, List<DepletableDisplay>>();
        foreach (var display in DepletableDisplays)
        {
            AddDepletableDisplay(display);
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

    public void UpdateDepletableDisplay(string depletableName)
    {
        if (DepletableDisplay.ContainsKey(depletableName) && DepletableDisplay[depletableName].Count > 0)
        {
            var current = Entity.DepletablesCurrent[depletableName];
            var max = Entity.DepletablesMax[depletableName];

            foreach (var display in DepletableDisplay[depletableName])
            {
                display.UpdateValues(current, max);
            }
        }
    }

    public void AddDepletableDisplay(DepletableDisplay display)
    {
        if (!DepletableDisplay.ContainsKey(display.DepletableName))
        {
            DepletableDisplay.Add(display.DepletableName, new List<DepletableDisplay>());
        }
        DepletableDisplay[display.DepletableName].Add(display);

        var current = Entity.DepletablesCurrent[display.DepletableName];
        var max = Entity.DepletablesMax[display.DepletableName];

        display.SetValues(current, max);
    }

    public void RemoveDepletableDisplay(DepletableDisplay display)
    {
        DepletableDisplay[display.DepletableName].Remove(display);
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
