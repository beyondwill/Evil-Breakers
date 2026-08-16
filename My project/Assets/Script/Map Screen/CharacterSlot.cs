using UnityEngine;
using UnityEngine.UI;

public class CharacterSlot : MonoBehaviour
{
    [SerializeField] private IconButton iconButton;
    [SerializeField] private HealthBar healthBar;
    [SerializeField] private Image buttonImage;

    [Header("Talking")]
    [SerializeField] private TalkingBoxUI talkingBoxUI;

    private CharacterInfo characterInfo;

    public void SetCharacter(
        PlayerCharacterData characterData,
        CharacterInfo info,
        Sprite icon,
        int currentHp,
        int maxHp)
    {
        characterInfo = info;

        iconButton.SetImage(icon);
        healthBar.SetHealth(currentHp, maxHp);
    }

    public void UpdateHealth(
        int currentHp,
        int maxHp)
    {
        healthBar.SetHealth(currentHp, maxHp);
    }

    public void IsDead()
    {
        buttonImage.color = Color.gray;
    }

    public CharacterInfo GetCharacterInfo()
    {
        return characterInfo;
    }
}