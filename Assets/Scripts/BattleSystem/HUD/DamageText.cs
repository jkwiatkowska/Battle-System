using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamageText : MonoBehaviour
{
    [SerializeField] float Duration = 0.5f;
    [SerializeField] Vector2 PositionOffsetPerSecond;
    [SerializeField] float FadePerSecond;
    [SerializeField] float ScaleChangePerSecond;
    Text Text;

    Vector3 WorldPosition;
    Vector2 ScreenSize;
    float StartTime;
    public void Setup(Vector3 worldPosition, Canvas canvas, Text text)
    {
        WorldPosition = worldPosition;
        ScreenSize.x = canvas.GetComponent<RectTransform>().rect.width;
        ScreenSize.y = canvas.GetComponent<RectTransform>().rect.height;
        Text = text;

        StartTime = BattleSystem.TimeSinceStart;
        Update();

        Destroy(gameObject, Duration);
    }

    void Update()
    {
        if (WorldPosition != null)
        {
            var timeSinceStart = BattleSystem.TimeSinceStart - StartTime;

            var offset = PositionOffsetPerSecond * timeSinceStart;
                
            Vector2 screenPosition = Camera.main.WorldToViewportPoint(WorldPosition);
            transform.localPosition = new Vector2(ScreenSize.x * (screenPosition.x - 0.5f + offset.x), ScreenSize.y * (screenPosition.y - 0.5f + offset.y));

            var fade = 1.0f - Mathf.Min(1.0f, FadePerSecond * timeSinceStart);
            var newColour = Text.color;
            newColour.a = fade;
            Text.color = newColour;

            transform.localScale *= 1.0f - Time.deltaTime * -ScaleChangePerSecond;
        }
    }
}
