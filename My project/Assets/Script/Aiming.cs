using UnityEngine;

public class Aiming : MonoBehaviour
{
    // 인스턴스화
    public static Aiming Instance;

    public RectTransform rectTransform;
    public Canvas canvas;

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    void Update()
    {
        Vector2 pos;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            Input.mousePosition,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out pos
        );

        rectTransform.anchoredPosition = pos;
    }
}