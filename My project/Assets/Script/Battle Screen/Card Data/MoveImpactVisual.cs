using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[CreateAssetMenu(menuName = "Card Visual/Move Impact")]
public class MoveImpactVisual : CardVisual
{
    public Sprite effectSprite;

    [Header("Effect Settings")]
    public Vector2 effectSize = new Vector2(250f, 170f);

    public float moveDistance = 300f;
    public float duration = 0.7f;


    // =================================================
    // 적용 대상
    // =================================================

    [Header("Apply Target")]
    public bool applyToCaster = false;


    // =================================================
    // 이동 방향
    // =================================================

    public enum MoveDirection
    {
        Left,
        Right,
        Up,
        Down
    }

    [Header("Direction")]
    public MoveDirection direction = MoveDirection.Left;


    // =================================================
    // 시작 방식
    // =================================================

    public enum StartMode
    {
        FromCharacter,     // 캐릭터 위치에서 시작
        TowardCharacter    // 반대편에서 시작해서 캐릭터로 이동
    }

    [Header("Start Mode")]
    public StartMode startMode = StartMode.FromCharacter;


    public override void Play(
        CharacterVariable caster,
        List<CharacterVariable> targets)
    {
        // =================================================
        // 적용 대상 결정
        // =================================================

        if (applyToCaster)
        {
            // Caster에게 적용
            PlayEffect(caster);
        }
        else
        {
            // Target들에게 적용
            if (targets == null)
                return;

            foreach (CharacterVariable target in targets)
            {
                PlayEffect(target);
            }
        }
    }


    // =================================================
    // 이펙트 실행
    // =================================================

    private void PlayEffect(CharacterVariable target)
    {
        if (target == null ||
            target.characterView == null)
            return;


        // =================================================
        // Canvas
        // =================================================

        Canvas canvas =
            target.characterView.GetComponentInParent<Canvas>();

        if (canvas == null)
        {
            Debug.LogError(
                "[MoveImpactVisual] Canvas를 찾을 수 없습니다."
            );

            return;
        }


        // =================================================
        // 캐릭터 위치
        // =================================================

        Vector3 targetPosition =
            target.characterView.GetEffectPosition();


        // =================================================
        // 방향
        // =================================================

        Vector3 moveDirection =
            GetDirection();


        // =================================================
        // 시작 / 종료 위치
        // =================================================

        Vector3 startPosition;
        Vector3 endPosition;


        if (startMode == StartMode.FromCharacter)
        {
            // 캐릭터 위치에서 시작
            startPosition = targetPosition;

            // 지정된 방향으로 이동
            endPosition =
                targetPosition +
                moveDirection * moveDistance;
        }
        else
        {
            // 캐릭터 반대편에서 시작
            startPosition =
                targetPosition -
                moveDirection * moveDistance;

            // 캐릭터 위치로 이동
            endPosition = targetPosition;
        }


        // =================================================
        // 이펙트 생성
        // =================================================

        GameObject effectObject =
            new GameObject("Move Impact Effect");

        effectObject.transform.SetParent(
            canvas.transform,
            false
        );


        // =================================================
        // Image
        // =================================================

        Image image =
            effectObject.AddComponent<Image>();

        image.sprite = effectSprite;
        image.preserveAspect = true;
        image.raycastTarget = false;


        // =================================================
        // RectTransform
        // =================================================

        RectTransform rect =
            effectObject.GetComponent<RectTransform>();

        rect.sizeDelta = effectSize;


        // =================================================
        // 시작 위치
        // =================================================

        effectObject.transform.position =
            startPosition;


        // 가장 위에 표시
        effectObject.transform.SetAsLastSibling();


        // =================================================
        // Alpha
        // =================================================

        Color color = image.color;
        color.a = 1f;
        image.color = color;


        // =================================================
        // 이동 + 페이드
        // =================================================

        Sequence sequence =
            DOTween.Sequence();

        sequence.Join(
            effectObject.transform.DOMove(
                endPosition,
                duration
            )
            .SetEase(Ease.OutCubic)
        );

        sequence.Join(
            image.DOFade(
                0f,
                duration
            )
            .SetEase(Ease.InQuad)
        );


        // =================================================
        // 종료
        // =================================================

        sequence.OnComplete(() =>
        {
            Destroy(effectObject);
        });
    }


    // =================================================
    // 방향 반환
    // =================================================

    private Vector3 GetDirection()
    {
        switch (direction)
        {
            case MoveDirection.Left:
                return Vector3.left;

            case MoveDirection.Right:
                return Vector3.right;

            case MoveDirection.Up:
                return Vector3.up;

            case MoveDirection.Down:
                return Vector3.down;
        }

        return Vector3.left;
    }
}