using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionAnimationSet<T> : Action
{
    public enum eAnimationAction
    {
        SetTrigger,
        SetBool,
        SetFloat,
        SetFloatChargeRatio,
        SetInt
    }

    public eAnimationAction AnimationAction;
    public string Name;
    public bool ValueBool;
    public string ValueOther;
    public override void Execute(Entity entity, out ActionResult actionResult)
    {
        actionResult = new ActionResult();

        if (!ConditionsMet(entity))
        {
            return;
        }
        
        var animator = entity.GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError($"Trying to execute an animation set action {ActionID}, but entity does not have an Animator component.");
            return;
        }

        switch(AnimationAction)
        {
            case eAnimationAction.SetTrigger:
            {
                animator.SetTrigger(Name);
                actionResult.Success = true;
                return;
            }
            case eAnimationAction.SetBool:
            {
                animator.SetBool(Name, ValueBool);
                actionResult.Success = true;
                return;
            }
            case eAnimationAction.SetFloat:
            {
                if (float.TryParse(ValueOther, out var floatValue))
                {
                    animator.SetFloat(Name, floatValue);
                    actionResult.Success = true;
                }
                else
                {
                    Debug.LogError($"Failed to parse animation action value into float.");
                }
                return;
            }
            case eAnimationAction.SetFloatChargeRatio:
            {
                animator.SetFloat(Name, entity.SkillChargeRatio);
                actionResult.Success = true;
                return;
            }
            case eAnimationAction.SetInt:
            {
                if (int.TryParse(ValueOther, out var intValue))
                {
                    animator.SetFloat(Name, intValue);
                    actionResult.Success = true;
                }
                else
                {
                    Debug.LogError($"Failed to parse animation action value into int.");
                }
                return;
            }
            default:
            {
                Debug.LogError($"Unsupported animation action type: {AnimationAction}");
                return;
            }
        }
    }

    public override bool NeedsTarget()
    {
        return false;
    }
}
