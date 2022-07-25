using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionSaveValue : Action
{
    public SaveValue ValueToSave;

    public override void Execute(Entity entity, Entity target, ref Dictionary<string, ActionResult> actionResults)
    {
        actionResults[ActionID] = new ActionResult();

        if (!ConditionsMet(entity, target, actionResults))
        {
            return;
        }

        ValueToSave.Save(entity.EntityInfo, target.EntityInfo, actionResults, ActionID);

        actionResults[ActionID].Success = true;
        entity.OnActionUsed(this, actionResults[ActionID], actionResults);
    }

    public override void SetTypeDefaults()
    {
        ValueToSave = new SaveValue();
    }
}

public class SaveValue
{
    public Value Value;
    public string Key;
    public int MaxUses;

    public SaveValue()
    {
        Value = new Value();
        Key = "";
        MaxUses = 0;
    }

    public void Save(ValueInfo valueInfo)
    {
        var value = Value.CalculateValue(valueInfo);

        valueInfo.Caster.Entity.SaveValue(Key, value, MaxUses);
    }

    public void Save(EntityInfo caster, EntityInfo target, Dictionary<string, ActionResult> actionResults, string actionID)
    {
        var valueInfo = new ValueInfo(caster, target, actionResults, actionID);
        Save(valueInfo);
    }
}