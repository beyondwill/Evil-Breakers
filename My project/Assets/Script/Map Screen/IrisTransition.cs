using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class IrisTransition : MonoBehaviour
{
    [SerializeField]
    private Image transitionImage;

    private Material irisMat;

    void Awake()
    {
        irisMat = Instantiate(transitionImage.material);
        transitionImage.material = irisMat;
    }

    private void Start()
    {
        Open();
    }

    /// <summary>
    /// 화면 열기 (검정 → 투명)
    /// </summary>
    public void Open(float duration = 1f)
    {
        irisMat.SetFloat("_Radius", 0f);

        DOTween.To(
            () => irisMat.GetFloat("_Radius"),
            x => irisMat.SetFloat("_Radius", x),
            1.5f,
            duration
        );
    }

    /// <summary>
    /// 화면 닫기 (투명 → 검정)
    /// </summary>
    public void Close(float duration = 1f)
    {
        irisMat.SetFloat("_Radius", 1.5f);

        DOTween.To(
            () => irisMat.GetFloat("_Radius"),
            x => irisMat.SetFloat("_Radius", x),
            0f,
            duration
        );
    }

    /// <summary>
    /// 특정 화면 좌표를 중심으로 닫기
    /// </summary>
    public void CloseAtScreenPosition(
        Vector2 screenPos,
        float duration = 1f)
    {
        Vector2 uv = new Vector2(
            screenPos.x / Screen.width,
            screenPos.y / Screen.height
        );

        irisMat.SetVector("_Center", uv);
        Close(duration);
    }

    /// <summary>
    /// 특정 화면 좌표를 중심으로 열기
    /// </summary>
    public void OpenAtScreenPosition(
        Vector2 screenPos,
        float duration = 0.7f)
    {
        Vector2 uv = new Vector2(
            screenPos.x / Screen.width,
            screenPos.y / Screen.height
        );

        irisMat.SetVector("_Center", uv);
        Open(duration);
    }
}