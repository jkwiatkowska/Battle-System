using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Targetable : MonoBehaviour
{
    Entity ParentEntity;
    [SerializeField] List<GameObject> ShowWhenSelected;
    [SerializeField] List<GameObject> ShowWhenNotSelected;
    public bool Selected { get; private set; }

    Dictionary<string, Entity> TargetedBy;
    public Entity Entity
    {
        get
        {
            return ParentEntity;
        }
        private set
        {
            ParentEntity = value;
        }
    }

    public void Setup(Entity parentEntity)
    {
        ParentEntity = parentEntity;
        TargetedBy = new Dictionary<string, Entity>();
        ToggleSelect(false);
    }

    public void Target(bool targetted, Entity entity)
    {
        if (TargetedBy.ContainsKey(entity.EntityUID) && !targetted)
        {
            TargetedBy.Remove(entity.EntityUID);
        }
        else if (!TargetedBy.ContainsKey(entity.EntityUID) && targetted)
        {
            TargetedBy.Add(entity.EntityUID, entity);
        }
    }

    public void ToggleSelect(bool select)
    {
        Selected = select;

        foreach (var item in ShowWhenSelected)
        {
            item.SetActive(select);
        }

        foreach (var item in ShowWhenNotSelected)
        {
            item.SetActive(!select);
        }
    }

    public void RemoveTargeting()
    {
        foreach (var entity in TargetedBy)
        {
            if (entity.Value != null)
            {
                entity.Value.TargetingSystem.ClearSelection(true);
            }
        }
    }
}
