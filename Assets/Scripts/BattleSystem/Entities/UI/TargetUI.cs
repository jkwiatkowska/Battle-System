using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TargetUI : MonoBehaviour
{
    [SerializeField] GameObject Canvas;
    [SerializeField] List<DepletableUI> DepletableUI;
    [SerializeField] Text TargetLevelText;
    [SerializeField] Text TargetNameText;

    private void Awake()
    {
        Canvas.SetActive(false);
    }

    public void SelectTarget(EntityUI entityUI)
    {
        foreach (var ui in DepletableUI)
        {
            entityUI.AddDepletableUI(ui);
        }

        entityUI.SetEntityNameText(TargetNameText);
        entityUI.SetEntityLevelText(TargetLevelText);

        Canvas.SetActive(true);
    }

    public void ClearSelection(EntityUI entityUI)
    {
        foreach (var ui in DepletableUI)
        {
            entityUI.RemoveDepletableUI(ui);
        }

        Canvas.SetActive(false);
    }
}
