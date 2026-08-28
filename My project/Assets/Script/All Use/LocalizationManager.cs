using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Components;
using TMPro; // TextMeshPro를 사용하기 위해 추가

public class LocalizationManager : MonoBehaviour
{
    private IEnumerator Start()
    {
        // 로컬라이제이션 시스템 초기화 대기
        yield return LocalizationSettings.InitializationOperation;

        string language = PlayerPrefs.GetString("Language", "ko-KR");
        Debug.Log("불러온 언어 설정: " + language);

        var locale = LocalizationSettings.AvailableLocales.GetLocale(language);

        if (locale != null)
        {
            LocalizationSettings.SelectedLocale = locale;
            Debug.Log("적용된 로케일: " + locale.Identifier.Code);
        }

        // 약간의 프레임 지연을 주어 로케일 변경 반영 보장
        yield return null;

        // 씬 내의 모든 LocalizeStringEvent 컴포넌트 강제 갱신
        var localizeEvents = FindObjectsOfType<LocalizeStringEvent>();
        foreach (var ev in localizeEvents)
        {
            ev.RefreshString();
        }
    }
}