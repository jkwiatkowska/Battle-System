using System.Collections.Generic;
using UnityEngine;

public class MultiplierAttributeData
{
    public List<DamageMultiplierAttribute> DamageDealtMultipliers;
    public List<DamageMultiplierAttribute> DamageReceivedMultipliers;
    public Dictionary<string, List<DamageMultiplierAttribute>> CategoryDamageDealtMultipliers;
    public Dictionary<string, List<DamageMultiplierAttribute>> CategoryDamageReceivedMultipliers;

    public List<MultiplierAttribute> InterruptResistanceMultipliers;

    public List<MultiplierAttribute> MovementSpeedMultipliers;
    public List<MultiplierAttribute> RotationSpeedMultipliers;
    public List<MultiplierAttribute> JumpHeightMultipliers;

    public MultiplierAttributeData()
    {
        DamageDealtMultipliers = new List<DamageMultiplierAttribute>();
        DamageReceivedMultipliers = new List<DamageMultiplierAttribute>();
        CategoryDamageDealtMultipliers = new Dictionary<string, List<DamageMultiplierAttribute>>();
        CategoryDamageReceivedMultipliers = new Dictionary<string, List<DamageMultiplierAttribute>>();

        InterruptResistanceMultipliers = new List<MultiplierAttribute>();

        MovementSpeedMultipliers = new List<MultiplierAttribute>();
        RotationSpeedMultipliers = new List<MultiplierAttribute>();
        JumpHeightMultipliers = new List<MultiplierAttribute>();
    }

    public void ApplyDamageMultipliers(Dictionary<string, float> casterAttributes, Dictionary<string, float> targetAttributes, 
                                       ref float damage, PayloadData payloadData, ref List<string> resultFlags)
    {
        foreach (var multiplier in BattleData.Multipliers.DamageDealtMultipliers)
        {
            var flag = multiplier.ApplyDamageMultiplier(casterAttributes, ref damage, payloadData.Flags);
            if (!string.IsNullOrEmpty(flag))
            {
                resultFlags.Add(flag);
            }
        }

        foreach (var multiplier in BattleData.Multipliers.DamageReceivedMultipliers)
        {
            var flag = multiplier.ApplyDamageMultiplier(targetAttributes, ref damage, payloadData.Flags);
            if (!string.IsNullOrEmpty(flag))
            {
                resultFlags.Add(flag);
            }
        }

        foreach (var category in payloadData.Categories)
        {
            if (BattleData.Multipliers.CategoryDamageDealtMultipliers.ContainsKey(category))
            {
                foreach (var multiplier in BattleData.Multipliers.CategoryDamageDealtMultipliers[category])
                {
                    var flag = multiplier.ApplyDamageMultiplier(casterAttributes, ref damage, payloadData.Flags);
                    if (!string.IsNullOrEmpty(flag))
                    {
                        resultFlags.Add(flag);
                    }
                }

                foreach (var multiplier in BattleData.Multipliers.CategoryDamageReceivedMultipliers[category])
                {
                    var flag = multiplier.ApplyDamageMultiplier(targetAttributes, ref damage, payloadData.Flags);
                    if (!string.IsNullOrEmpty(flag))
                    {
                        resultFlags.Add(flag);
                    }
                }
            }
        }
    }
}

public class MultiplierAttribute
{
    public string Attribute;
    public string ChanceAttribute;

    public void ApplyMultiplier(Entity entity, ref float currentValue)
    {
        if (entity != null)
        {
            if (!string.IsNullOrEmpty(ChanceAttribute))
            {
                var chanceAttribute = entity.Attribute(ChanceAttribute);
                if (chanceAttribute > Random.value)
                {
                    return;
                }
            }
            currentValue *= entity.Attribute(Attribute, defaultValue: 1.0f);
        }
    }
    
    public MultiplierAttribute()
    {
        
    }

    public MultiplierAttribute(string attribute) : base()
    {
        Attribute = attribute;
    }
}

public class DamageMultiplierAttribute : MultiplierAttribute
{
    public string SuccessFlag;
    public List<string> PayloadFlagsRequired;

    public string ApplyDamageMultiplier(Dictionary<string, float> entityAttributes, ref float currentValue, List<string> payloadFlags)
    {
        if (entityAttributes != null)
        {
            // Chance.
            if (!string.IsNullOrEmpty(ChanceAttribute))
            {
                if (!entityAttributes.ContainsKey(ChanceAttribute))
                {
                    return "";
                }

                var chance = entityAttributes[ChanceAttribute];
                if (chance < Random.value)
                {
                    return "";
                }
            }

            // Payload flags
            if (PayloadFlagsRequired != null)
            {
                if (payloadFlags == null)
                {
                    return "";
                }

                foreach (var flag in PayloadFlagsRequired)
                {
                    if (!payloadFlags.Contains(flag))
                    {
                        return "";
                    }
                }
            }

            // Apply attribute.
            if (entityAttributes.ContainsKey(Attribute))
            {
                currentValue *= entityAttributes[Attribute];
                return SuccessFlag;
            }
        }
        return "";
    }

    public DamageMultiplierAttribute()
    {
        SuccessFlag = "";
        PayloadFlagsRequired = new List<string>();
    }

    public DamageMultiplierAttribute(string attribute) : base()
    {
        Attribute = attribute;
    }
}