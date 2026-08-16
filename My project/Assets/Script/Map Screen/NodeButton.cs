using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NodeButton : MonoBehaviour
{
    // 외부 요소
    [SerializeField] private Button button;
    [SerializeField] private Image button_color;
    [SerializeField] private Image button_image;
    [SerializeField] private Image character_image;

    [SerializeField] private GameObject key_box;                // 키 박스
    [SerializeField] private TextMeshProUGUI key_text;          // 키 텍스트
    [SerializeField] private GameObject character_icon;         // 캐릭터 아이콘

    [SerializeField] private MapNodeConfig mapNodeConfig;       // 맵 노드 정보
    [SerializeField] private ZoneNodeConfig zoneNodeConfig;     // 구역 노드 정보

    // 변수
    [SerializeField] private Color button_uninteractable_color;
    [SerializeField] private CanvasGroup hideInfoGroup;
    [SerializeField] private HexNode node;

    void Awake()
    {
        HideKeyBox();
    }

    // 버튼 클릭 가능 여부 변경
    public void ButtonClickToggle(bool click_possible)
    {
        button.interactable = click_possible;

        Color targetColor = click_possible
            ? Color.white
            : button_uninteractable_color;

        button_image.DOColor(targetColor, 0.5f);
    }

    // 버튼 색상 변경
    public void ChangeButtonColor(HexNode.NodeType type, bool is_visited, float duration = 0.5f)
    {
        // 1. 색상 결정 로직
        Color targetColor;

        if (!is_visited || type == HexNode.NodeType.Start)
        {
            var data = mapNodeConfig.GetMapNodeData(type);
            // 데이터가 없으면 기본값(흰색)을 사용하거나 에러 방지
            targetColor = (data != null) ? data.zone_color : Color.white;
        }
        else
        {
            targetColor = mapNodeConfig.isVisitedColor;
        }

        // 2. DOTween으로 부드럽게 변경
        button_color.DOColor(targetColor, duration);
    }

    // 키 박스 보여주기
    public void ShowKeyBox(string s)
    {
        key_box.SetActive(true);
        key_text.text = s;
    }

    // 키 박스 숨기기
    public void HideKeyBox()
    {
        key_box.SetActive(false);
    }

    // 플레이어 캐릭터 얼굴 집어넣기
    public void SetPlayerCharacterFace(Sprite face)
    {
        character_image.sprite = face;
    }

    // 버튼 정보 보여주기
    public void RevealButtonInfo(float time = 0.5f)
    {
        hideInfoGroup.DOFade(0f, time)
            .OnComplete(() =>
            {
                hideInfoGroup.gameObject.SetActive(false);
            });
    }
    public void FaceShow(float time = 0.5f)
    {
        // 이미 진행 중인 트윈이 있다면 중단 (버그 방지)
        character_icon.transform.DOKill();

        // 활성화 및 스케일 초기화
        character_icon.SetActive(true);
        character_icon.transform.localScale = Vector3.zero;

        // OutBack으로 튀어 오르는 효과 적용
        character_icon.transform.DOScale(Vector3.one, time)
            .SetEase(Ease.OutBack);
    }

    public void FaceHide(float time = 0.5f)
    {
        // 이미 진행 중인 트윈이 있다면 중단
        character_icon.transform.DOKill();

        // InBack을 사용하면 작아질 때도 자연스러운 느낌을 줍니다
        character_icon.transform.DOScale(Vector3.zero, time)
            .SetEase(Ease.InBack)
            .OnComplete(() => character_icon.SetActive(false)); // 애니메이션 끝난 후 비활성화
    }

    public void SetZoneImage(HexNode.ZoneType zone_type)
    {
        button_image.sprite = zoneNodeConfig.GetZoneNodeData(zone_type).zone_icon_sprite;
    }
}
