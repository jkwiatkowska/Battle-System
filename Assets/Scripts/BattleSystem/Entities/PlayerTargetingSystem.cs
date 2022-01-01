using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTargetingSystem : TargetingSystem
{
    [SerializeField] float DistanceWeight = 0.5f;
    [SerializeField] float AngleWeight = 0.5f;
    [SerializeField] float InputCooldown = 0.25f;
    [SerializeField] float MaxDistance = 50.0f;
    Vector3 ParentLastPosition;

    float InputCd = 0.0f;

    PlayerController Player;
    float PlayerLastMoved;

    protected override void Awake()
    {
        base.Awake();

        Player = Parent.GetComponent<PlayerController>();
        if (Player == null)
        {
            Debug.LogError("Player entity does not have a player controller component.");
        }
    }

    private void Update()
    {
        InputCd -= Time.deltaTime;

        if (InputCd <= 0.0f && Input.GetKey(KeyCode.Tab))
        {
            if (Player.LastMoved == PlayerLastMoved)
            {
                Debug.Log("Selecting next");
                SelectNextEnemy();
            }
            else
            {
                Debug.Log("Selecting best");
                SelectBestEnemy();
                PlayerLastMoved = Player.LastMoved;
            }
            InputCd = InputCooldown;
        }
    }

    protected override void ProcessEnemyEntityList()
    {
        var scores = new Dictionary<string, float>();

        foreach (var target in EnemyEntities)
        {
            scores.Add(target.Entity.EntityUID, GetTargetScore(target));
        }

        EnemyEntities.Sort((e1, e2) => scores[e2.Entity.EntityUID].CompareTo(scores[e1.Entity.EntityUID]));

        ParentLastPosition = Parent.transform.position;
    }

    protected override void ProcessFriendlyEntityList()
    {

    }

    float GetTargetScore(Targetable target)
    {
        var v = Parent.transform.position - target.transform.position;
        var distanceScore = 1.0f - v.sqrMagnitude / MaxDistance;

        var angleScore = 1.0f - (Mathf.Abs(Vector3.Angle(Parent.transform.forward, v) - 180.0f)/180.0f);

        return DistanceWeight * distanceScore + AngleWeight + angleScore;
    }
}
