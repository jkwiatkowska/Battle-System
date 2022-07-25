using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillChargeProgress : MonoBehaviour
{
    [SerializeField] Image CurrentChargeBar;
    [SerializeField] Image RequiredChargeBar;
    [SerializeField] Text SkillNameText;
    [SerializeField] Text TimeProgressText;
    public float ChargeStartTime    { get; private set; }
    public float FullChargeTime     { get; private set; }

    void Awake()
    {
        gameObject.SetActive(false);
    }

    public void StartCharge(float requiredChargeTime, float fullChargeTime, string skillID, float chargeStartTime)
    {
        ChargeStartTime = chargeStartTime;
        FullChargeTime = fullChargeTime;

        if (RequiredChargeBar != null)
        {
            RequiredChargeBar.fillAmount = requiredChargeTime / FullChargeTime;
        }

        if (SkillNameText != null)
        {
            SkillNameText.text = NamesAndText.SkillName(skillID);
        }

        Update();
    }
    
    void Update()
    {
        var timeElapsed = BattleSystem.Time - ChargeStartTime;
        if (CurrentChargeBar != null)
        {
            CurrentChargeBar.fillAmount = timeElapsed / FullChargeTime;
        }
        else
        {
            Debug.LogError("Skill charge bar is missing.");
        }
        if (TimeProgressText != null)
        {
            TimeProgressText.text = NamesAndText.SkillChargeProgressText(timeElapsed, FullChargeTime);
        }
    }
}
