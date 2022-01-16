using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupText : MonoBehaviour
{
    [SerializeField] float Duration = 0.5f;
    [SerializeField] Vector2 PositionOffsetPerSecond;
    [SerializeField] float FadePerSecond;
    [SerializeField] float ScaleChangePerSecond;
    Text Text;

    Canvas Canvas;
    Vector3 WorldPosition;
    float StartTime;

    public void Setup(Vector3 worldPosition, Canvas canvas, string text)
    {
        WorldPosition = worldPosition;
        Canvas = canvas;
        Text = GetComponentInChildren<Text>();
        Text.text = text;

        StartTime = BattleSystem.Time;
        Update();

        Destroy(gameObject, Duration);
    }

    void Update()
    {
        if (WorldPosition != null)
        {
            var timeSinceStart = BattleSystem.Time - StartTime;

            var offset = PositionOffsetPerSecond * timeSinceStart;

            WorldPosToHUD.SetPosition(transform, Canvas, WorldPosition, offset.x, offset.y);

            var fade = 1.0f - Mathf.Min(1.0f, FadePerSecond * timeSinceStart);
            var newColour = Text.color;
            newColour.a = fade;
            Text.color = newColour;

            transform.localScale *= 1.0f - Time.deltaTime * -ScaleChangePerSecond;
        }
    }

    public void SetColor(Color colour)
    {
        Text.color = colour;
    }
}
