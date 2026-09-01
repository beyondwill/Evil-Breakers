// 타겟 타입
public enum TargetType
{
    None,
    Player,                 // 플레이어 자기 자신
    Self,                   // 해당 캐릭터 자신
    RandomEnemy,            // 무작위 적
    AllAllys,               // 모든 아군 캐릭터
    AllEnemies,             // 모든 적군 캐릭터
    LowestHpEnemy,          // 생명력이 가장 낮은 적
    HighestHpEnemy,         // 생명력이 가장 높은 적
    SelectedTarget,         // 선택한 대상(카드를 냈을 때)
}

// 효과 타입
public enum EffectType
{
    None,                   // 없음
    DrawCard,               // 카드 뽑기
    GainArmor,              // 방어도 획득
    GainAttack,             // 공격력 획득
    GainGold,               // 골드 획득
    SearchRange,            // 탐지 범위
    GetEnergy,              // 에너지 얻기
    GainBuff                // 버프 부여
}

// 유물 효과 발동
[System.Serializable]
public class EffectValue
{
    public TargetType targetType;       // 목표 대상
    public EffectType effectType;       // 효과 발휘
    public float value;                 // 밸류
    public CardData cardInfo;           // 대상에게 발휘되는 카드 효과
}