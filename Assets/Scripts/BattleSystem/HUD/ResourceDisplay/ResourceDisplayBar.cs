using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceDisplayBar : ResourceDisplay
{
    [SerializeField] private Image FastBar;
    [SerializeField] private Image SlowBar;
    [SerializeField] private float UpdateSpeed;

    float FillGoal = 0;

    public void SetFill(float fillRatio, bool instant = false)
    {
        FillGoal = fillRatio;
        FastBar.fillAmount = fillRatio;
        if (instant)
        {
            SlowBar.fillAmount = fillRatio;
        }
    }

    private void Update()
    {
        if (SlowBar.fillAmount != FillGoal)
        {
            SlowBar.fillAmount = Mathf.Lerp(SlowBar.fillAmount, FillGoal, UpdateSpeed);
        }
    }

    public override void SetValues(float current, float max)
    {
        SetFill(current / max, true);
    }

    public override void UpdateValues(float current, float max)
    {
        SetFill(current / max);
    }
}
