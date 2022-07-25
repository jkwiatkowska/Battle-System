using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDPopupText : MonoBehaviour
{
    [SerializeField] Canvas HUD;
    [SerializeField] ObjectPool PopupPool;
    [SerializeField] float PopupDisplayTime;
    Queue<(PopupText, float)> Popups;

    public static HUDPopupText Instance;
    void Awake()
    {
        Instance = this;
        Popups = new Queue<(PopupText, float)>();
    }

    void Update()
    {
        if (Popups.Count > 0 && Popups.Peek().Item2 < BattleSystem.Time)
        {
            var popup = Popups.Dequeue().Item1;
            popup.gameObject.SetActive(false);
            PopupPool.ReturnToPool(popup.gameObject);
        }
    }

    public void DisplayDamage(string resource, float change, PayloadComponentResult payloadResult)
    {
        string text = NamesAndText.ResourceChangeText(resource, change, payloadResult, out var color);

        DisplayText(payloadResult.Payload.Target.Entity, text, color);
    }

    public void DisplayMiss(Entity target)
    {
        string text = NamesAndText.MissText(out var color);

        DisplayText(target, text, color);
    }

    public void DisplayImmune(Entity target)
    {
        string text = NamesAndText.ImmuneText(out var color);

        DisplayText(target, text, color);
    }

    public void DisplayText(Entity target, string text, Color color)
    {
        var popupTextObject = PopupPool.GetFromPool();
        if (popupTextObject == null)
        {
            popupTextObject = Popups.Dequeue().Item1.gameObject;
        }

        var popup = popupTextObject.GetComponentInChildren<PopupText>();
        if (popup == null)
        {
            Debug.LogError($"A PopupText component could not be found.");
            return;
        }

        Popups.Enqueue((popup, BattleSystem.Time + PopupDisplayTime));

        var position = target.Origin;

        popup.Setup(position, HUD, text, color);
        popup.gameObject.SetActive(true);
    }
}
