using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EntityUI : MonoBehaviour
{
    Entity Entity;
    [SerializeField] List<Text> EntityNameText;
    [SerializeField] List<Text> EntityLevelText;
    [SerializeField] List<DepletableUI> DepletableUIs;
    Dictionary<string, List<DepletableUI>> DepletableUI;
    Camera Camera;
    
    public void Setup(Entity entity)
    {
        Entity = entity;
        Camera = Camera.main;

        DepletableUI = new Dictionary<string, List<DepletableUI>>();
        foreach (var ui in DepletableUIs)
        {
            AddDepletableUI(ui);
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

    public void UpdateDepletableUI(string depletableName)
    {
        if (DepletableUI.ContainsKey(depletableName) && DepletableUI[depletableName].Count > 1)
        {
            var current = Entity.DepletablesCurrent[depletableName];
            var max = Entity.DepletablesMax[depletableName];

            foreach (var ui in DepletableUI[depletableName])
            {
                ui.UpdateValues(current, max);
            }
        }
    }

    public void AddDepletableUI(DepletableUI ui)
    {
        if (!DepletableUI.ContainsKey(ui.DepletableName))
        {
            DepletableUI.Add(ui.DepletableName, new List<DepletableUI>());
        }
        DepletableUI[ui.DepletableName].Add(ui);

        var current = Entity.DepletablesCurrent[ui.DepletableName];
        var max = Entity.DepletablesMax[ui.DepletableName];

        ui.SetValues(current, max);
    }

    public void RemoveDepletableUI(DepletableUI ui)
    {
        DepletableUI[ui.DepletableName].Remove(ui);
    }

    public void SetEntityNameText(Text text)
    {
        text.text = Names.EntityName(Entity);
    }

    public void SetEntityLevelText(Text text)
    {
        text.text = Names.EntityLevel(Entity);
    }
}
