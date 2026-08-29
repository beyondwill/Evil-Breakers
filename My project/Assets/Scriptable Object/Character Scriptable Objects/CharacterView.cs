using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[System.Serializable]
public class TargetingInfo
{
    public int choosed_index = -1;
    public bool is_player_choosed;
}

public class CharacterView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // 외부 요소
    [SerializeField] private Image character_image;
    [SerializeField] private Image[] l_shape_images;

    [SerializeField] private Slider health_bar_slider;
    [SerializeField] private Image health_bar_fill;
    [SerializeField] private TextMeshProUGUI health_bar_text;

    [SerializeField] private Image shield_image;
    [SerializeField] private TextMeshProUGUI shield_text;

    [SerializeField] private TextMeshProUGUI character_name_text;

    [SerializeField] private GameObject conversation_box;
    [SerializeField] private TextMeshProUGUI conversation_text;

    [SerializeField] private Image name_box;

    [SerializeField] private GameObject current_turn;

    // 피해 텍스트 위치
    [SerializeField] private Transform damaged_text_location;
    [SerializeField] private GameObject damaged_text;

    // 모든 이펙트 생성 기준 위치
    [SerializeField] private Transform effect_point;

    [SerializeField] private BuffUI buffUI;


    // 변수
    [SerializeField] private RectTransform image_rect;
    [SerializeField] private CharacterVariable characterVariable;

    private Sequence currentSeq;

    // 이름 페이드
    private Sequence nameSeq;

    // 죽음 연출
    private Sequence deathSeq;

    private TargetingInfo targeting_info =
        new TargetingInfo();


    [SerializeField] private Color normal_color;
    [SerializeField] private Color selected_color;


    private void Start()
    {
        SetNameAlpha(0);
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        if (characterVariable.is_dead)
            return;


        SetLShapeColor(true);
        ShowName(true);


        if (CardDragController.Instance.IsDragging &&
            CardDragController.Instance.IsTargetCard())
        {
            CardDragController.Instance.SetTarget(
                characterVariable);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetLShapeColor(false);
        ShowName(false);


        if (CardDragController.Instance.IsDragging &&
            CardDragController.Instance.IsTargetCard())
        {
            CardDragController.Instance.ClearTarget();
        }
    }

    private void ShowName(bool show)
    {
        if (nameSeq != null)
            nameSeq.Kill();


        float alpha =
            show ? 1f : 0f;


        nameSeq =
            DOTween.Sequence();


        nameSeq.Join(
            character_name_text.DOFade(
                alpha,
                0.5f)
        );


        nameSeq.Join(
            name_box.DOFade(
                alpha,
                0.5f)
        );
    }


    private void SetNameAlpha(float alpha)
    {
        Color textColor =
            character_name_text.color;

        textColor.a = alpha;

        character_name_text.color =
            textColor;


        Color boxColor =
            name_box.color;

        boxColor.a = alpha;

        name_box.color =
            boxColor;
    }


    // ========================================
    // 캐릭터 초기화
    // ========================================

    public void CharacterInit(CharacterVariable CV)
    {
        characterVariable = CV;

        health_bar_fill.color = CV.character_info.icon_background_color;

        CV.characterView = this;


        // 이벤트 구독
        characterVariable.OnHealthChanged +=
            HealthUpdate;

        characterVariable.OnDeath +=
            DeathAnimation;

        // ⭐ 버프 변경 이벤트 구독
        characterVariable.OnBuffChanged +=
            ShowBuffIcons;


        gameObject.SetActive(true);


        character_image.sprite =
            CV.character_info.character_full_art;


        health_bar_slider.maxValue =
            CV.max_health;


        health_bar_slider.value =
            CV.current_health;


        health_bar_text.text =
            CV.current_health + "/" +
            CV.max_health;


        character_name_text.text =
            CV.character_info.character_name;


        targeting_info.choosed_index =
            CV.character_location_index;


        if (CV is PlayerCharacterVariable)
        {
            selected_color =
                Color.green;

            targeting_info.is_player_choosed =
                true;

            CharacterFlip(true);
        }
        else
        {
            selected_color =
                Color.red;

            targeting_info.is_player_choosed =
                false;

            CharacterFlip(false);
        }


        // 현재 가지고 있는 버프도 한번 갱신
        ShowBuffIcons(
            characterVariable.statContainer.buffList);


        SetNameAlpha(0);
    }


    public void SetLShapeColor(bool is_selected)
    {
        for (int i = 0;
             i < l_shape_images.Length;
             i++)
        {
            l_shape_images[i].color =
                is_selected
                ? selected_color
                : normal_color;
        }
    }


    public void CharacterFlip(bool isPlayer)
    {
        Vector3 scale =
            character_image.rectTransform.localScale;


        scale.x =
            isPlayer
            ? Mathf.Abs(scale.x)
            : -Mathf.Abs(scale.x);


        character_image.rectTransform.localScale =
            scale;
    }


    public void Conversation(string s)
    {
        QuickLocalizationSetup.Instance.GetTextDictionary.Remove(conversation_text);

        conversation_text.text = s;


        if (currentSeq != null)
            currentSeq.Kill();


        conversation_box.SetActive(true);


        RectTransform rect =
            conversation_box.GetComponent<RectTransform>();


        rect.localScale =
            Vector3.zero;


        currentSeq =
            DOTween.Sequence();


        currentSeq.Append(
            rect.DOScale(
                Vector3.one,
                0.3f)
        );


        currentSeq.AppendInterval(1f);


        currentSeq.Append(
            rect.DOScale(
                Vector3.zero,
                0.3f)
        );


        currentSeq.OnComplete(() =>
        {
            conversation_box.SetActive(false);
        });
    }


    private Coroutine hpCoroutine;


    public void HealthUpdate(
        int current,
        int max)
    {
        health_bar_text.text =
            current + " / " + max;


        health_bar_slider.maxValue =
            max;


        if (hpCoroutine != null)
            StopCoroutine(hpCoroutine);


        hpCoroutine =
            StartCoroutine(
                SmoothHealthChange(current));
    }


    IEnumerator SmoothHealthChange(
        int targetHealth)
    {
        float duration = 1f;

        float time = 0f;


        float startValue =
            health_bar_slider.value;


        while (time < duration)
        {
            time += Time.deltaTime;


            float t =
                time / duration;


            health_bar_slider.value =
                Mathf.Lerp(
                    startValue,
                    targetHealth,
                    t);


            yield return null;
        }


        health_bar_slider.value =
            targetHealth;
    }


    // ========================================
    // 버프 UI
    // ========================================

    public void ShowBuffIcons(
        List<CharacterBuffValue> CBV)
    {
        Debug.Log(
            $"[CharacterView] 버프 UI 갱신 : {CBV.Count}"
        );

        if (buffUI == null)
        {
            Debug.LogError(
                "[CharacterView] BuffUI가 NULL임!"
            );

            return;
        }

        buffUI.ShowBuffIcons(CBV);
    }


    public void TakeDamage(int damage_amount)
    {
        Canvas canvas =
            GetComponentInParent<Canvas>();

        GameObject DT;

        if (canvas != null)
        {
            DT = Instantiate(
                damaged_text,
                canvas.transform
            );

            DT.transform.position =
                damaged_text_location.position;

            DT.transform.localScale =
                Vector3.one;

            DT.transform.SetAsLastSibling();
        }
        else
        {
            DT = Instantiate(
                damaged_text,
                damaged_text_location,
                false);
        }

        DT.GetComponent<ShowText>()
            .Init(damage_amount);
    }

    public void Miss()
    {
        Canvas canvas =
            GetComponentInParent<Canvas>();

        GameObject DT;

        if (canvas != null)
        {
            DT = Instantiate(
                damaged_text,
                canvas.transform
            );

            DT.transform.position =
                damaged_text_location.position;

            DT.transform.localScale =
                Vector3.one;

            DT.transform.SetAsLastSibling();
        }
        else
        {
            DT = Instantiate(
                damaged_text,
                damaged_text_location,
                false);
        }

        DT.GetComponent<ShowText>().Miss();
    }


    private void DeathAnimation()
    {
        if (deathSeq != null)
            deathSeq.Kill();


        CanvasGroup cg =
            GetComponent<CanvasGroup>();


        if (cg == null)
            cg =
                gameObject.AddComponent<CanvasGroup>();


        RectTransform rt =
            GetComponent<RectTransform>();


        float width =
            rt.sizeDelta.x;


        deathSeq =
            DOTween.Sequence();


        deathSeq.Append(
            cg.DOFade(
                0f,
                0.5f)
        );


        deathSeq.Append(
            DOTween.To(
                () => rt.sizeDelta.x,
                x =>
                {
                    rt.sizeDelta =
                        new Vector2(
                            x,
                            rt.sizeDelta.y);
                },
                0f,
                0.5f)
        );


        deathSeq.OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }


    public void ShowLShapes(
        bool is_show)
    {
        for (int i = 0;
             i < l_shape_images.Length;
             i++)
        {
            l_shape_images[i]
                .gameObject
                .SetActive(is_show);
        }
    }


    // ========================================
    // 이벤트 해제
    // ========================================

    private void OnDestroy()
    {
        if (characterVariable != null)
        {
            characterVariable.OnHealthChanged -=
                HealthUpdate;

            characterVariable.OnDeath -=
                DeathAnimation;

            // ⭐ 버프 변경 이벤트 해제
            characterVariable.OnBuffChanged -=
                ShowBuffIcons;
        }
    }


    // ========================================
    // 호출 위치
    // ========================================

    public Vector3 GetEffectPosition()
    {
        return character_image.rectTransform.position
            + new Vector3(0, 0, -50);
    }


    // ========================================
    // 캐릭터 턴
    // ========================================

    public void SetCurrentTurn(
        bool is_current_turn)
    {
        if (current_turn == null)
            return;


        current_turn.SetActive(
            is_current_turn);
    }

    // ========================================
    // 반환
    // ========================================

    public Image CharacterImage =>
        character_image;


    public CharacterVariable GetCharacterVariable =>
        characterVariable;
}