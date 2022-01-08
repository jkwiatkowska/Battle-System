using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    [SerializeField] string EntityID;
    [SerializeField] Animator Animator;
    [SerializeField] AudioRenderer AudioRenderer;
    public string EntityUID                                 { get; private set; }
    static int EntityCount = 0;
    public Dictionary<string, float> Attributes             { get; private set; }
    public Dictionary<string, float> DepletablesCurrent     { get; private set; }
    public Dictionary<string, float> DepletablesMax         { get; private set; }
    public int EntityLevel                                  { get; private set; }

    protected Dictionary<string, float> SkillAvailableTime;
    protected string CurrentSkill;
    protected Dictionary<string, Coroutine> SkillCoroutines;
    protected SkillChargeData SkillCharge                   { get; private set; }
    protected float SkillChargeStartTime                    { get; private set; }
    protected float SkillChargeRatio                        { get; private set; }

    [SerializeField] protected TargetingSystem TargetingSystem;
    public Dictionary<string, Entity> TaggedEntities        { get; private set; }

    public string FactionOverride                           { get; private set; }

    public float TimeOfLastAttack                           { get; private set;}

    public EntityData EntityData
    {
        get
        {
            return GameData.GetEntityData(EntityID);
        }
    }

    public FactionData FactionData
    {
        get
        {
            return GameData.GetFactionData(EntityID);
        }
    }

    public virtual void Setup(string entityID, int entityLevel)
    {
        EntityID = entityID;
        EntityUID = entityID + EntityCount.ToString();
        EntityCount++;
        EntityLevel = entityLevel;

        Attributes = EntityData.GetAttributesForLevel(entityLevel);
        DepletablesCurrent = EntityData.GetStartingDepletablesForLevel(entityLevel);
        DepletablesMax = EntityData.GetMaxDepletablesForLevel(entityLevel);

        SkillAvailableTime = new Dictionary<string, float>();

        BattleSystem.Instance.AddEntity(this);

        TargetingSystem.Setup(this);

        name = EntityUID;
    }

    protected virtual void Start()
    {
        Setup(EntityID, EntityLevel);
    }

    protected virtual void Update()
    {
        var isInCombat = IsInCombat();

        if (DepletablesCurrent != null)
        {
            foreach (var depletable in DepletablesCurrent)
            {
                if (isInCombat)
                {
                    var recovery = EntityData.DepletableRecovery[depletable.Key].x * Time.deltaTime;
                    if (recovery != 0.0f)
                    {
                        UpdateDepletable(depletable.Key, recovery);
                    }
                }
                else
                {
                    var recovery = EntityData.DepletableRecovery[depletable.Key].y * Time.deltaTime;
                    if (recovery != 0.0f)
                    {
                        UpdateDepletable(depletable.Key, recovery);
                    }
                }
            }
        }
    }

    #region Skills
    public virtual void UseSkill(string skillID)
    {
        var skillData = GameData.GetSkillData(skillID);

        if (!skillData.ParallelSkill)
        {
            CancelCurrentSkill();
            CurrentSkill = skillID;
        }

        if (skillData.NeedsTarget)
        {
            var hasTarget = TargetingSystem.EnemySelected;

            if (!hasTarget)
            {
                // Ensure target is close enough and turn toward it. 
            }
        }

        if (skillData.HasChargeTime)
        {
            SkillChargeStart(skillData.SkillChargeData);
        }
        else
        {
            StartSkillCoroutine(skillData);
        }
    }

    protected virtual void StartSkillCoroutine(SkillData skillData)
    {
        var skillCoroutine = StartCoroutine(UseSkillCoroutine(skillData));
        SkillCoroutines.Add(skillData.SkillID, skillCoroutine);
    }

    public virtual IEnumerator PreSkillChargeCoroutine(SkillData skillData)
    {
        var startTime = BattleSystem.TimeSinceStart;
        var actionResults = new Dictionary<string, ActionResult>();

        foreach (var action in skillData.SkillChargeData.PreChargeTimeline)
        {
            var timeBeforeAction = startTime + action.Timestamp - BattleSystem.TimeSinceStart;
            if (timeBeforeAction > 0.0f)
            {
                yield return new WaitForSeconds(timeBeforeAction);
            }

            ExecuteSkillAction(action, ref actionResults);
        }
        yield return null;
    }

    public virtual IEnumerator UseSkillCoroutine(SkillData skillData)
    {
        var startTime = BattleSystem.TimeSinceStart;
        var actionResults = new Dictionary<string, ActionResult>();

        foreach (var action in skillData.SkillTimeline)
        {
            var timeBeforeAction = startTime + action.Timestamp - BattleSystem.TimeSinceStart;
            if (timeBeforeAction > 0.0f)
            {
                yield return new WaitForSeconds(timeBeforeAction);
            }

            ExecuteSkillAction(action, ref actionResults);
        }

        yield return null;
    }

    protected virtual void ExecuteSkillAction(Action actionData, ref Dictionary<string, ActionResult> actionResults)
    {
        var actionResult = new ActionResult(actionData);
        bool executeAction = false;

        switch(actionData.ExecuteCondition)
        {
            case Action.eActionCondition.AlwaysExecute:
            {
                executeAction = true;
                break;
            }
            case Action.eActionCondition.OnActionSuccess:
            {
                if (actionResults.ContainsKey(actionData.ConditionActionID))
                {
                    executeAction = actionResults[actionData.ConditionActionID].Success;
                }
                else
                {
                    Debug.LogError($"Action {actionData.ActionID} requires action {actionData.ConditionActionID} to execute first, but its result could not be found.");
                }
                break;
            }
            case Action.eActionCondition.OnActionFail:
            {
                if (actionResults.ContainsKey(actionData.ConditionActionID))
                {
                    executeAction = !actionResults[actionData.ConditionActionID].Success;
                }
                else
                {
                    Debug.LogError($"Action {actionData.ActionID} requires action {actionData.ConditionActionID} to execute first, but its result could not be found.");
                }
                break;
            }
            case Action.eActionCondition.OnMinChargeRatio:
            {
                executeAction = actionData.MinChargeRatio <= SkillChargeRatio;
                break;
            }
            default:
            {
                Debug.LogError($"Unsupported skill action condition: {actionData.ExecuteCondition}");
                break;
            }
        }

        if (executeAction)
        {
            switch (actionData.ActionType)
            {
                case Action.eActionType.PayloadArea:
                {
                    // Get targets in area
                    // Apply payload to targets
                    // Update result with damage dealt
                    break;
                }
                case Action.eActionType.PayloadDirect:
                {
                    // Get affected targets
                    // Apply payload to targets
                    // Update result with damage dealt
                    break;
                }
                case Action.eActionType.SpawnProjectile:
                {
                    // Create a projectile entity and set it up with data
                    break;
                }
                case Action.eActionType.SpawnEntity:
                {
                    // Create an entity and set it up with data
                    break;
                }
                case Action.eActionType.TriggerAnimation:
                {
                    break;
                }
                // Also needs an audio action and a movement action
                default:
                {
                    Debug.LogError($"Unsupported skill action type: {actionData.ActionType}");
                    break;
                }
            }
        }

        actionResults.Add(actionData.ActionID, actionResult);
    }

    protected virtual List<Entity> GetTargetsForDirectAction(ActionPayloadDirect action)
    {
        var targets = new List<Entity>();

        switch (action.SkillTargets)
        {
            case ActionPayloadDirect.eDirectSkillTargets.SelectedEntity:
            {
                if (action.Target == ActionPayload.eTarget.FriendlyEntities)
                {
                    if (TargetingSystem.FriendlySelected)
                    {
                        targets.Add(TargetingSystem.SelectedTarget.Entity);
                    }
                    else
                    {
                        targets.Add(this);
                    }
                }
                else if (action.Target == ActionPayload.eTarget.EnemyEntities)
                {
                    if (TargetingSystem.EnemySelected)
                    {
                        targets.Add(TargetingSystem.SelectedTarget.Entity);
                    }
                    else
                    {
                        Debug.LogError($"Attempting to execute skill action {action.ActionID}, but no enemy target is selected.");
                    }
                }
                break;
            }
            case ActionPayloadDirect.eDirectSkillTargets.AllEntities:
            {
                if (action.Target == ActionPayload.eTarget.FriendlyEntities)
                {
                    targets = TargetingSystem.GetAllFriendlyEntites();
                }
                else if (action.Target == ActionPayload.eTarget.EnemyEntities)
                {
                    targets = TargetingSystem.GetAllEnemyEntites();
                }
                break;
            }
            case ActionPayloadDirect.eDirectSkillTargets.RandomEntities:
            {
                if (action.Target == ActionPayload.eTarget.FriendlyEntities)
                {
                    targets = TargetingSystem.GetAllFriendlyEntites();
                }
                else if (action.Target == ActionPayload.eTarget.EnemyEntities)
                {
                    targets = TargetingSystem.GetAllEnemyEntites();
                }
                // Randomly remove entities from list until the desired number is left
                while (targets.Count > action.TargetCount)
                {
                    targets.RemoveAt(Random.Range(0, targets.Count));
                }
                break;
            }
            case ActionPayloadDirect.eDirectSkillTargets.TaggedEntity:
            {

                break;
            }
        }

        // Single target skill
        if (action.SkillTargets == ActionPayloadDirect.eDirectSkillTargets.SelectedEntity)
        {
            // Friendly entity. Select self if selected entity is not friendly.
            if (action.Target == ActionPayload.eTarget.FriendlyEntities)
            {
                if (TargetingSystem.FriendlySelected)
                {
                    targets.Add(TargetingSystem.SelectedTarget.Entity);
                }
                else
                {
                    targets.Add(this);
                }
            }
            else if (action.Target == ActionPayload.eTarget.EnemyEntities)
            {
                if (TargetingSystem.EnemySelected)
                {
                    targets.Add(TargetingSystem.SelectedTarget.Entity);
                }
            }
        }
        else if (action.SkillTargets == ActionPayloadDirect.eDirectSkillTargets.AllEntities ||
                 action.SkillTargets == ActionPayloadDirect.eDirectSkillTargets.RandomEntities)
        {
            // Get all targets.
            if (action.Target == ActionPayload.eTarget.FriendlyEntities)
            {
                foreach (var target in TargetingSystem.FriendlyEntities)
                {
                    targets.Add(target.Entity);
                }
            }
            else if (action.Target == ActionPayload.eTarget.EnemyEntities)
            {
                foreach (var target in TargetingSystem.EnemyEntities)
                {
                    targets.Add(target.Entity);
                }
            }

            // Randomly remove targets until the desired amount is left.
            if (action.SkillTargets == ActionPayloadDirect.eDirectSkillTargets.RandomEntities)
            {
                while (targets.Count > action.TargetCount)
                {
                    targets.RemoveAt(Random.Range(0, targets.Count));
                }
            }
        }
        else if (action.SkillTargets == ActionPayloadDirect.eDirectSkillTargets.TaggedEntity)
        {
            if (TaggedEntities.ContainsKey(action.EntityTag) && TaggedEntities[action.EntityTag] != null)
            {
                targets.Add(TaggedEntities[action.EntityTag]);
            }
        }

        return targets;
    }

    protected virtual List<Entity> GetTargetsForAreaAction(ActionPayloadArea action)
    {
        var targets = new List<Entity>();

        var potentialTargets = new List<Entity>();
        switch (action.Target)
        {
            case ActionPayload.eTarget.EnemyEntities:
            {
                potentialTargets = TargetingSystem.GetAllEnemyEntites();
                break;
            }
            case ActionPayload.eTarget.FriendlyEntities:
            {
                potentialTargets = TargetingSystem.GetAllFriendlyEntites();
                break;
            }
            default:
            {
                Debug.LogError($"{action.Target} target type not supported by area actions.");
                break;
            }
        }

        foreach(var area in action.AreasAffected)
        {
            var foundPosition = GetPosition(area.AreaPosition, out Vector2 areaPosition, out Vector2 areaForward);
            if (!foundPosition)
            {
                continue;
            }

            switch (area.Shape)
            {
                case ActionPayloadArea.Area.eShape.Cone:
                {
                    var minDistance = area.InnerDimensions.x * area.InnerDimensions.x;
                    var maxDistance = area.Dimensions.x * area.Dimensions.x;

                    var minAngle = area.InnerDimensions.y;
                    var maxAngle = area.InnerDimensions.x;

                    for (int i = potentialTargets.Count - 1; i >= 0; i--)
                    {
                        var target = potentialTargets[i];

                        // Check if the target is inside circle
                        var distance = Vector2.SqrMagnitude(areaPosition - Utility.Get2DPosition(target.transform.position));
                        if (distance < minDistance || distance > maxDistance)
                        {
                            continue;
                        }

                        // Check if the target is inside cone
                        if (minAngle > 0.0f || maxAngle < 360.0f) // If not a circle
                        {

                        }

                        targets.Add(target);
                        potentialTargets.Remove(target);
                    }
                    break;
                }
                case ActionPayloadArea.Area.eShape.Rectangle:
                {
                    foreach (var target in potentialTargets)
                    {

                    }
                    break;
                }
            }
        }

        return targets;
    }

    public virtual bool GetPosition(PositionData positionData, out Vector2 position, out Vector2 forward)
    {
        position = new Vector2();
        forward = new Vector2();

        switch (positionData.PositionOrigin)
        {
            case PositionData.ePositionOrigin.WorldPosition:
            {
                position = Vector2.zero;
                forward = Vector2.zero;
                break;
            }
            case PositionData.ePositionOrigin.CasterPosition:
            {
                position = Utility.Get2DPosition(transform.position);
                forward = Utility.Get2DPosition(transform.forward);
                break;
            }
            case PositionData.ePositionOrigin.SelectedTargetPosition:
            {
                if (TargetingSystem.SelectedTarget == null)
                {
                    Debug.LogError($"Area action requires a target, but none is selected.");
                }
                position = Utility.Get2DPosition(TargetingSystem.SelectedTarget.transform.position);
                forward = Utility.Get2DPosition(TargetingSystem.SelectedTarget.transform.forward);
                break;
            }
            case PositionData.ePositionOrigin.TaggedEntityPosition:
            {
                if (TaggedEntities.ContainsKey(positionData.EntityTag) && TaggedEntities[positionData.EntityTag] != null)
                {
                    position = Utility.Get2DPosition(TaggedEntities[positionData.EntityTag].transform.position);
                    forward = Utility.Get2DPosition(TaggedEntities[positionData.EntityTag].transform.forward);
                }
                else
                {
                    // No tagged entity
                    return false;
                }
                break;
            }
            default:
            {
                Debug.LogError($"Unsupported position origin: {positionData.PositionOrigin}");
                break;
            }
        }

        // Add offset.
        position += positionData.PositionOffset;
        if (positionData.RandomPositionOffset.x != 0.0f)
        {
            position.x += Random.Range(0.0f, positionData.RandomPositionOffset.x);
        }
        if (positionData.RandomPositionOffset.y != 0.0f)
        {
            position.y += Random.Range(0.0f, positionData.RandomPositionOffset.y);
        }

        return true;
    }

    public virtual void SkillChargeStart(SkillChargeData skillChargeData)
    {
        SkillCharge = skillChargeData;
        SkillChargeStartTime = BattleSystem.TimeSinceStart;

        // Show charge UI 
    }

    public virtual void SkillChargeUpdate()
    {
        // Update UI

        if (BattleSystem.TimeSinceStart < SkillChargeStartTime + SkillCharge.FullChargeTime)
        {
            SkillChargeStop();
        }
    }

    public virtual bool SkillChargeStop()
    {
        // Hide UI

        var timeElapsed = BattleSystem.TimeSinceStart - SkillChargeStartTime;
        if (timeElapsed >= SkillCharge.RequiredChargeTime)
        {
            var minCharge = SkillCharge.RequiredChargeTime;
            var maxCharge = SkillCharge.FullChargeTime;
            SkillChargeRatio = maxCharge - minCharge / timeElapsed - minCharge;

            var skillData = GameData.GetSkillData(CurrentSkill);
            var skillCoroutine = StartCoroutine(UseSkillCoroutine(skillData));
            SkillCoroutines[CurrentSkill] = skillCoroutine;

            return true;
        }
        else
        {
            return false;
        }
    }

    public virtual void SkillActionCollectCost(string skillID, ActionCostCollection action, ref ActionResult actionResult)
    {
        var canAfford = CanAffordCost(action);

        if (!canAfford)
        {
            actionResult.Success = false;

            if (!action.Optional)
            {
                CancelSkill(skillID);
            }
        }
    }

    public virtual void SkillActionApplyCooldown(string skillID, ActionCooldownApplication action)
    {
        var availableTime = BattleSystem.TimeSinceStart + action.Cooldown;
        SkillAvailableTime[skillID] = availableTime;
        foreach (var skill in action.SharedCooldown)
        {
            SkillAvailableTime[skill] = availableTime;
        }
    }

    public virtual void CancelCurrentSkill()
    {
        if (!string.IsNullOrEmpty(CurrentSkill))
        {
            CancelSkill(CurrentSkill);

            SkillChargeStop();
        }
    }

    public virtual void CancelSkill(string skillID)
    {
        if (SkillCoroutines.ContainsKey(skillID))
        {
            if (SkillCoroutines[skillID] != null)
            {
                StopCoroutine(SkillCoroutines[skillID]);
            }
            SkillCoroutines.Remove(skillID);
        }
    }
    #endregion

    public virtual void Destroy()
    {
        BattleSystem.Instance.RemoveEntity(EntityUID);
    }

    #region Change Functions
    public void UpdateDepletable(string depletable, float change)
    {
        DepletablesCurrent[depletable] = Mathf.Clamp(DepletablesCurrent[depletable] + change, 0, DepletablesMax[depletable]);
    }
    #endregion

    #region Bool Checks
    public virtual bool IsInCombat()
    {
        return false;
    }

    protected virtual bool IsSkillOnCooldown(string skillID)
    {
        // Not on cooldown if cooldown hasn't been registered
        if (!SkillAvailableTime.ContainsKey(skillID))
        {
            return false;
        }

        return SkillAvailableTime[skillID] > BattleSystem.TimeSinceStart;
    }

    protected virtual bool CanAffordCost(ActionCostCollection costAction)
    {
        return (costAction.GetValue(this) <= DepletablesCurrent[costAction.DepletableName]);
    }

    protected virtual bool CanAffordCost(List<ActionCostCollection> costActions)
    {
        var costs = new Dictionary<string, float>();
        foreach (var cost in costActions)
        {
            if (costs.ContainsKey(cost.DepletableName))
            {
                costs[cost.DepletableName] += cost.GetValue(this);
            }
            else
            {
                costs.Add(cost.DepletableName, cost.GetValue(this));
            }
        }
        foreach (var cost in costs)
        {
            if (cost.Value > DepletablesCurrent[cost.Key])
            {
                return false;
            }
        }
        return true;
    }

    public virtual bool CanUseSkill(string skillID)
    {
        if (IsSkillOnCooldown(skillID))
        {
            return false;
        }

        if (!CanAffordCost(GameData.GetSkillData(skillID).SkillCost))
        {
            return false;
        }

        return true;
    }
    #endregion
}
