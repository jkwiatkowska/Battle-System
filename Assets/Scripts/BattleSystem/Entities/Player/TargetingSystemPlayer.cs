using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetingSystemPlayer : TargetingSystem
{
    HUDTarget TargetHUD; 
    [SerializeField] float DistanceWeight = 0.5f;
    [SerializeField] float AngleWeight = 0.5f;
    [SerializeField] float MaxDistance = 100.0f;

    EntityMovement Player;
    float PlayerLastMoved;

    public override void Setup(Entity parent)
    {
        TargetHUD = FindObjectOfType<HUDTarget>();
        if (TargetHUD == null)
        {
            Debug.LogError("TargetHUD could not be found");
        }

        base.Setup(parent);

        Player = Parent.GetComponentInChildren<EntityMovement>();
        if (Player == null)
        {
            Debug.LogError("Player entity does not have a player controller component.");
        }
    }

    public override void SelectTarget(Entity entity)
    {
        if (!entity.Targetable)
        {
            return;
        }

        base.SelectTarget(entity);

        SelectedTarget.Targetable.ToggleSelect(true);

        var entityCanvas = entity.GetComponentInChildren<EntityCanvas>();
        if (entityCanvas != null)
        {
            TargetHUD.SelectTarget(entityCanvas);
        }
        else
        {
            Debug.LogError($"Entity {entity.EntityUID} is missing EntityCanvas.");
        }
    }

    public void SelectWithMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(UnityEngine.Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100))
        {
            var target = hit.transform.gameObject.GetComponent<Entity>();
            if (target != null && target.IsTargetable)
            {
                SelectTarget(target);
            }
            else
            {
                ClearSelection();
            }

            Debug.Log(hit.transform.gameObject.name);
        }
    }

    public override void ClearSelection(bool selfOnly = false)
    {
        var entity = SelectedTarget;

        if (SelectedTarget != null)
        {
            SelectedTarget.Targetable.ToggleSelect(false);
        }

        base.ClearSelection(selfOnly);

        if (entity != null)
        {
            if (entity.EntityCanvas != null)
            {
                TargetHUD.ClearSelection(entity.EntityCanvas);
            }
            else
            {
                Debug.LogError($"Entity {entity.EntityUID} is missing EntityCanvas.");
            }
        }
    }

    public void SwitchTarget()
    {
        if (Player.LastMoved == PlayerLastMoved)
        {
            base.SelectNextEnemy();
        }
        else
        {
            base.SelectBestEnemy();
            PlayerLastMoved = Player.LastMoved;
        }
    }

    protected override void ProcessEnemyEntityList()
    {
        var scores = new Dictionary<string, float>();

        foreach (var target in EnemyEntities)
        {
            scores.Add(target.EntityUID, GetTargetScore(target));
        }

        EnemyEntities.Sort((e1, e2) => scores[e2.EntityUID].CompareTo(scores[e1.EntityUID]));
    }

    protected override void ProcessFriendlyEntityList()
    {

    }

    float GetTargetScore(Entity target)
    {
        if (!target.Alive)
        {
            return 0;
        }

        var v = Parent.transform.position - target.transform.position;
        var distanceScore = 1.0f - v.sqrMagnitude / MaxDistance;

        var angleScore = 1.0f - (Mathf.Abs(Vector3.Angle(Parent.transform.forward, v) - 180.0f)/180.0f);

        return DistanceWeight * distanceScore + AngleWeight * angleScore;
    }
}
