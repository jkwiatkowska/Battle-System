using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetingSystem : MonoBehaviour
{
    protected Entity Parent;
    public Targetable SelectedTarget { get; protected set; }

    public List<Targetable> EnemyEntities { get; protected set; }
    public List<Targetable> FriendlyEntities { get; protected set; }

    public bool FriendlySelected
    {
        get
        {
            if (SelectedTarget == null)
            {
                return false;
            }

            return BattleSystem.IsFriendly(Parent.EntityUID, SelectedTarget.Entity.EntityUID);
        }
    }
    public bool EnemySelected
    {
        get
        {
            if (SelectedTarget == null)
            {
                return false;
            }

            return BattleSystem.IsEnemy(Parent.EntityUID, SelectedTarget.Entity.EntityUID);
        }
    }

    public virtual void Setup(Entity parent)
    {
        Parent = parent;
        EnemyEntities = new List<Targetable>();
        FriendlyEntities = new List<Targetable>();
    }

    protected virtual void Update()
    {

    }

    public virtual void SelectTarget(Targetable entity)
    {
        ClearSelection();
        SelectedTarget = entity;
        SelectedTarget.ToggleSelect(true);
    }

    public virtual void ClearSelection()
    {
        if (SelectedTarget != null)
        {
            SelectedTarget.ToggleSelect(false);
        }
        SelectedTarget = null;
    }

    public virtual bool TrySelectParent()
    {
        if (Parent.EntityData.IsTargetable)
        {
            var target = Parent.GetComponent<Targetable>();
            if (target == null)
            {
                Debug.LogError($"Parent entity {Parent.EntityUID} marked as targetable, but doesn't have a Targetable component.");
            }
            SelectTarget(target);

            return true;
        }

        return false;
    }

    public virtual void SelectBestEnemy()
    {
        UpdateEntityLists();
        if (EnemyEntities.Count == 0)
        {
            return;
        }
        if (EnemyEntities.Count > 1 && SelectedTarget == EnemyEntities[0])
        {
            SelectTarget(EnemyEntities[1]);
            return;
        }
        SelectTarget(EnemyEntities[0]);
    }

    public virtual void SelectNextEnemy()
    {
        if (SelectedTarget == null)
        {
            SelectBestEnemy();
            return;
        }
        if (EnemyEntities.Count == 0)
        {
            return;
        }
        if (EnemyEntities.Count > 1)
        {
            var index = EnemyEntities.IndexOf(SelectedTarget) + 1;
            if (index >= EnemyEntities.Count)
            {
                index = 0;
            }
            SelectTarget(EnemyEntities[index]);
            return;
        }
        SelectTarget(EnemyEntities[0]);
    }

    public virtual void SelectBestFriend()
    {
        UpdateEntityLists();
        if (FriendlyEntities.Count == 0)
        {
            TrySelectParent();
            return;
        }
        if (FriendlyEntities.Count > 1 && SelectedTarget == FriendlyEntities[0])
        {
            SelectTarget(FriendlyEntities[1]);
            return;
        }
        SelectTarget(FriendlyEntities[0]);
    }

    public virtual void SelectNextFriend()
    {
        if (SelectedTarget == null)
        {
            SelectBestFriend();
            return;
        }
        if (FriendlyEntities.Count > 1)
        {
            var index = Mathf.Clamp(FriendlyEntities.IndexOf(SelectedTarget) + 1, 0, FriendlyEntities.Count - 1);
            SelectTarget(FriendlyEntities[index]);
            return;
        }
        TrySelectParent();
    }

    public virtual void UpdateEntityLists()
    {
        var allEntities = BattleSystem.Instance.TargetableEntities;

        EnemyEntities = new List<Targetable>();
        FriendlyEntities = new List<Targetable>();

        // Go through all entities and add them to the lists
        foreach(var target in allEntities)
        {
            if (BattleSystem.IsEnemy(Parent.EntityUID, target.Entity.EntityUID))
            {
                EnemyEntities.Add(target);
            }
            else if (BattleSystem.IsFriendly(Parent.EntityUID, target.Entity.EntityUID))
            {
                FriendlyEntities.Add(target);
            }
        }

        // Sort and filter the lists as needed
        ProcessEnemyEntityList();
        ProcessFriendlyEntityList();
    }

    protected virtual void ProcessEnemyEntityList()
    {

    }

    protected virtual void ProcessFriendlyEntityList()
    {

    }

    public List<Entity> GetAllFriendlyEntites()
    {
        var entities = new List<Entity>();
        foreach (var target in FriendlyEntities)
        {
            entities.Add(target.Entity);
        }
        return entities;
    }

    public List<Entity> GetAllEnemyEntites()
    {
        var entities = new List<Entity>();
        foreach (var target in EnemyEntities)
        {
            entities.Add(target.Entity);
        }
        return entities;
    }
}
