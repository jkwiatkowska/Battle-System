using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldPosToHUD
{
    public static void SetPosition(Transform transform, Canvas canvas, Vector3 worldPosition, float offsetX = 0.0f, float offsetY = 0.0f)
    {
        var screenSizeX = canvas.GetComponent<RectTransform>().rect.width;
        var screenSizeY = canvas.GetComponent<RectTransform>().rect.height;

        var screenPosition = Camera.main.WorldToViewportPoint(worldPosition);

        transform.localPosition = new Vector2(screenSizeX * (screenPosition.x - 0.5f + offsetX), screenSizeY * (screenPosition.y - 0.5f + offsetY));
    }
}
