// =========================================================
// 패시브 트리거 컨텍스트
// =========================================================

public class PassiveTriggerContext
{
    // =====================================================
    // 어떤 이벤트인가?
    // =====================================================

    public PassiveTriggerType triggerType;


    // =====================================================
    // 이벤트를 발생시킨 캐릭터
    // =====================================================

    public CharacterVariable source;


    // =====================================================
    // 이벤트의 대상
    // =====================================================

    public CharacterVariable target;


    // =====================================================
    // 이벤트 관련 수치
    // =====================================================

    public float value;


    // =====================================================
    // Constructor
    // =====================================================

    public PassiveTriggerContext(
        PassiveTriggerType triggerType,
        CharacterVariable source = null,
        CharacterVariable target = null,
        float value = 0)
    {
        this.triggerType = triggerType;

        this.source = source;

        this.target = target;

        this.value = value;
    }
}