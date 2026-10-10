using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Card/Effects/ChooseAllyCharacter")]
public class ChooseAllyCharacterEffect : CardEffect
{
    public override void Execute(
        CharacterVariable caster,
        List<CharacterVariable> targets,
        CardEffectEntry entry,
        CardData card)
    {
        if (caster == null ||
            targets == null ||
            entry == null ||
            entry.valueList == null ||
            entry.valueList.Count == 0)
            return;

        targets.Clear();

        int characterCount = entry.valueList[0];

        if (characterCount <= 0)
            return;

        // 시전자와 같은 진영의 캐릭터 목록
        List<CharacterVariable> candidateList;

        if (caster.is_player_character)
        {
            // 아군이 사용 → 아군 선택
            candidateList =
                BattleCharacterManager.Instance.PlayerCharacters;
        }
        else
        {
            // 적이 사용 → 적 선택
            candidateList =
                BattleCharacterManager.Instance.EnemyCharacters;
        }

        if (candidateList == null ||
            candidateList.Count == 0)
            return;

        // 살아 있는 캐릭터만 후보로 등록
        List<CharacterVariable> candidates =
            new List<CharacterVariable>();

        foreach (CharacterVariable character in candidateList)
        {
            if (character == null)
                continue;

            if (character.is_dead)
                continue;

            candidates.Add(character);
        }

        // 가능한 수만큼 선택
        int selectCount =
            Mathf.Min(characterCount, candidates.Count);

        // 중복 없이 랜덤 선택
        for (int i = 0; i < selectCount; i++)
        {
            int randomIndex =
                Random.Range(0, candidates.Count);

            CharacterVariable selected =
                candidates[randomIndex];

            targets.Add(selected);

            // 선택한 캐릭터는 후보에서 제거
            candidates.RemoveAt(randomIndex);
        }
    }
}