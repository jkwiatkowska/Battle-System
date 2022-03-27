using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AggroData
{
    public class AggroChange
    {
        public enum eAggroChangeMultiplier
        {
            NoMultiplier,
            CurrentAggro,
            MaxAggro
        }

        public Value Change;
        public eAggroChangeMultiplier ChangeMultiplier;
        public string MultiplierAttribute;

        public float GetAggroChange(Entity caster, string enmitySourceUID, Entity enmityHolder, float multiplier = 1.0f)
        {
            if (Change == null || Change.Count < 1)
            {
                return 0.0f;
            }

            var change = Change.GetValue(caster, caster.EntityAttributes());

            if (ChangeMultiplier == eAggroChangeMultiplier.CurrentAggro)
            {
                change *= enmityHolder.EntityBattle.GetAggro(enmitySourceUID);
            }
            else if (ChangeMultiplier == eAggroChangeMultiplier.MaxAggro)
            {
                change *= BattleData.Aggro.MaxAggro;
            }

            if (!string.IsNullOrEmpty(MultiplierAttribute) && caster.BaseAttributes.ContainsKey(MultiplierAttribute))
            {
                change *= caster.Attribute(MultiplierAttribute, "", "", "", null);
            }

            return change * multiplier;
        }

        public AggroChange()
        {
            Change = new Value();
        }
    }

    public float MaxAggro;
    public AggroChange AggroChangePerSecond;

    public AggroData()
    {
        MaxAggro = 100.0f;
        AggroChangePerSecond = new AggroChange();
    }
}
