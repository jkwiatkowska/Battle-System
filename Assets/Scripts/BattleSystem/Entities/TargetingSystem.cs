using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetingSystem : MonoBehaviour
{
    [SerializeField] protected Targetable Parent;
    public Targetable SelectedTarget { get; protected set; }

    public List<Targetable> EnemyEntities { get; protected set; }
    public List<Targetable> FriendlyEntities { get; protected set; }

    protected virtual void Awake()
    {
        EnemyEntities = new List<Targetable>();
        FriendlyEntities = new List<Targetable>();
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
            SelectTarget(Parent);
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
        SelectTarget(Parent);
    }

    public virtual void SelectTarget(Targetable entity)
    {
        if (SelectedTarget != null)
        {
            SelectedTarget.ToggleSelect(false);
        }
        SelectedTarget = entity;
        SelectedTarget.ToggleSelect(true);
    }

    public virtual void UpdateEntityLists()
    {
        var allEntities = BattleSystem.Instance.TargetableEntities;

        EnemyEntities.Clear();
        FriendlyEntities.Clear();

        // Go through all entities and add them to the lists
        foreach(var target in allEntities)
        {
            if (BattleSystem.IsEnemy(Parent.Entity.EntityUID, target.Entity.EntityUID))
            {
                EnemyEntities.Add(target);
            }
            else if (BattleSystem.IsFriendly(Parent.Entity.EntityUID, target.Entity.EntityUID))
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
