using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Spine.Unity;


[System.Serializable]
public class TargetingInfo
{
    public int choosed_index = -1;
    public bool is_player_choosed;
}


public class CharacterView :
    MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    // =====================================================
    // 외부 UI
    // =====================================================

    [Header("Character")]
    [SerializeField] private Image character_image;

    // Character Image와 같은 레벨의 빈 UI 오브젝트
    // Character
    // ├─ Character Image
    // ├─ Spine Parent
    [SerializeField] private Transform spine_parent;

    [SerializeField] private Image[] l_shape_images;


    [Header("Health")]
    [SerializeField] private Slider health_bar_slider;

    [SerializeField] private Image health_bar_fill;

    [SerializeField] private TextMeshProUGUI health_bar_text;


    [Header("Shield")]
    [SerializeField] private Image shield_image;

    [SerializeField] private TextMeshProUGUI shield_text;


    [Header("Name")]
    [SerializeField] private TextMeshProUGUI character_name_text;

    [SerializeField] private Image name_box;


    [Header("Conversation")]
    [SerializeField] private GameObject conversation_box;

    [SerializeField] private TextMeshProUGUI conversation_text;


    [Header("Turn")]
    [SerializeField] private GameObject current_turn;


    [Header("Damage")]
    [SerializeField] private Transform damaged_text_location;

    [SerializeField] private GameObject damaged_text;


    [Header("Effect")]
    [SerializeField] private Transform effect_point;


    [Header("Buff")]
    [SerializeField] private BuffUI buffUI;


    // =====================================================
    // 변수
    // =====================================================

    [SerializeField] private RectTransform image_rect;

    [SerializeField] private CharacterVariable characterVariable;


    // =====================================================
    // Spine
    // =====================================================

    // UI 렌더링
    private SkeletonGraphic spine_graphic;

    // Spine 애니메이션
    private SkeletonAnimation spine_animation;


    // =====================================================
    // Tween
    // =====================================================

    private Sequence currentSeq;

    private Sequence nameSeq;

    private Sequence deathSeq;


    // =====================================================
    // HP
    // =====================================================

    private Coroutine hpCoroutine;


    // =====================================================
    // Target
    // =====================================================

    private TargetingInfo targeting_info =
        new TargetingInfo();


    // =====================================================
    // Color
    // =====================================================

    [SerializeField] private Color normal_color;

    [SerializeField] private Color selected_color;


    // =====================================================
    // Start
    // =====================================================

    private void Start()
    {
        SetNameAlpha(0);
    }


    // =====================================================
    // Pointer Enter
    // =====================================================

    public void OnPointerEnter(
        PointerEventData eventData)
    {
        if (characterVariable == null)
            return;


        if (characterVariable.is_dead)
            return;


        SetLShapeColor(true);

        ShowName(true);


        if (CardDragController.Instance != null &&
            CardDragController.Instance.IsDragging &&
            CardDragController.Instance.IsTargetCard())
        {
            CardDragController.Instance.SetTarget(
                characterVariable);
        }
    }


    // =====================================================
    // Pointer Exit
    // =====================================================

    public void OnPointerExit(
        PointerEventData eventData)
    {
        SetLShapeColor(false);

        ShowName(false);


        if (CardDragController.Instance != null &&
            CardDragController.Instance.IsDragging &&
            CardDragController.Instance.IsTargetCard())
        {
            CardDragController.Instance.ClearTarget();
        }
    }


    // =====================================================
    // 이름 표시
    // =====================================================

    private void ShowName(
        bool show)
    {
        if (nameSeq != null)
            nameSeq.Kill();


        float alpha =
            show ? 1f : 0f;


        nameSeq =
            DOTween.Sequence();


        if (character_name_text != null)
        {
            nameSeq.Join(
                character_name_text.DOFade(
                    alpha,
                    0.5f));
        }


        if (name_box != null)
        {
            nameSeq.Join(
                name_box.DOFade(
                    alpha,
                    0.5f));
        }
    }


    // =====================================================
    // 이름 Alpha
    // =====================================================

    private void SetNameAlpha(
        float alpha)
    {
        if (character_name_text != null)
        {
            Color textColor =
                character_name_text.color;

            textColor.a =
                alpha;

            character_name_text.color =
                textColor;
        }


        if (name_box != null)
        {
            Color boxColor =
                name_box.color;

            boxColor.a =
                alpha;

            name_box.color =
                boxColor;
        }
    }


    // =====================================================
    // 캐릭터 초기화
    // =====================================================

    public void CharacterInit(
        CharacterVariable CV)
    {
        if (CV == null)
        {
            Debug.LogError(
                "[CharacterView] CV가 NULL!");

            return;
        }


        if (CV.character_info == null)
        {
            Debug.LogError(
                "[CharacterView] character_info가 NULL!");

            return;
        }


        if (health_bar_fill == null)
        {
            Debug.LogError(
                "[CharacterView] health_bar_fill이 NULL!");

            return;
        }


        if (character_image == null)
        {
            Debug.LogError(
                "[CharacterView] character_image가 NULL!");

            return;
        }


        if (health_bar_slider == null)
        {
            Debug.LogError(
                "[CharacterView] health_bar_slider가 NULL!");

            return;
        }


        if (health_bar_text == null)
        {
            Debug.LogError(
                "[CharacterView] health_bar_text가 NULL!");

            return;
        }


        if (character_name_text == null)
        {
            Debug.LogError(
                "[CharacterView] character_name_text가 NULL!");

            return;
        }


        // =================================================
        // 기존 이벤트 중복 방지
        // =================================================

        if (characterVariable != null)
        {
            characterVariable.OnHealthChanged -=
                HealthUpdate;

            characterVariable.OnDeath -=
                DeathAnimation;

            characterVariable.OnBuffChanged -=
                ShowBuffIcons;
        }


        characterVariable =
            CV;


        health_bar_fill.color =
            CV.character_info.icon_background_color;


        CV.characterView =
            this;


        // =================================================
        // 이벤트
        // =================================================

        characterVariable.OnHealthChanged +=
            HealthUpdate;

        characterVariable.OnDeath +=
            DeathAnimation;

        characterVariable.OnBuffChanged +=
            ShowBuffIcons;


        gameObject.SetActive(true);


        // =================================================
        // 캐릭터 표시
        // =================================================

        SetupCharacter(
            CV.character_info);


        // =================================================
        // HP
        // =================================================

        health_bar_slider.maxValue =
            CV.max_health;

        health_bar_slider.value =
            CV.current_health;


        health_bar_text.text =
            CV.current_health +
            "/" +
            CV.max_health;


        // =================================================
        // 이름
        // =================================================

        character_name_text.text =
            CV.character_info.character_name;


        // =================================================
        // Target
        // =================================================

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


        // =================================================
        // Buff
        // =================================================

        if (CV.statContainer != null)
        {
            ShowBuffIcons(
                CV.statContainer.buffList);
        }


        SetNameAlpha(0);
    }


    // =====================================================
    // 캐릭터 표시 설정
    // =====================================================

    private void SetupCharacter(
        CharacterInfo info)
    {
        // =================================================
        // 기존 Spine 제거
        // =================================================

        if (spine_graphic != null)
        {
            Destroy(
                spine_graphic.gameObject);

            spine_graphic =
                null;

            spine_animation =
                null;
        }


        // =================================================
        // Spine 데이터가 없으면 기존 Sprite 사용
        // =================================================

        if (info.character_spine_data == null)
        {
            if (character_image != null)
            {
                character_image.gameObject.SetActive(true);

                character_image.sprite =
                    info.character_full_art;
            }

            return;
        }


        // =================================================
        // Spine Parent 검사
        // =================================================

        if (spine_parent == null)
        {
            Debug.LogError(
                "[CharacterView] Spine Parent가 지정되지 않았습니다.");

            return;
        }


        // =================================================
        // 기존 Sprite 숨기기
        // =================================================

        Color color = character_image.color;
        color.a = 0f;
        character_image.color = color;


        // =================================================
        // 생성 전 자식 개수 저장
        // =================================================

        int previousChildCount =
            spine_parent.childCount;


        // =================================================
        // Spine 4.3 UI 생성
        //
        // SkeletonAnimation
        // +
        // SkeletonGraphic
        //
        // 공식 API 사용
        // =================================================

        SkeletonGraphic.NewSkeletonGraphicGameObject(
            info.character_spine_data,
            spine_parent,
            null);


        // =================================================
        // 생성된 GameObject 확인
        // =================================================

        if (spine_parent.childCount <=
            previousChildCount)
        {
            Debug.LogError(
                "[CharacterView] " +
                "Spine UI 생성에 실패했습니다.");

            return;
        }


        Transform createdObject =
            spine_parent.GetChild(
                spine_parent.childCount - 1);


        // =================================================
        // SkeletonGraphic 가져오기
        // =================================================

        spine_graphic =
            createdObject.GetComponent<
                SkeletonGraphic>();


        // =================================================
        // SkeletonAnimation 가져오기
        // =================================================

        spine_animation =
            createdObject.GetComponent<
                SkeletonAnimation>();


        // =================================================
        // 확인
        // =================================================

        if (spine_graphic == null)
        {
            Debug.LogError(
                "[CharacterView] " +
                "생성된 Spine 오브젝트에 " +
                "SkeletonGraphic이 없습니다.");

            return;
        }


        if (spine_animation == null)
        {
            Debug.LogError(
                "[CharacterView] " +
                "생성된 Spine 오브젝트에 " +
                "SkeletonAnimation이 없습니다.");

            return;
        }


        // =================================================
        // 이름
        // =================================================

        createdObject.name =
            info.character_name +
            "_SpineUI";


        // =================================================
        // RectTransform
        // =================================================

        RectTransform spineRect =
            createdObject.GetComponent<
                RectTransform>();


        if (spineRect != null)
        {
            spineRect.anchorMin =
                new Vector2(0.5f, 0.5f);

            spineRect.anchorMax =
                new Vector2(0.5f, 0.5f);

            spineRect.pivot =
                new Vector2(0.5f, 0.5f);

            spineRect.anchoredPosition =
                Vector2.zero;

            spineRect.localRotation =
                Quaternion.identity;

            spineRect.localScale =
                Vector3.one;
        }


        // =================================================
        // 초기화
        // =================================================

        if (!spine_graphic.IsValid)
        {
            spine_graphic.Initialize(false);
        }


        if (!spine_animation.IsValid)
        {
            spine_animation.Initialize(false);
        }

        if (spine_graphic.Skeleton != null)
        {
            var skin = spine_graphic.Skeleton.Data.FindSkin("base");

            if (skin != null)
            {
                spine_graphic.Skeleton.SetSkin("base");
            }

            else
            {
                skin = spine_graphic.Skeleton.Data.FindSkin("weapon/sword");


                if (skin != null)
                {
                    spine_graphic.Skeleton.SetSkin("weapon/sword");
                }
            }
        }

        // =================================================
        // 기본 애니메이션
        // =================================================

        PlayIdleAnimation();
    }


    // =====================================================
    // Spine 애니메이션 존재 여부
    // =====================================================

    private bool HasAnimation(
        string animationName)
    {
        if (spine_animation == null)
            return false;


        if (!spine_animation.IsValid)
            return false;


        if (spine_animation.Skeleton == null)
            return false;


        if (spine_animation.Skeleton.Data == null)
            return false;


        return
            spine_animation
                .Skeleton
                .Data
                .FindAnimation(
                    animationName) != null;
    }


    // =====================================================
    // 애니메이션 재생
    // =====================================================

    private void PlayAnimation(
        string animationName,
        bool loop)
    {
        if (!HasAnimation(
                animationName))
        {
            return;
        }


        spine_animation
            .AnimationState
            .SetAnimation(
                0,
                animationName,
                loop);
    }


    // =====================================================
    // Idle
    // =====================================================

    public void PlayIdleAnimation()
    {
        PlayAnimation(
            "idle",
            true);
    }


    // =====================================================
    // Turn
    // =====================================================

    public void PlayTurnAnimation()
    {
        PlayAnimation(
            "turn",
            true);
    }


    // =====================================================
    // Attack
    // =====================================================

    public void PlayAttackAnimation()
    {
        if (!HasAnimation(
                "shoot"))
        {
            PlayIdleAnimation();
            return;
        }


        Spine.TrackEntry entry =
            spine_animation
                .AnimationState
                .SetAnimation(
                    0,
                    "shoot",
                    false);


        if (entry == null)
            return;


        entry.Complete +=
            OnAttackComplete;
    }


    private void OnAttackComplete(
        Spine.TrackEntry entry)
    {
        if (entry != null)
        {
            entry.Complete -=
                OnAttackComplete;
        }


        PlayIdleAnimation();
    }


    // =====================================================
    // Hit
    // =====================================================

    public void PlayHitAnimation()
    {
        if (!HasAnimation("hit"))
        {
            Debug.Log("없어요");
            return;
        }


        Spine.TrackEntry entry =
            spine_animation
                .AnimationState
                .SetAnimation(
                    0,
                    "hit",
                    false);


        if (entry == null)
            return;


        entry.Complete +=
            OnHitComplete;
    }


    private void OnHitComplete(
        Spine.TrackEntry entry)
    {
        if (entry != null)
        {
            entry.Complete -=
                OnHitComplete;
        }


        PlayIdleAnimation();
    }


    // =====================================================
    // Buff
    // =====================================================

    public void PlayBuffAnimation()
    {
        if (!HasAnimation(
                "buff"))
        {
            return;
        }


        Spine.TrackEntry entry =
            spine_animation
                .AnimationState
                .SetAnimation(
                    0,
                    "buff",
                    false);


        if (entry == null)
            return;


        entry.Complete +=
            OnBuffComplete;
    }


    private void OnBuffComplete(
        Spine.TrackEntry entry)
    {
        if (entry != null)
        {
            entry.Complete -=
                OnBuffComplete;
        }


        PlayIdleAnimation();
    }


    // =====================================================
    // 현재 턴
    // =====================================================

    public void SetCurrentTurn(
        bool is_current_turn)
    {
        if (current_turn != null)
        {
            current_turn.SetActive(
                is_current_turn);
        }


        if (spine_animation == null)
            return;


        if (is_current_turn)
        {
            if (HasAnimation("turn"))
            {
                PlayTurnAnimation();
            }
            else
            {
                PlayIdleAnimation();
            }
        }
        else
        {
            PlayIdleAnimation();
        }
    }


    // =====================================================
    // 좌우 반전
    // =====================================================

    public void CharacterFlip(
        bool isPlayer)
    {
        // =================================================
        // Spine
        // =================================================

        if (spine_graphic != null)
        {
            RectTransform rect =
                spine_graphic.GetComponent<
                    RectTransform>();


            if (rect != null)
            {
                Vector3 scale =
                    rect.localScale;


                scale.x =
                    isPlayer
                    ? Mathf.Abs(scale.x)
                    : -Mathf.Abs(scale.x);


                rect.localScale =
                    scale;
            }


            return;
        }


        // =================================================
        // 기존 Image
        // =================================================

        if (character_image != null)
        {
            Vector3 scale =
                character_image
                    .rectTransform
                    .localScale;


            scale.x =
                isPlayer
                ? Mathf.Abs(scale.x)
                : -Mathf.Abs(scale.x);


            character_image
                .rectTransform
                .localScale =
                scale;
        }
    }


    // =====================================================
    // L Shape 색상
    // =====================================================

    public void SetLShapeColor(
        bool is_selected)
    {
        if (l_shape_images == null)
            return;


        for (int i = 0;
             i < l_shape_images.Length;
             i++)
        {
            if (l_shape_images[i] == null)
                continue;


            l_shape_images[i].color =
                is_selected
                ? selected_color
                : normal_color;
        }
    }


    // =====================================================
    // 대화
    // =====================================================

    public void Conversation(
        string s)
    {
        if (string.IsNullOrEmpty(s))
            return;


        // =================================================
        // Localization
        // =================================================

        if (QuickLocalizationSetup.Instance != null)
        {
            QuickLocalizationSetup.Instance
                .GetTextDictionary
                .Remove(conversation_text);


            QuickLocalizationSetup.Instance
                .RegisterText(
                    conversation_text,
                    s);
        }
        else
        {
            conversation_text.text =
                s;
        }


        // =================================================
        // 말풍선 연출
        // =================================================

        if (currentSeq != null)
            currentSeq.Kill();


        conversation_box.SetActive(true);


        RectTransform rect =
            conversation_box
                .GetComponent<
                    RectTransform>();


        rect.localScale =
            Vector3.zero;


        currentSeq =
            DOTween.Sequence();


        currentSeq.Append(
            rect.DOScale(
                Vector3.one,
                0.3f));


        currentSeq.AppendInterval(
            1f);


        currentSeq.Append(
            rect.DOScale(
                Vector3.zero,
                0.3f));


        currentSeq.OnComplete(() =>
        {
            conversation_box.SetActive(false);
        });
    }


    // =====================================================
    // HP 변경
    // =====================================================

    public void HealthUpdate(
        int current,
        int max)
    {
        if (health_bar_text != null)
        {
            health_bar_text.text =
                current +
                " / " +
                max;
        }


        if (health_bar_slider == null)
            return;


        health_bar_slider.maxValue =
            max;


        if (hpCoroutine != null)
        {
            StopCoroutine(
                hpCoroutine);
        }


        hpCoroutine =
            StartCoroutine(
                SmoothHealthChange(
                    current));
    }


    // =====================================================
    // HP 부드럽게 변경
    // =====================================================

    private IEnumerator SmoothHealthChange(
        int targetHealth)
    {
        float duration =
            1f;

        float time =
            0f;


        float startValue =
            health_bar_slider.value;


        while (time < duration)
        {
            time +=
                Time.deltaTime;


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


    // =====================================================
    // Buff UI
    // =====================================================

    public void ShowBuffIcons(
        List<CharacterBuffValue> CBV)
    {
        if (CBV == null)
            return;


        Debug.Log(
            $"[CharacterView] 버프 UI 갱신 : {CBV.Count}");


        if (buffUI == null)
        {
            Debug.LogError(
                "[CharacterView] BuffUI가 NULL임!");

            return;
        }


        buffUI.ShowBuffIcons(
            CBV);
    }


    // =====================================================
    // 피해
    // =====================================================

    public void TakeDamage(int damage_amount, bool death)
    {
        // 사망 처리가 아닌 경우: 피격 애니메이션 동작
        if (!death)
        {
            PlayHitAnimation();
        } 

        if (damaged_text == null)
            return;


        if (damaged_text_location == null)
            return;


        Canvas canvas =
            GetComponentInParent<Canvas>();


        GameObject DT;


        if (canvas != null)
        {
            DT =
                Instantiate(
                    damaged_text,
                    canvas.transform);


            DT.transform.position =
                damaged_text_location.position;


            DT.transform.localScale =
                Vector3.one;


            DT.transform.SetAsLastSibling();
        }
        else
        {
            DT =
                Instantiate(
                    damaged_text,
                    damaged_text_location,
                    false);
        }


        ShowText showText =
            DT.GetComponent<ShowText>();


        if (showText != null)
        {
            showText.Init(
                damage_amount);
        }
    }


    // =====================================================
    // Miss
    // =====================================================

    public void Miss()
    {
        if (damaged_text == null)
            return;


        if (damaged_text_location == null)
            return;


        Canvas canvas =
            GetComponentInParent<Canvas>();


        GameObject DT;


        if (canvas != null)
        {
            DT =
                Instantiate(
                    damaged_text,
                    canvas.transform);


            DT.transform.position =
                damaged_text_location.position;


            DT.transform.localScale =
                Vector3.one;


            DT.transform.SetAsLastSibling();
        }
        else
        {
            DT =
                Instantiate(
                    damaged_text,
                    damaged_text_location,
                    false);
        }


        ShowText showText =
            DT.GetComponent<ShowText>();


        if (showText != null)
        {
            showText.Miss();
        }
    }


    // =====================================================
    // 죽음
    // =====================================================

    private void DeathAnimation()
    {
        if (deathSeq != null)
            deathSeq.Kill();


        // =================================================
        // Spine Death
        // =================================================

        if (spine_animation != null &&
            HasAnimation("death"))
        {
            Spine.TrackEntry entry =
                spine_animation
                    .AnimationState
                    .SetAnimation(
                        0,
                        "death",
                        false);


            if (entry != null)
            {
                entry.Complete +=
                    OnSpineDeathComplete;

                return;
            }
        }


        // =================================================
        // Spine이 없으면 기존 Image Death
        // =================================================

        PlayImageDeathAnimation();
    }


    // =====================================================
    // Spine Death 완료
    // =====================================================

    private void OnSpineDeathComplete(
        Spine.TrackEntry entry)
    {
        if (entry != null)
        {
            entry.Complete -=
                OnSpineDeathComplete;
        }


        gameObject.SetActive(false);
    }


    // =====================================================
    // 기존 Sprite 죽음 연출
    // =====================================================

    private void PlayImageDeathAnimation()
    {
        CanvasGroup cg =
            GetComponent<CanvasGroup>();


        if (cg == null)
        {
            cg =
                gameObject.AddComponent<
                    CanvasGroup>();
        }


        RectTransform rt =
            GetComponent<
                RectTransform>();


        if (rt == null)
        {
            gameObject.SetActive(false);
            return;
        }


        deathSeq =
            DOTween.Sequence();


        deathSeq.Append(
            cg.DOFade(
                0f,
                0.5f));


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
                0.5f));


        deathSeq.OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }


    // =====================================================
    // L Shape 표시
    // =====================================================

    public void ShowLShapes(
        bool is_show)
    {
        if (l_shape_images == null)
            return;


        for (int i = 0;
             i < l_shape_images.Length;
             i++)
        {
            if (l_shape_images[i] == null)
                continue;


            l_shape_images[i]
                .gameObject
                .SetActive(is_show);
        }
    }


    // =====================================================
    // 이펙트 위치
    // =====================================================

    public Vector3 GetEffectPosition()
    {
        Vector3 position;

        if (effect_point != null)
        {
            position = effect_point.position;
        }
        else if (spine_graphic != null)
        {
            position = spine_graphic.transform.position;
        }
        else if (character_image != null)
        {
            position = character_image.rectTransform.position;
        }
        else
        {
            position = transform.position;
        }

        position.z = 0f;

        return position;
    }


    // =====================================================
    // 현재 턴
    // =====================================================

    // 기존 코드에서 이 함수를 호출하고 있으므로
    // Spine이 있으면 turn 애니메이션도 같이 실행한다.

    // =====================================================
    // 이벤트 해제
    // =====================================================

    private void OnDestroy()
    {
        if (characterVariable != null)
        {
            characterVariable.OnHealthChanged -=
                HealthUpdate;


            characterVariable.OnDeath -=
                DeathAnimation;


            characterVariable.OnBuffChanged -=
                ShowBuffIcons;
        }


        if (currentSeq != null)
            currentSeq.Kill();


        if (nameSeq != null)
            nameSeq.Kill();


        if (deathSeq != null)
            deathSeq.Kill();
    }


    // =====================================================
    // 반환
    // =====================================================

    public Image CharacterImage =>
        character_image;


    public CharacterVariable GetCharacterVariable =>
        characterVariable;


    public SkeletonAnimation GetSpine =>
        spine_animation;
}