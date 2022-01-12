using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDTarget : MonoBehaviour
{
    [SerializeField] Canvas HUD;
    [SerializeField] GameObject TargetHUDCanvas;
    [SerializeField] List<DepletableDisplay> DepletableDisplay;
    [SerializeField] Text TargetLevelText;
    [SerializeField] Text TargetNameText;

    void Awake()
    {
        TargetHUDCanvas.SetActive(false);
    }

    public void SelectTarget(EntityCanvas entityCanvas)
    {
        foreach (var display in DepletableDisplay)
        {
            entityCanvas.AddDepletableDisplay(display);
        }

        entityCanvas.SetEntityNameText(TargetNameText);
        entityCanvas.SetEntityLevelText(TargetLevelText);

        TargetHUDCanvas.SetActive(true);
    }

    public void ClearSelection(EntityCanvas entityCanvas)
    {
        foreach (var display in DepletableDisplay)
        {
            entityCanvas.RemoveDepletableDisplay(display);
        }

        TargetHUDCanvas.SetActive(false);
    }
}
