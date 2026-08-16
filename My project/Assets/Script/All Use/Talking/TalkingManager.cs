using System.Collections.Generic;
using UnityEngine;

// 대화를 나누는 장소
public enum TalkingState
{
    Main,                       // 메인 화면
    BattleMap                   // 전투 화면
}

public class TalkingManager : MonoBehaviour
{
    public static TalkingManager Instance { get; private set; }

    [Header("변수")]
    [SerializeField] private TalkingState TS;
    [SerializeField] private float waitingTime = 5f;

    [Header("TalkingBox")]
    [SerializeField] private List<TalkingBoxUI> talkingBoxUIList;

    [SerializeField] private float currentTime;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        ShowTalkingBox();
    }

    private void ShowTalkingBox()
    {
        currentTime += Time.deltaTime;

        if (currentTime < waitingTime)
            return;

        TalkOnCharacter();

        currentTime = 0f;
    }

    // 랜덤 캐릭터에게 대화 출력
    private void TalkOnCharacter()
    {
        if (talkingBoxUIList == null ||
            talkingBoxUIList.Count == 0)
            return;

        // 랜덤 TalkingBoxUI 선택

        int choose_one = Random.Range(0, talkingBoxUIList.Count);

        TalkingBoxUI talkingBox =
            talkingBoxUIList[
                choose_one
            ];

        // 현재 장소에 맞는 대사 출력
        switch (TS)
        {
            case TalkingState.Main:
                talkingBox.Talking(
                    Situation.MainNormal
                );
                break;

            case TalkingState.BattleMap:
                talkingBox.Talking(
                    Situation.BattleMapNormal
                );
                break;
        }
    }

    // 대화 상자에 추가
    public void AddTalkingBox(TalkingBoxUI TBU)
    {
        talkingBoxUIList.Add(TBU);
    }
}