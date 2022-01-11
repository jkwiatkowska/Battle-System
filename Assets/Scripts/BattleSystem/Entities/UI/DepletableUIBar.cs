using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DepletableUIBar : DepletableUI
{
    [SerializeField] private Image FastBar;
    [SerializeField] private Image SlowBar;
    [SerializeField] private float UpdateSpeed;

    public float currentFill = 0;
    public float fillGoal = 0;

    public void SetFill(float fillRatio, bool instant = false)
    {
        fillGoal = fillRatio;
        FastBar.fillAmount = fillRatio;
        if (instant)
        {
            SlowBar.fillAmount = fillRatio;
            currentFill = SlowBar.fillAmount;
        }
    }

    private void Update()
    {
        if (currentFill != fillGoal)
        {
            SlowBar.fillAmount = Mathf.Lerp(currentFill, fillGoal, UpdateSpeed);
        }
        currentFill = SlowBar.fillAmount;
    }

    public override void UpdateValues(float current, float max)
    {
        SetFill(current / max);
    }
}
