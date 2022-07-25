using System.Collections.Generic;
using UnityEngine;

using PayloadMultiplierSet = System.Collections.Generic.List<PayloadMultiplier>; // Multiplier attributes in a set are added together

public class MultiplierAttributeData
{
    public class PayloadMultiplierData
    {
        // Set = 1 + mult + mult + mult +....
        // The sets are multiplied together.
        public List<PayloadMultiplierSet> OutgoingMultipliers;
        public List<PayloadMultiplierSet> IncomingMultipliers;

        public PayloadMultiplierData()
        {
            OutgoingMultipliers = new List<PayloadMultiplierSet>();
            IncomingMultipliers = new List<PayloadMultiplierSet>();
        }

        public float GetMultiplier(Payload payloadInfo, List<string> payloadFlags, List<string> resultFlags)
        {
            var multiplier = 1.0f;
            var resultFlag = "";

            foreach (var set in OutgoingMultipliers)
            {
                var setMult = 1.0f;

                foreach (var m in set)
                {
                    setMult += m.GetMultiplier(payloadInfo.Caster, payloadFlags, payloadInfo.PayloadData.Categories, ref resultFlag);

                    if (!string.IsNullOrEmpty(resultFlag))
                    {
                        resultFlags.Add(resultFlag);
                    }
                }

                multiplier *= setMult;
            }

            foreach (var set in IncomingMultipliers)
            {
                var setMult = 1.0f;

                foreach (var m in set)
                {
                    setMult += m.GetMultiplier(payloadInfo.Target, payloadFlags, payloadInfo.PayloadData.Categories, ref resultFlag);

                    if (!string.IsNullOrEmpty(resultFlag))
                    {
                        resultFlags.Add(resultFlag);
                    }
                }

                multiplier *= setMult;
            }

            return multiplier;
        }
    }

    public PayloadMultiplierData DamageMultipliers;
    public PayloadMultiplierData RecoveryMultipliers;

    public List<MultiplierAttribute> InterruptResistanceMultipliers;

    public List<MultiplierAttribute> MovementSpeedMultipliers;
    public List<MultiplierAttribute> RotationSpeedMultipliers;
    public List<MultiplierAttribute> JumpHeightMultipliers;

    public MultiplierAttributeData()
    {
        DamageMultipliers = new PayloadMultiplierData();
        RecoveryMultipliers = new PayloadMultiplierData();

        InterruptResistanceMultipliers = new List<MultiplierAttribute>();

        MovementSpeedMultipliers = new List<MultiplierAttribute>();
        RotationSpeedMultipliers = new List<MultiplierAttribute>();
        JumpHeightMultipliers = new List<MultiplierAttribute>();
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

public class PayloadMultiplier : MultiplierAttribute
{
    public string SuccessFlag;
    public string PayloadCategoryRequired;
    public List<string> PayloadFlagsRequired;

    public float GetMultiplier(EntityInfo entity, List<string> payloadFlags, List<string> payloadCategories, ref string resultFlag)
    {
        resultFlag = "";

        if (entity.Attributes != null && entity.Attributes.ContainsKey(Attribute))
        {
            // Category
            if (!string.IsNullOrEmpty(PayloadCategoryRequired))
            {
                if (payloadCategories == null || !payloadCategories.Contains(PayloadCategoryRequired))
                {
                    return 0.0f;
                }
            }

            // Chance.
            if (!string.IsNullOrEmpty(ChanceAttribute))
            {
                if (!entity.Attributes.ContainsKey(ChanceAttribute))
                {
                    return 0.0f;
                }

                var chance = entity.Attribute(ChanceAttribute);
                if (chance < Random.value)
                {
                    return 0.0f;
                }
            }

            // Payload flags.
            if (PayloadFlagsRequired != null && PayloadFlagsRequired.Count > 0)
            {
                if (payloadFlags == null)
                {
                    return 0.0f;
                }

                foreach (var flag in PayloadFlagsRequired)
                {
                    if (!payloadFlags.Contains(flag))
                    {
                        return 0.0f;
                    }
                }
            }

            // Set success flag and return the multiplier attribute.
            resultFlag = SuccessFlag;
            return entity.Attribute(Attribute);
        }

        return 0.0f;
    }

    public PayloadMultiplier()
    {
        SuccessFlag = "";
        PayloadFlagsRequired = new List<string>();
    }

    public PayloadMultiplier(string attribute) : base()
    {
        Attribute = attribute;
    }
}