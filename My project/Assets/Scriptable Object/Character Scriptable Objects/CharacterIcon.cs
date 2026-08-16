using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CharacterIcon : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image character_icon_background;
    [SerializeField] private Image character_image;
    [SerializeField] private Image current_turn_effect;

    [SerializeField] private LayoutElement layout_element;


    private CharacterVariable character_variable;

    private bool is_destroying = false;


    public CharacterVariable Character
    {
        get
        {
            return character_variable;
        }
    }



    public void Init(CharacterVariable CV)
    {
        character_variable = CV;


        CharacterInfo character_info =
            CV.character_info;


        if (character_info is PlayerCharacterInfo)
        {
            character_icon_background.color =
                ((PlayerCharacterInfo)character_info)
                .icon_background_color;
        }
        else
        {
            character_icon_background.color =
                Color.black;
        }


        character_image.sprite =
            character_info.character_icon;


        // 사망 이벤트 구독
        character_variable.OnDeath +=
            FadeAndShrinkIcon;


        SetCurrent(false);
    }



    public void SetCurrent(bool value)
    {
        if (current_turn_effect == null)
            return;


        current_turn_effect.gameObject.SetActive(value);
    }



    // 캐릭터 사망 시 아이콘 제거
    public void FadeAndShrinkIcon()
    {
        if (is_destroying)
            return;


        is_destroying = true;


        transform.DOKill();


        Sequence seq = DOTween.Sequence();


        seq.Join(
            character_icon_background
            .DOFade(0f, 0.2f)
        );


        seq.Join(
            character_image
            .DOFade(0f, 0.2f)
        );


        RectTransform rt =
            transform as RectTransform;


        seq.Append(
            DOTween.To(
                () => rt.sizeDelta.x,
                x =>
                {
                    rt.sizeDelta =
                        new Vector2(
                            x,
                            rt.sizeDelta.y
                        );
                },
                0,
                0.5f
            )
        );


        seq.OnComplete(() =>
        {
            if (this != null)
                Destroy(gameObject);
        });
    }



    private void OnDestroy()
    {
        transform.DOKill();


        if (character_variable != null)
        {
            character_variable.OnDeath -= FadeAndShrinkIcon;
        }
    }
}