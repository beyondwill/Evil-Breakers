using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class HorrorUI : MonoBehaviour
{
    [Header("Slider")]
    [SerializeField]
    private Slider horror_slider;


    [Header("Value")]
    [SerializeField]
    private int max_value = 100;


    [Header("Animation")]
    [SerializeField]
    private float fillDuration = 0.5f;


    private BattleData battleData;

    private Tween horrorTween;


    private void Start()
    {
        if (horror_slider == null)
        {
            Debug.LogError(
                "[HorrorUI] Slider가 연결되지 않았습니다."
            );

            return;
        }


        // ==========================================
        // Slider 설정
        // ==========================================

        horror_slider.minValue = 0;
        horror_slider.maxValue = max_value;


        // ==========================================
        // BattleData 가져오기
        // ==========================================

        battleData =
            DataManager.Instance.GetBattleData;


        if (battleData == null)
        {
            Debug.LogError(
                "[HorrorUI] BattleData가 없습니다."
            );

            return;
        }


        // ==========================================
        // 이벤트 구독
        // ==========================================

        battleData.OnHorrorChanged +=
            OnHorrorChanged;


        // ==========================================
        // 현재 Horror 즉시 반영
        // ==========================================

        horror_slider.value =
            Mathf.Clamp(
                battleData.GetHorror(),
                0,
                max_value
            );
    }


    // ==========================================
    // Horror 변경
    // ==========================================

    private void OnHorrorChanged(
        int value,
        int changeAmount)
    {
        value =
            Mathf.Clamp(
                value,
                0,
                max_value
            );


        // ==========================================
        // 기존 애니메이션 중단
        // ==========================================

        horrorTween?.Kill();


        // ==========================================
        // 현재 위치에서 목표값까지 이동
        // ==========================================

        horrorTween =
            horror_slider.DOValue(
                value,
                fillDuration
            )
            .SetEase(Ease.OutQuad);
    }


    // ==========================================
    // Destroy
    // ==========================================

    private void OnDestroy()
    {
        // Tween 정리
        horrorTween?.Kill();


        // 이벤트 해제
        if (battleData == null)
            return;


        battleData.OnHorrorChanged -=
            OnHorrorChanged;
    }
}
