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

        public float GetAggroChange(ValueInfo valueInfo, float multiplier = 1.0f)
        {
            if (Change == null || Change.Components.Count < 1 || valueInfo == null || valueInfo.Target == null || 
                valueInfo.Target.Entity == null || valueInfo.Caster == null || valueInfo.Caster.Entity == null)
            {
                return 0.0f;
            }

            var change = Change.CalculateValue(valueInfo);

            if (ChangeMultiplier == eAggroChangeMultiplier.CurrentAggro)
            {
                change *= valueInfo.Target.Entity.EntityBattle.GetAggro(valueInfo.Caster.UID);
            }
            else if (ChangeMultiplier == eAggroChangeMultiplier.MaxAggro)
            {
                change *= BattleData.Aggro.MaxAggro;
            }

            return change * multiplier;
        }

        public AggroChange()
        {
            Change = new Value(false);
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
