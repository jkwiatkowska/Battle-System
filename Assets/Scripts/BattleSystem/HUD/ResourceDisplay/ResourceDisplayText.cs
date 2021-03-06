using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceDisplayText : ResourceDisplay
{
    [SerializeField] Text Text;

    public override void SetValues(float current, float max)
    {
        Text.text = $"{Mathf.Floor(current)} / {Mathf.Floor(max)} {NamesAndText.ResourceName(ResourceName)}";
    }

    public override void UpdateValues(float current, float max)
    {
        SetValues(current, max);
    }
}