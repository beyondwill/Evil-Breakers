using DG.Tweening;
using UnityEngine;

public class CardMoving : MonoBehaviour
{
    private RectTransform rt;

    private float x_point;
    private float y_point;
    private float card_angle;

    private int card_index;
    private CanvasGroup canvasGroup;


    private RectTransform graveyardTarget;


    private void Awake()
    {
        rt = GetComponent<RectTransform>();

        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }


    public void CardInit(int index)
    {
        card_index = index;
    }


    public void SetRaycast(bool value)
    {
        canvasGroup.blocksRaycasts = value;
    }


    public void SetCoordinate(float x, float y, float angle)
    {
        x_point = x;
        y_point = y;
        card_angle = angle;
    }


    public void SetGraveyardTarget(RectTransform target)
    {
        graveyardTarget = target;
    }


    public void MoveCard(float x, float y, float angle, float time = 0f)
    {
        rt.DOKill();

        rt.DOAnchorPos(
            new Vector2(x, y),
            time)
            .SetEase(Ease.OutCubic);


        rt.DOLocalRotate(
            new Vector3(0, 0, -angle),
            time)
            .SetEase(Ease.OutQuad);
    }


    public void HoverCard(float moveY, float scale)
    {
        rt.DOKill();


        rt.DOAnchorPos(
            new Vector2(x_point, y_point + moveY),
            0.25f)
            .SetEase(Ease.OutCubic);


        rt.DOLocalRotate(
            Vector3.zero,
            0.25f)
            .SetEase(Ease.OutCubic);


        rt.DOScale(
            scale,
            0.25f)
            .SetEase(Ease.OutBack);
    }


    public void SizeCard(float size, float time)
    {
        rt.DOKill();

        rt.DOScale(
            size,
            time)
            .SetEase(Ease.OutCubic);
    }


    public void MoveOffset(Vector2 pos, float time)
    {
        rt.DOKill();

        rt.DOAnchorPos(
            pos,
            time)
            .SetEase(Ease.OutCubic);
    }


    public void ResetHover()
    {
        rt.DOKill();


        Sequence seq = DOTween.Sequence();


        seq.Join(
            rt.DOAnchorPos(
                new Vector2(x_point, y_point),
                0.25f)
            .SetEase(Ease.OutCubic));


        seq.Join(
            rt.DOLocalRotate(
                new Vector3(0, 0, -card_angle),
                0.25f)
            .SetEase(Ease.OutCubic));


        seq.Join(
            rt.DOScale(
                Vector3.one,
                0.25f)
            .SetEase(Ease.OutCubic));
    }


    public void ResetRotation(float time = 0.2f)
    {
        rt.DOKill();

        rt.DOLocalRotate(
            Vector3.zero,
            time)
            .SetEase(Ease.OutCubic);
    }


    public void KillTween()
    {
        rt.DOKill();
    }


    public void SetFlatRotation()
    {
        rt.DOKill();

        rt.localRotation = Quaternion.identity;
    }


    public Vector2 GetCoordinate()
    {
        return new Vector2(
            x_point,
            y_point);
    }


    public void DiscardMove()
    {
        if (graveyardTarget == null)
        {
            Destroy(gameObject);
            return;
        }


        rt.DOKill();

        SetRaycast(false);


        Vector3 targetWorldPosition =
            graveyardTarget.position;


        Vector3 targetLocalPosition =
            rt.parent.InverseTransformPoint(
                targetWorldPosition
            );


        Sequence seq = DOTween.Sequence();


        seq.Join(
            rt.DOAnchorPos(
                targetLocalPosition,
                0.5f)
            .SetEase(Ease.InCubic)
        );


        seq.Join(
            rt.DOLocalRotate(
                new Vector3(0, 0, 360f),
                0.5f)
            .SetEase(Ease.OutCubic)
        );


        seq.Join(
            rt.DOScale(
                0f,
                0.5f)
            .SetEase(Ease.InCubic)
        );


        seq.OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }


    public float Angle => card_angle;

    public int CardIndex => card_index;
}