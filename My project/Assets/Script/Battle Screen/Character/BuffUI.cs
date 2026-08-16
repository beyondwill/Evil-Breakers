using System.Collections.Generic;
using UnityEngine;

public class BuffUI : MonoBehaviour
{
    // 외부 요소
    [SerializeField] private Transform buffIconHLG;
    [SerializeField] private GameObject buffIconPrefab;


    // ========================================
    // 버프 아이콘 보여주기
    // ========================================

    public void ShowBuffIcons(
        List<CharacterBuffValue> buffList)
    {
        // 기존 아이콘 전부 제거
        foreach (Transform child in buffIconHLG)
        {
            Destroy(child.gameObject);
        }


        // 버프가 없으면 종료
        if (buffList == null ||
            buffList.Count == 0)
        {
            return;
        }


        // ========================================
        // 값이 0인 버프는 제외
        // ========================================

        foreach (CharacterBuffValue CBV in buffList)
        {
            if (CBV == null)
                continue;

            if (CBV.value == 0)
                continue;


            // ========================================
            // 새 아이콘 생성
            // ========================================

            GameObject buffIcon =
                Instantiate(
                    buffIconPrefab,
                    buffIconHLG
                );


            BuffIconPrefab buffIconComponent =
                buffIcon.GetComponent<BuffIconPrefab>();


            if (buffIconComponent == null)
            {
                Debug.LogError(
                    "BuffIconPrefab 컴포넌트를 찾을 수 없음!"
                );

                continue;
            }


            buffIconComponent.BuffInit(CBV);
        }
    }
}