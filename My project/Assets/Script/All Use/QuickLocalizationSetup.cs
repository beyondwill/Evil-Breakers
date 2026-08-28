using UnityEngine;
using TMPro;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;

public class QuickLocalizationSetup : MonoBehaviour
{
    public static QuickLocalizationSetup Instance { get; private set; }

    [SerializeField]
    private string tableName = "UI_Text";

    public enum TargetLanguage
    {
        Korean,
        English,
        Japanese,
        ChineseSimplified,
        ChineseTraditional
    }

    [Header("게임 시작 시 적용할 언어")]
    [SerializeField]
    private TargetLanguage targetLanguage = TargetLanguage.English;

    private bool isInitialized = false;

    // =========================================================
    // TMP → 원본 번역 Key
    // =========================================================

    private Dictionary<TextMeshProUGUI, string> localizedTexts
        = new Dictionary<TextMeshProUGUI, string>();


    // =========================================================
    // 초기화
    // =========================================================

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 씬 전환 감지
        SceneManager.sceneLoaded += OnSceneLoaded;

        // 언어 변경 감지
        LocalizationSettings.SelectedLocaleChanged
            += OnLanguageChanged;

        StartCoroutine(InitializeLocalizationSequence());
    }


    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            LocalizationSettings.SelectedLocaleChanged
                -= OnLanguageChanged;
        }
    }


    // =========================================================
    // Localization 초기화
    // =========================================================

    private IEnumerator InitializeLocalizationSequence()
    {
        // 1. 유니티 로컬라이제이션 시스템이 완전히 로드될 때까지 대기
        yield return LocalizationSettings.InitializationOperation;

        // 2. 시스템 안정화를 위한 1프레임 추가 대기 (영어 시작 시 핸들 꼬임 방지)
        yield return null;

        string targetCode = GetLanguageCode(targetLanguage);
        bool found = false;

        foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
        {
            string code = locale.Identifier.Code;

            if (code.Equals(targetCode, System.StringComparison.OrdinalIgnoreCase) || code.StartsWith(targetCode))
            {
                LocalizationSettings.SelectedLocale = locale;
                found = true;
                Debug.Log($"[Localization] 시작 언어 설정 완료: {code}");
                break;
            }
        }

        if (!found && LocalizationSettings.AvailableLocales.Locales.Count > 0)
        {
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[0];
            Debug.LogWarning("[Localization] 일치하는 언어를 찾지 못해 기본 언어를 사용합니다.");
        }

        PlayerPrefs.SetString("ForcedTargetLang", targetCode);
        PlayerPrefs.Save();

        // 3. 모든 준비가 끝난 후 초기화 플래그 활성화
        isInitialized = true;
    }


    // =========================================================
    // 언어 코드 매핑
    // =========================================================

    private string GetLanguageCode(TargetLanguage language)
    {
        switch (language)
        {
            case TargetLanguage.Korean: return "ko";
            case TargetLanguage.English: return "en";
            case TargetLanguage.Japanese: return "ja";
            case TargetLanguage.ChineseSimplified: return "zh-CN";
            case TargetLanguage.ChineseTraditional: return "zh-TW";
        }
        return "en";
    }


    // =========================================================
    // TMP 자동 검색 및 등록
    // =========================================================

    private void Update()
    {
        if (!isInitialized)
            return;

        TextMeshProUGUI[] texts =
            FindObjectsByType<TextMeshProUGUI>(
                FindObjectsSortMode.None);

        foreach (var tmp in texts)
        {
            if (tmp == null)
                continue;

            // 이미 등록된 TMP
            if (localizedTexts.ContainsKey(tmp))
                continue;

            string currentText = tmp.text;

            // 빈 텍스트
            if (string.IsNullOrEmpty(currentText))
                continue;

            // Localization 에러 텍스트
            if (currentText.Contains("No translation found"))
                continue;

            // 현재 텍스트를 원본 Key로 저장
            localizedTexts.Add(tmp, currentText);

            // 최초 번역
            TryTranslateText(tmp, currentText);
        }

        // 파괴된 TMP 정리
        CleanupNullReferences();
    }


    // =========================================================
    // 번역 처리
    // =========================================================

    private void TryTranslateText(TextMeshProUGUI tmp, string key)
    {
        if (tmp == null)
            return;

        string localizedString =
            LocalizationSettings.StringDatabase
                .GetLocalizedString(tableName, key);

        if (!string.IsNullOrEmpty(localizedString)
            && !localizedString.Contains("No translation found"))
        {
            tmp.text = localizedString;
        }
    }


    // =========================================================
    // Inspector 버튼 등에서 호출용
    // =========================================================

    public void ChangeLanguage(TargetLanguage language)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[Localization] 아직 초기화되지 않았습니다.");
            return;
        }

        string targetCode = GetLanguageCode(language);

        foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
        {
            string code = locale.Identifier.Code;

            if (code.Equals(targetCode, System.StringComparison.OrdinalIgnoreCase) || code.StartsWith(targetCode))
            {
                LocalizationSettings.SelectedLocale = locale;
                targetLanguage = language;

                PlayerPrefs.SetString("ForcedTargetLang", targetCode);
                PlayerPrefs.Save();

                Debug.Log($"[Localization] 언어 변경: {code}");
                return;
            }
        }

        Debug.LogWarning($"[Localization] 언어를 찾을 수 없습니다: {targetCode}");
    }


    // =========================================================
    // 언어 변경 이벤트
    // =========================================================

    private void OnLanguageChanged(UnityEngine.Localization.Locale newLocale)
    {
        if (!isInitialized)
            return;

        Debug.Log($"[Localization] 언어 변경 감지: {newLocale.Identifier.Code}");
        RefreshAllTexts();
    }


    // =========================================================
    // 모든 텍스트 및 카드 뷰 갱신
    // =========================================================

    private void RefreshAllTexts()
    {
        // 1. 일반 TMP 갱신
        List<KeyValuePair<TextMeshProUGUI, string>> textList =
            new List<KeyValuePair<TextMeshProUGUI, string>>(localizedTexts);

        foreach (var pair in textList)
        {
            TextMeshProUGUI tmp = pair.Key;
            string key = pair.Value;

            if (tmp == null)
            {
                localizedTexts.Remove(tmp);
                continue;
            }

            TryTranslateText(tmp, key);
        }

        CleanupNullReferences();

        // 2. 현재 씬의 모든 CardView 갱신 (존재하는 경우)
        CardView[] cardViews =
            FindObjectsByType<CardView>(
                FindObjectsSortMode.None);

        foreach (var cardView in cardViews)
        {
            if (cardView == null)
                continue;

            cardView.RefreshLocalization();
        }
    }


    // =========================================================
    // 파괴된 TMP 정리
    // =========================================================

    private void CleanupNullReferences()
    {
        List<TextMeshProUGUI> removeList = new List<TextMeshProUGUI>();

        foreach (var pair in localizedTexts)
        {
            if (pair.Key == null)
            {
                removeList.Add(pair.Key);
            }
        }

        foreach (var tmp in removeList)
        {
            localizedTexts.Remove(tmp);
        }
    }


    // =========================================================
    // 씬 전환 처리
    // =========================================================

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        localizedTexts.Clear();
        Debug.Log($"[Localization] 씬 변경 → Dictionary 초기화: {scene.name}");
    }
}