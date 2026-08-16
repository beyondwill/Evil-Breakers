using UnityEngine;
using UnityEngine.UI;

public class DragItemUI : MonoBehaviour
{
    [SerializeField] private Image icon;

    private RectTransform rect;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();

        Hide();
    }

    public void Show(Sprite sprite)
    {
        icon.sprite = sprite;

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void SetPosition(Vector2 position)
    {
        rect.position = position;
    }
}