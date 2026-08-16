using UnityEngine;

public class BattleFieldArea : MonoBehaviour
{
    private static BattleFieldArea Instance;

    [SerializeField] private Canvas canvas;

    private RectTransform rect;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        Instance = this;
    }

    public static bool CheckDropArea(Vector2 screenPos)
    {
        if (Instance == null)
            return false;

        Vector2 localPos;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            Instance.rect,
            screenPos,
            Instance.canvas.worldCamera,
            out localPos
        );

        return Instance.rect.rect.Contains(localPos);
    }
}