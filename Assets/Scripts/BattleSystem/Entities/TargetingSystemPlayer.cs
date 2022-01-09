using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetingSystemPlayer : TargetingSystem
{
    [SerializeField] float DistanceWeight = 0.5f;
    [SerializeField] float AngleWeight = 0.5f;
    [SerializeField] float InputCooldown = 0.25f;
    [SerializeField] float MaxDistance = 100.0f;

    float InputCd = 0.0f;

    EntityMovement Player;
    float PlayerLastMoved;

    public override void Setup(Entity parent)
    {
        base.Setup(parent);

        Player = Parent.GetComponent<EntityMovement>();
        if (Player == null)
        {
            Debug.LogError("Player entity does not have a player controller component.");
        }
    }

    public void SwitchTarget()
    {
        if (InputCd > 0.0f)
        {
            return;
        }

        if (Player.LastMoved == PlayerLastMoved)
        {
            Debug.Log("Selecting next");
            base.SelectNextEnemy();
        }
        else
        {
            Debug.Log("Selecting best");
            base.SelectBestEnemy();
            PlayerLastMoved = Player.LastMoved;
        }

        InputCd = InputCooldown;
    }

    public void SelectWithMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(UnityEngine.Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100))
        {
            var target = hit.transform.gameObject.GetComponent<Targetable>();
            if (target != null)
            {
                base.SelectTarget(target);
            }
            else
            {
                base.ClearSelection();
            }

            Debug.Log(hit.transform.gameObject.name);

            InputCd = InputCooldown;
        }
    }

    protected override void Update()
    {
        base.UpdateEntityLists();
        InputCd -= Time.deltaTime;
    }

    protected override void ProcessEnemyEntityList()
    {
        var scores = new Dictionary<string, float>();

        foreach (var target in EnemyEntities)
        {
            scores.Add(target.Entity.EntityUID, GetTargetScore(target));
        }

        EnemyEntities.Sort((e1, e2) => scores[e2.Entity.EntityUID].CompareTo(scores[e1.Entity.EntityUID]));
    }

    protected override void ProcessFriendlyEntityList()
    {

    }

    float GetTargetScore(Targetable target)
    {
        var v = Parent.transform.position - target.transform.position;
        var distanceScore = 1.0f - v.sqrMagnitude / MaxDistance;

        var angleScore = 1.0f - (Mathf.Abs(Vector3.Angle(Parent.transform.forward, v) - 180.0f)/180.0f);

        return DistanceWeight * distanceScore + AngleWeight * angleScore;
    }
}
