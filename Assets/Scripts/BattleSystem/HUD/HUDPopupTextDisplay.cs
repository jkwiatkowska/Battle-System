using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDPopupTextDisplay : MonoBehaviour
{
    [SerializeField] Canvas HUD;
    [SerializeField] PopupText PopupTextPrefab;

    public static HUDPopupTextDisplay Instance;
    void Awake()
    {
        Instance = this;    
    }

    public void DisplayDamage(Entity target, ActionPayload action, float change, List<string> flags)
    {
        string text = NamesAndText.DamageText(action, change, flags, out var color);

        DisplayText(target, text, color);
    }

    public void DisplayMiss(Entity target)
    {
        string text = NamesAndText.MissText(out var color);

        DisplayText(target, text, color);
    }

    public void DisplayText(Entity target, string text, Color color)
    {
        PopupText popupText = Instantiate(PopupTextPrefab, transform);

        var position = target.transform.position;
        position.y += target.EntityData.Height;

        popupText.Setup(position, HUD, text);

        popupText.SetColor(color);
    }
}
