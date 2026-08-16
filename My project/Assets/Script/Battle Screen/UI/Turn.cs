using UnityEngine;
using DG.Tweening;
using TMPro;

public class Turn : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    // 변수
    private Vector3 initScale;
    [SerializeField] private float bigger_time = 1.5f;
    [SerializeField] private float smaller_time = 0.2f;


    void Awake()
    {
        // 처음 스케일 저장
        initScale = transform.localScale;
    }

    public void Init(string s)
    {
        text.text = s;
    }

    void OnEnable()
    {
        // 시작할 때 0으로
        transform.localScale = Vector3.zero;

        Sequence seq = DOTween.Sequence();

        seq.Append(transform.DOScale(initScale, bigger_time));
        seq.Append(transform.DOScale(Vector3.zero, smaller_time));
        seq.OnComplete(() => gameObject.SetActive(false));
    }
}