using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShowText : MonoBehaviour
{
    // 외부 요소
    [SerializeField] private TextMeshProUGUI damage_text;           // 데미지 텍스트

    // 변수
    [SerializeField] private Color damaged_color;                   // 피해 색상
    [SerializeField] private Color non_color;                       // 피해 색상
    [SerializeField] private Color healed_color;                    // 회복 색상

    public void Init(int damage_amount)
    {
        // 피해라면
        if (damage_amount > 0)
        {
            damage_text.text = "-" + damage_amount.ToString();
            damage_text.color = damaged_color;
        }

        // 아무것도 없다면
        else if (damage_amount == 0)
        {
            damage_text.text = damage_amount.ToString();
            damage_text.color = non_color;
        }

        // 회복이라면
        else
        {
            damage_text.text = "+" + (-damage_amount).ToString();
            damage_text.color = healed_color;
        }

        RisingText();
    }

    public void Miss()
    {
        damage_text.text = "Miss";
        damage_text.color = Color.cyan;

        RisingText();
    }

    public void ShowTextInit(string text, Color c)
    {
        damage_text.text = text;
        damage_text.color = c;
        RisingText();
    }

    // 텍스트 띄우기
    public void RisingText()
    {
        // 초기 위치 저장
        RectTransform rect = damage_text.rectTransform;

        Vector3 startPos =
            rect.localPosition;

        Vector3 endPos =
            startPos + new Vector3(0, 100f, 0);


        // 처음에는 투명
        Color color =
            damage_text.color;

        color.a = 0f;

        damage_text.color =
            color;


        // 전체 지속시간 약 2초
        Sequence seq =
            DOTween.Sequence();


        // 위로 이동
        seq.Join(
            rect.DOLocalMove(
                endPos,
                2f
            )
        );


        // 천천히 나타남
        seq.Insert(
            0f,
            damage_text.DOFade(
                1f,
                0.3f
            )
        );


        // 1.6초부터 천천히 사라짐
        seq.Insert(
            1.6f,
            damage_text.DOFade(
                0f,
                0.4f
            )
        );


        // 종료 후 삭제
        seq.OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }
}
