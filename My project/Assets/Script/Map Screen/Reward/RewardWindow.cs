using System.Collections.Generic;
using UnityEngine;

public class RewardWindow : MonoBehaviour
{
    [Header("Class")]
    [SerializeField] private RewardManager rewardManager;

    [Header("UI")]
    [SerializeField] private Transform rewardGrid;

    [Header("Prefab")]
    [SerializeField] private IconButton iconPrefab;

    private readonly List<IconButton> buttons = new();

    public void ShowItems(List<InventoryItem> items)
    {
        Clear();

        for (int i = 0; i < items.Count; i++)
        {
            int index = i;
            InventoryItem inventoryItem = items[index];

            IconButton button =
                Instantiate(iconPrefab, rewardGrid);

            button.ButtonInit(
                index,
                Color.white,
                inventoryItem.item.icon
            );

            button.SetAmount(inventoryItem.amount);

            // Å¬¸¯ÇÏ¸é È¹µæ
            button.ActionAdd(() =>
            {
                rewardManager.TakeItem(index);
                TooltipUI.Instance.Hide();
            });

            // ¸¶¿ì½º ¿Ã¸®¸é ÅøÆÁ
            button.PointerEnterAdd(() =>
            {
                TooltipUI.Instance.Show(
                    inventoryItem.item,
                    button.transform as RectTransform
                );
            });

            // ¸¶¿ì½º ³ª°¡¸é ÅøÆÁ ¼û±è
            button.PointerExitAdd(() =>
            {
                TooltipUI.Instance.Hide();
            });

            buttons.Add(button);
        }
    }

    public void Refresh(List<InventoryItem> items)
    {
        ShowItems(items);
    }

    private void Clear()
    {
        foreach (IconButton button in buttons)
        {
            if (button == null)
                continue;

            button.ActionRemove();
            button.PointerEnterRemove();
            button.PointerExitRemove();

            Destroy(button.gameObject);
        }

        buttons.Clear();
    }

    private void OnDisable()
    {
        TooltipUI.Instance?.Hide();
    }
}