using UnityEngine;
using TMPro;
using UnityEngine.Localization.Settings;

public class AutoLocalization : MonoBehaviour
{
    private void Start()
    {
        // 초기화 완료 후 텍스트 체크 및 번역 적용
        LocalizationSettings.InitializationOperation.Completed += op =>
        {
            ApplyLocalization();
            LocalizationSettings.SelectedLocaleChanged += locale => ApplyLocalization();
        };
    }

    private void ApplyLocalization()
    {
        TextMeshProUGUI[] texts = FindObjectsOfType<TextMeshProUGUI>();

        foreach (var tmp in texts)
        {
            if (tmp == null) continue;

            // 현재 적혀있는 텍스트가 키값이라고 가정 (처음에 GAME_TITLE 등으로 적어둔 경우)
            string key = tmp.text;

            if (!string.IsNullOrEmpty(key) && !key.Contains("No translation found"))
            {
                // 현재 선택된 로케일의 테이블에서 키에 해당하는 값을 가져옴 (에러 발생 안 함)
                string localizedText = LocalizationSettings.StringDatabase.GetLocalizedString("UI_Text", key);

                // 번역 결과가 유효하고 에러 문구가 아닐 때만 텍스트 변경
                if (!string.IsNullOrEmpty(localizedText) &&
                    !localizedText.StartsWith("No translation found") &&
                    localizedText != key)
                {
                    tmp.text = localizedText;
                }
                // 값이 없으면 기존 텍스트를 절대 건드리지 않고 그대로 둡니다.
            }
        }
    }
}