using UnityEngine;

public static class UIFollowMouse
{
    public static void Follow(
        RectTransform target,
        Canvas canvas,
        Vector2 offset
    )
    {
        Vector2 mousePos = Input.mousePosition;

        RectTransform canvasRect = canvas.transform as RectTransform;


        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            mousePos,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : canvas.worldCamera,
            out Vector2 localPoint
        );


        target.localPosition = localPoint + offset;
    }
}