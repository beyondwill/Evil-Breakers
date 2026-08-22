using UnityEngine;
using UnityEngine.SceneManagement;

public class ConversationManager : MonoBehaviour
{
    // =========================================================
    // Instance
    // =========================================================

    public static ConversationManager Instance { get; private set; }


    // =========================================================
    // Conversation Box
    // =========================================================

    [Header("Conversation Box")]
    [SerializeField]
    private ConversationBoxUI conversationBoxUI;


    // =========================================================
    // ConversationSO
    // =========================================================

    // 현재 재생 중인 ConversationSO
    private ConversationSO currentConversation;

    // 현재 몇 번째 대화인지
    private int conversationIndex;

    // ConversationSO 대화 재생 중인지
    private bool isConversationPlaying;


    // =========================================================
    // Test
    // =========================================================

    [Header("Test")]
    public ConversationSO conver;

    public ConversationSO conver2;

    public ConversationSO conver3;

    public ConversationSO conver4;

    // =========================================================
    // Awake
    // =========================================================

    private void Awake()
    {
        // 현재 씬의 ConversationManager를 Instance로 지정
        Instance = this;
    }


    // =========================================================
    // Start
    // =========================================================

    private void Start()
    {
        // ConversationBox 연결
        if (conversationBoxUI != null)
        {
            conversationBoxUI.OnConversationComplete =
                OnConversationComplete;
        }


        // =====================================================
        // 테스트
        // =====================================================

        if (
            DataManager.Instance.GetMainData.day == 1 &&
            DataManager.Instance.GetAllData.current_state ==
            CurrentState.MainScreen &&
            SceneManager.GetActiveScene().name == "Main Scene"
        )
        {
            Debug.Log("대화 시작!");

            StartConversation(conver4);
        }

        if (
            DataManager.Instance.GetMainData.day == 2 &&
            DataManager.Instance.GetAllData.current_state ==
            CurrentState.MainScreen &&
            SceneManager.GetActiveScene().name == "Main Scene"
        )
        {
            Debug.Log("대화 시작!");

            StartConversation(conver);
        }
    }


    // =========================================================
    // ConversationSO 시작
    // =========================================================

    public void StartConversation(
        ConversationSO conversation)
    {
        if (conversation == null)
        {
            Debug.LogWarning(
                "ConversationManager : ConversationSO가 없습니다."
            );

            return;
        }


        if (
            conversation.CACList == null ||
            conversation.CACList.Count == 0
        )
        {
            Debug.LogWarning(
                "ConversationManager : 대화가 비어있습니다."
            );

            return;
        }


        // 이미 대화 중이면 무시
        if (isConversationPlaying)
        {
            Debug.LogWarning(
                "ConversationManager : 이미 대화 중입니다."
            );

            return;
        }


        // 현재 대화 설정
        currentConversation =
            conversation;


        // 첫 번째 대화
        conversationIndex = 0;


        // 대화 중
        isConversationPlaying = true;


        // 첫 대사 출력
        ShowConversation();
    }


    // =========================================================
    // 대화 출력
    // =========================================================

    private void ShowConversation()
    {
        if (currentConversation == null)
        {
            EndConversation();
            return;
        }


        if (
            currentConversation.CACList == null ||
            currentConversation.CACList.Count == 0
        )
        {
            EndConversation();
            return;
        }


        // 모든 대화 종료
        if (
            conversationIndex >=
            currentConversation.CACList.Count
        )
        {
            EndConversation();
            return;
        }


        CharacterAndConversation data =
            currentConversation.CACList[
                conversationIndex
            ];


        // 잘못된 데이터
        if (
            data == null ||
            data.characterInfo == null ||
            string.IsNullOrEmpty(
                data.character_conversation_text
            )
        )
        {
            conversationIndex++;

            ShowConversation();

            return;
        }


        if (conversationBoxUI == null)
        {
            Debug.LogError(
                "ConversationManager : ConversationBoxUI가 연결되지 않았습니다."
            );

            EndConversation();

            return;
        }


        // 캐릭터 + 대화 출력
        conversationBoxUI.Talking(
            data.characterInfo,
            data.character_conversation_text
        );
    }


    // =========================================================
    // 한 줄 대화 종료
    // =========================================================

    private void OnConversationComplete()
    {
        if (!isConversationPlaying)
            return;


        // 다음 대화
        conversationIndex++;


        // 다음 대화가 있으면 출력
        if (
            currentConversation != null &&
            currentConversation.CACList != null &&
            conversationIndex <
            currentConversation.CACList.Count
        )
        {
            ShowConversation();

            return;
        }


        // 모든 대화 종료
        EndConversation();
    }


    // =========================================================
    // 대화 종료
    // =========================================================

    private void EndConversation()
    {
        isConversationPlaying = false;

        conversationIndex = 0;

        currentConversation = null;

        Debug.Log("Conversation 종료");
    }


    // =========================================================
    // 대화 중인지 확인
    // =========================================================

    public bool IsConversationPlaying()
    {
        return isConversationPlaying;
    }
}
