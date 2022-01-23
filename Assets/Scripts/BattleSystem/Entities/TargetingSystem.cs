using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetingSystem : MonoBehaviour
{
    public Entity Parent                        { get; protected set; }
    public Entity SelectedTarget                { get; protected set; }

    public List<Entity> EnemyEntities           { get; protected set; }
    public List<Entity> FriendlyEntities        { get; protected set; }
    public virtual List<Entity> AllEntities
    {
        get
        {
            return BattleSystem.TargetableEntities;
        }    
    }

    public bool FriendlySelected
    {
        get
        {
            if (SelectedTarget == null)
            {
                return false;
            }

            return BattleSystem.IsFriendly(Parent.EntityUID, SelectedTarget.EntityUID);
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

            return BattleSystem.IsEnemy(Parent.EntityUID, SelectedTarget.EntityUID);
        }
    }

    public virtual void Setup(Entity parent)
    {
        Parent = parent;
        EnemyEntities = new List<Entity>();
        FriendlyEntities = new List<Entity>();
    }

    protected virtual void Update()
    {

    }

    public virtual void SelectTarget(Entity entity)
    {
        ClearSelection();
        SelectedTarget = entity;
    }

    public virtual void ClearSelection()
    {
        SelectedTarget = null;
    }

    public virtual bool TrySelectParent()
    {
        if (Parent.EntityData.IsTargetable)
        {
            SelectTarget(Parent);

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
        EnemyEntities.Clear();
        FriendlyEntities.Clear();

        // Go through all entities and add them to the lists
        foreach(var target in AllEntities)
        {
            if (BattleSystem.IsEnemy(Parent.EntityUID, target.EntityUID))
            {
                EnemyEntities.Add(target);
            }
            else if (BattleSystem.IsFriendly(Parent.EntityUID, target.EntityUID))
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
}
