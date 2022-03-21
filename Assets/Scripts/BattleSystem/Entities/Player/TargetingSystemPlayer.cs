using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetingSystemPlayer : TargetingSystem
{
    HUDTarget TargetHUD; 

    MovementEntity Player;
    float PlayerLastMoved;

    public override void Setup(Entity parent)
    {
        TargetHUD = FindObjectOfType<HUDTarget>();
        if (TargetHUD == null)
        {
            Debug.LogError("TargetHUD could not be found");
        }

        base.Setup(parent);

        Player = Parent.GetComponentInChildren<MovementEntity>();
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

        if (entity != null && entity.EntityCanvas != null)
        {
                TargetHUD.ClearSelection(entity.EntityCanvas);
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
            SelectTarget(GetBestEnemy());
            PlayerLastMoved = Player.LastMoved;
        }
    }
}
