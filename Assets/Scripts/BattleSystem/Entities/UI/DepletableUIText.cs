using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DepletableUIText : DepletableUI
{
    [SerializeField] Text Text;

    public override void UpdateValues(float current, float max)
    {
        Text.text = $"{Mathf.Floor(current)} / {Mathf.Floor(max)} {Names.DepletableName(DepletableName)}";
    }
}