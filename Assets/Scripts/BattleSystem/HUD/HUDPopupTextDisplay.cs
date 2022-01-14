using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDPopupTextDisplay : MonoBehaviour
{
    [SerializeField] Canvas HUD;
    [SerializeField] PopupText DamageTextPrefab;
    [SerializeField] Color DamageColor;
    [SerializeField] Color HealingColor;

    public static HUDPopupTextDisplay Instance;
    void Awake()
    {
        Instance = this;    
    }

    public void DisplayDamage(Entity target, ActionPayload action, float change)
    {
        PopupText damageText = Instantiate(DamageTextPrefab, transform);

        string text = NamesAndText.DamageText(action, change);

        var position = target.transform.position;
        position.y += target.EntityData.Height;

        damageText.Setup(position, HUD, text);

        if (change > 0)
        {
            damageText.SetColor(HealingColor);
        }
        else if (change < 0)
        {
            damageText.SetColor(DamageColor);
        }
    }
}
