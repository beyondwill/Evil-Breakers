using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class ConversationManager : MonoBehaviour
{
    [Header("Conversation Box")]
    [SerializeField] private ConversationBoxUI conversationBoxUI;

    [Header("변수")]
    [SerializeField] private float waitingTime = 5f;
    [SerializeField] private float currentTime;

    [Header("Characters")]
    [SerializeField]
    private List<PlayerCharacterData> characters =
        new List<PlayerCharacterData>();

    private void Start()
    {
        switch (DataManager.Instance.GetAllData.current_state)
        {
            case CurrentState.MainScreen:
                characters = DataManager.Instance.GetMainData.player_character_data_list;
                break;
            default:
                characters = DataManager.Instance.GetBattleData.characters_in_battle_data_list;
                if (DataManager.Instance.GetAllData.current_state == CurrentState.BattleBegin)
                {
                    TalkOnCharacter();
                    gameObject.SetActive(false);
                }
                break;
        }

        currentTime = 0f;
    }

    private void Update()
    {
        ShowTalkingBox();
    }

    private void ShowTalkingBox()
    {
        // 대화 중이면 새로운 대화 호출 안 함
        if (conversationBoxUI.gameObject.activeSelf)
            return;

        currentTime += Time.deltaTime;

        if (currentTime < waitingTime)
            return;

        TalkOnCharacter();

        currentTime = 0f;
    }

    // 랜덤 캐릭터에게 대화 출력
    private void TalkOnCharacter()
    {
        if (characters == null ||
            characters.Count == 0)
            return;

        // 랜덤 캐릭터 선택
        int randomIndex =
            Random.Range(0, characters.Count);

        PlayerCharacterData randomCharacter =
            characters[randomIndex];

        if (randomCharacter == null)
            return;

        Situation situation = GetCurrentSituation();

        // 해당 캐릭터의 현재 상황 대사 출력
        conversationBoxUI.Talking(
            randomCharacter,
            situation
        );
    }

    // 현재 게임 상태에 따라 Situation 반환
    private Situation GetCurrentSituation()
    {
        switch (DataManager.Instance.GetAllData.current_state)
        {
            case CurrentState.MainScreen:
                return Situation.MainNormal;

            case CurrentState.BattleMap:
                return Situation.BattleMapNormal;

            case CurrentState.BattleBegin:
                return Situation.BattleStart;

            default:
                return Situation.MainNormal;
        }
    }
}