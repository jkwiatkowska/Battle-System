using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupText : MonoBehaviour
{
    [SerializeField] Vector2 PositionOffsetPerSecond;
    [SerializeField] float FadePerSecond;
    [SerializeField] float ScaleChangePerSecond;
    Text Text;
    Vector3 Scale;

    Canvas Canvas;
    Vector3 WorldPosition;
    float StartTime;

    void Awake()
    {
        Text = GetComponentInChildren<Text>();
        Scale = transform.localScale;
    }

    public void Setup(Vector3 worldPosition, Canvas canvas, string text, Color color)
    {
        WorldPosition = worldPosition;
        Canvas = canvas;

        Setup(text, color);
    }

    public void Setup(string text, Color color)
    {
        Text.text = text;
        Text.color = color;
        transform.localScale = Scale;

        StartTime = BattleSystem.Time;
        Update();
    }

    void Update()
    {
        if (WorldPosition != null)
        {
            var timeSinceStart = BattleSystem.Time - StartTime;

            var offset = PositionOffsetPerSecond * timeSinceStart;

            if (Canvas != null && WorldPosition != null)
            {
                WorldPosToHUD.SetPosition(transform, Canvas, WorldPosition, offset.x, offset.y);
            }
            else
            {
                var position = transform.position;
                position.x += offset.x;
                position.y += offset.y;
                transform.position = position;
            }

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
