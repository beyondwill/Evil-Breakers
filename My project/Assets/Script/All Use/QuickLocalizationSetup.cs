using UnityEngine;
using TMPro;
using UnityEngine.Localization;
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

    // =========================================================
    // 언어별 TMP Font Asset
    // =========================================================

    [Header("언어별 TMP Font Asset")]
    [SerializeField] private TMP_FontAsset koreanFont;
    [SerializeField] private TMP_FontAsset englishFont;
    [SerializeField] private TMP_FontAsset japaneseFont;
    [SerializeField] private TMP_FontAsset chineseSimplifiedFont;
    [SerializeField] private TMP_FontAsset chineseTraditionalFont;

    // =========================================================
    // Localization에서 완전히 제외할 TMP
    // =========================================================

    [Header("Localization에서 제외할 TMP")]
    [SerializeField]
    private List<TextMeshProUGUI> excludedTexts =
        new List<TextMeshProUGUI>();

    private bool isInitialized = false;

    // =========================================================
    // TMP → Localization Key
    // =========================================================

    private Dictionary<TextMeshProUGUI, string> localizedTexts =
        new Dictionary<TextMeshProUGUI, string>();

    // =========================================================
    // TMP → 마지막으로 Localization 시스템이 적용한 문자열
    // =========================================================

    private Dictionary<TextMeshProUGUI, string> lastLocalizedValues =
        new Dictionary<TextMeshProUGUI, string>();

    // =========================================================
    // 런타임 TMP 자동 검색
    // =========================================================

    [Header("런타임 TMP 자동 검색")]
    [SerializeField]
    private float dynamicScanInterval = 0.1f;

    private float dynamicScanTimer = 0f;

    // =========================================================
    // 초기화
    // =========================================================

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        // 씬 전환 감지
        SceneManager.sceneLoaded += OnSceneLoaded;

        // 언어 변경 감지
        LocalizationSettings.SelectedLocaleChanged +=
            OnLanguageChanged;

        StartCoroutine(
            InitializeLocalizationSequence()
        );
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;

            LocalizationSettings.SelectedLocaleChanged -=
                OnLanguageChanged;

            Instance = null;
        }
    }

    // =========================================================
    // Update
    // =========================================================

    private void Update()
    {
        if (!isInitialized)
            return;

        // 이미 등록된 TMP의 text 변경 감지
        CheckRegisteredTexts();

        // 새로운 TMP 검색
        dynamicScanTimer +=
            Time.unscaledDeltaTime;

        if (dynamicScanTimer >= dynamicScanInterval)
        {
            dynamicScanTimer = 0f;

            ScanNewTexts();
        }
    }

    // =========================================================
    // Localization 제외 TMP 확인
    // =========================================================

    private bool IsExcludedTMP(
        TextMeshProUGUI tmp)
    {
        if (tmp == null)
            return false;

        return excludedTexts.Contains(tmp);
    }

    // =========================================================
    // Localization 초기화
    // =========================================================

    private IEnumerator InitializeLocalizationSequence()
    {
        // Unity Localization 초기화 대기
        yield return LocalizationSettings.InitializationOperation;

        // 한 프레임 대기
        yield return null;

        string targetCode =
            GetLanguageCode(targetLanguage);

        bool found = false;

        foreach (var locale in
                 LocalizationSettings
                     .AvailableLocales
                     .Locales)
        {
            if (locale == null)
                continue;

            string code =
                locale.Identifier.Code;

            if (code.Equals(
                    targetCode,
                    System.StringComparison.OrdinalIgnoreCase)
                ||
                code.StartsWith(targetCode))
            {
                LocalizationSettings.SelectedLocale =
                    locale;

                found = true;

                Debug.Log(
                    $"[Localization] 시작 언어 설정 완료: {code}"
                );

                break;
            }
        }

        if (!found &&
            LocalizationSettings
                .AvailableLocales
                .Locales
                .Count > 0)
        {
            LocalizationSettings.SelectedLocale =
                LocalizationSettings
                    .AvailableLocales
                    .Locales[0];

            Debug.LogWarning(
                "[Localization] 일치하는 언어를 찾지 못해 기본 언어를 사용합니다."
            );
        }

        PlayerPrefs.SetString(
            "ForcedTargetLang",
            targetCode
        );

        PlayerPrefs.Save();

        isInitialized = true;

        // 현재 언어 폰트 적용
        ApplyFontToAllTexts(targetLanguage);

        // 초기 씬 TMP 검색
        ScanExistingTexts();
    }

    // =========================================================
    // 언어 코드 매핑
    // =========================================================

    private string GetLanguageCode(
        TargetLanguage language)
    {
        switch (language)
        {
            case TargetLanguage.Korean:
                return "ko";

            case TargetLanguage.English:
                return "en";

            case TargetLanguage.Japanese:
                return "ja";

            case TargetLanguage.ChineseSimplified:
                return "zh-CN";

            case TargetLanguage.ChineseTraditional:
                return "zh-TW";
        }

        return "en";
    }

    // =========================================================
    // 언어별 Font Asset 가져오기
    // =========================================================

    private TMP_FontAsset GetFont(
        TargetLanguage language)
    {
        switch (language)
        {
            case TargetLanguage.Korean:
                return koreanFont;

            case TargetLanguage.English:
                return englishFont;

            case TargetLanguage.Japanese:
                return japaneseFont;

            case TargetLanguage.ChineseSimplified:
                return chineseSimplifiedFont;

            case TargetLanguage.ChineseTraditional:
                return chineseTraditionalFont;
        }

        return englishFont;
    }

    // =========================================================
    // 모든 TMP에 현재 언어 Font 적용
    // =========================================================

    private void ApplyFontToAllTexts(
        TargetLanguage language)
    {
        TMP_FontAsset font =
            GetFont(language);

        if (font == null)
        {
            Debug.LogWarning(
                $"[Localization] {language} Font Asset이 등록되지 않았습니다."
            );

            return;
        }

        TextMeshProUGUI[] texts =
            FindObjectsByType<TextMeshProUGUI>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        foreach (var tmp in texts)
        {
            if (tmp == null)
                continue;

            // =================================================
            // 제외 TMP
            // =================================================
            // 폰트조차 변경하지 않는다.
            if (IsExcludedTMP(tmp))
                continue;

            tmp.font = font;
        }

        Debug.Log(
            $"[Localization] 전체 TMP Font 변경: {language}"
        );
    }

    // =========================================================
    // 기존 TMP 검색
    // =========================================================

    private void ScanExistingTexts()
    {
        TextMeshProUGUI[] texts =
            FindObjectsByType<TextMeshProUGUI>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        foreach (var tmp in texts)
        {
            if (tmp == null)
                continue;

            // =================================================
            // Localization 완전 제외
            // =================================================

            if (IsExcludedTMP(tmp))
                continue;

            string currentText =
                tmp.text;

            if (string.IsNullOrEmpty(currentText))
                continue;

            if (currentText.Contains(
                    "No translation found"))
                continue;

            // 현재 언어 Font 적용
            ApplyCurrentFont(tmp);

            // 이미 등록된 TMP
            if (localizedTexts.ContainsKey(tmp))
            {
                TryTranslateText(
                    tmp,
                    localizedTexts[tmp]
                );

                continue;
            }

            // 처음 발견된 TMP
            RegisterText(
                tmp,
                currentText
            );
        }

        CleanupNullReferences();
    }

    // =========================================================
    // 새 TMP 검색
    // =========================================================

    private void ScanNewTexts()
    {
        TextMeshProUGUI[] texts =
            FindObjectsByType<TextMeshProUGUI>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        foreach (var tmp in texts)
        {
            if (tmp == null)
                continue;

            // =================================================
            // Localization 완전 제외
            // =================================================

            if (IsExcludedTMP(tmp))
                continue;

            // 이미 등록된 TMP
            if (localizedTexts.ContainsKey(tmp))
                continue;

            string currentText =
                tmp.text;

            // 아직 텍스트가 없는 TMP
            if (string.IsNullOrEmpty(currentText))
                continue;

            // Localization 오류 문자열
            if (currentText.Contains(
                    "No translation found"))
                continue;

            Debug.Log(
                $"[Localization] 새 TMP 발견: " +
                $"{tmp.gameObject.name} / " +
                $"Key = {currentText}"
            );

            RegisterText(
                tmp,
                currentText
            );
        }

        CleanupNullReferences();
    }

    // =========================================================
    // 현재 언어 Font 적용
    // =========================================================

    private void ApplyCurrentFont(
        TextMeshProUGUI tmp)
    {
        if (tmp == null)
            return;

        // 제외 TMP는 폰트도 변경하지 않는다.
        if (IsExcludedTMP(tmp))
            return;

        TMP_FontAsset font =
            GetFont(targetLanguage);

        if (font == null)
            return;

        if (tmp.font != font)
        {
            tmp.font = font;
        }
    }

    // =========================================================
    // 등록된 TMP의 text 변경 감지
    // =========================================================

    private void CheckRegisteredTexts()
    {
        List<KeyValuePair<TextMeshProUGUI, string>> textList =
            new List<KeyValuePair<TextMeshProUGUI, string>>(
                localizedTexts
            );

        foreach (var pair in textList)
        {
            TextMeshProUGUI tmp =
                pair.Key;

            string key =
                pair.Value;

            if (tmp == null)
                continue;

            // =================================================
            // Localization 제외 TMP
            // =================================================

            if (IsExcludedTMP(tmp))
                continue;

            if (string.IsNullOrEmpty(key))
                continue;

            // 현재 언어 Font가 아니라면 다시 적용
            ApplyCurrentFont(tmp);

            // 마지막으로 Localization이 적용한 값
            string lastValue = "";

            lastLocalizedValues.TryGetValue(
                tmp,
                out lastValue
            );

            // 우리가 마지막으로 넣었던 값과 같음
            if (tmp.text == lastValue)
                continue;

            // 아직 번역되지 않은 상태
            if (tmp.text == key)
            {
                StartCoroutine(
                    TranslateTextCoroutine(
                        tmp,
                        key
                    )
                );

                continue;
            }

            // 다른 코드가 TMP.text를 변경한 경우
            string newKey =
                tmp.text;

            if (string.IsNullOrEmpty(newKey))
                continue;

            if (newKey.Contains(
                    "No translation found"))
                continue;

            Debug.Log(
                $"[Localization] TMP Key 변경 감지: " +
                $"{key} → {newKey}"
            );

            localizedTexts[tmp] =
                newKey;

            StartCoroutine(
                TranslateTextCoroutine(
                    tmp,
                    newKey
                )
            );
        }

        CleanupNullReferences();
    }

    // =========================================================
    // 동적 TMP 등록
    // =========================================================

    public void RegisterText(
        TextMeshProUGUI tmp,
        string key)
    {
        if (tmp == null)
            return;

        // =====================================================
        // Localization 완전 제외
        // =====================================================

        if (IsExcludedTMP(tmp))
            return;

        if (string.IsNullOrEmpty(key))
            return;

        // =====================================================
        // 현재 언어 Font 적용
        // =====================================================

        ApplyCurrentFont(tmp);

        // =====================================================
        // Dictionary 등록
        // =====================================================

        if (!localizedTexts.ContainsKey(tmp))
        {
            localizedTexts.Add(
                tmp,
                key
            );
        }
        else
        {
            localizedTexts[tmp] =
                key;
        }

        // =====================================================
        // 초기화 전이면 Dictionary에만 등록
        // =====================================================

        if (!isInitialized)
            return;

        // =====================================================
        // 번역
        // =====================================================

        StartCoroutine(
            TranslateTextCoroutine(
                tmp,
                key
            )
        );
    }

    // =========================================================
    // 번역 시도
    // =========================================================

    private void TryTranslateText(
        TextMeshProUGUI tmp,
        string key)
    {
        if (tmp == null)
            return;

        // 제외 TMP
        if (IsExcludedTMP(tmp))
            return;

        if (string.IsNullOrEmpty(key))
            return;

        if (!isInitialized)
            return;

        ApplyCurrentFont(tmp);

        StartCoroutine(
            TranslateTextCoroutine(
                tmp,
                key
            )
        );
    }

    // =========================================================
    // 실제 비동기 번역
    // =========================================================

    private IEnumerator TranslateTextCoroutine(
        TextMeshProUGUI tmp,
        string key)
    {
        if (tmp == null)
            yield break;

        // =====================================================
        // Localization 완전 제외
        // =====================================================

        if (IsExcludedTMP(tmp))
            yield break;

        if (string.IsNullOrEmpty(key))
            yield break;

        if (!isInitialized)
            yield break;

        // 번역 시작 시 현재 언어 Font 적용
        ApplyCurrentFont(tmp);

        var operation =
            LocalizationSettings
                .StringDatabase
                .GetLocalizedStringAsync(
                    tableName,
                    key
                );

        yield return operation;

        if (tmp == null)
            yield break;

        // 번역 도중 제외된 경우
        if (IsExcludedTMP(tmp))
            yield break;

        // =====================================================
        // Operation 실패
        // =====================================================

        if (operation.Status !=
            UnityEngine
                .ResourceManagement
                .AsyncOperations
                .AsyncOperationStatus
                .Succeeded)
        {
            Debug.LogWarning(
                $"[Localization] 번역 실패: " +
                $"Table = {tableName}, Key = {key}"
            );

            yield break;
        }

        string localizedString =
            operation.Result;

        // =====================================================
        // 번역 결과 적용
        // =====================================================

        if (!string.IsNullOrEmpty(localizedString)
            &&
            !localizedString.Contains(
                "No translation found"))
        {
            // 요청 중 Key가 변경됐는지 확인
            if (localizedTexts.TryGetValue(
                    tmp,
                    out string currentKey
                ))
            {
                if (currentKey != key)
                    yield break;
            }

            // 혹시 번역 도중 제외된 경우
            if (IsExcludedTMP(tmp))
                yield break;

            // 혹시 번역 도중 언어가 변경됐을 경우
            ApplyCurrentFont(tmp);

            // 실제 적용
            tmp.text =
                localizedString;

            // 우리가 적용한 값 기록
            lastLocalizedValues[tmp] =
                localizedString;
        }
    }

    // =========================================================
    // 외부 스크립트에서 Localization 문자열 가져오기
    // =========================================================

    public string GetLocalizedString(
        string key)
    {
        if (string.IsNullOrEmpty(key))
            return string.Empty;

        if (!isInitialized)
            return key;

        string localizedString =
            LocalizationSettings
                .StringDatabase
                .GetLocalizedString(
                    tableName,
                    key
                );

        if (string.IsNullOrEmpty(localizedString)
            ||
            localizedString.Contains(
                "No translation found"))
        {
            return key;
        }

        return localizedString;
    }

    // =========================================================
    // 비동기 Localization 문자열 가져오기
    // =========================================================

    public IEnumerator GetLocalizedStringAsync(
        string key,
        System.Action<string> onComplete)
    {
        if (string.IsNullOrEmpty(key))
        {
            onComplete?.Invoke(
                string.Empty
            );

            yield break;
        }

        if (!isInitialized)
        {
            onComplete?.Invoke(
                key
            );

            yield break;
        }

        var operation =
            LocalizationSettings
                .StringDatabase
                .GetLocalizedStringAsync(
                    tableName,
                    key
                );

        yield return operation;

        if (operation.Status !=
            UnityEngine
                .ResourceManagement
                .AsyncOperations
                .AsyncOperationStatus
                .Succeeded)
        {
            onComplete?.Invoke(
                key
            );

            yield break;
        }

        string result =
            operation.Result;

        if (string.IsNullOrEmpty(result)
            ||
            result.Contains(
                "No translation found"))
        {
            onComplete?.Invoke(
                key
            );

            yield break;
        }

        onComplete?.Invoke(
            result
        );
    }

    // =========================================================
    // 언어 변경
    // =========================================================

    public void ChangeLanguage(
        TargetLanguage language)
    {
        if (!isInitialized)
        {
            Debug.LogWarning(
                "[Localization] 아직 초기화되지 않았습니다."
            );

            return;
        }

        string targetCode =
            GetLanguageCode(language);

        foreach (var locale in
                 LocalizationSettings
                     .AvailableLocales
                     .Locales)
        {
            if (locale == null)
                continue;

            string code =
                locale.Identifier.Code;

            if (code.Equals(
                    targetCode,
                    System.StringComparison.OrdinalIgnoreCase)
                ||
                code.StartsWith(targetCode))
            {
                // =================================================
                // 현재 언어 변경
                // =================================================

                targetLanguage =
                    language;

                LocalizationSettings.SelectedLocale =
                    locale;

                // =================================================
                // 언어 변경 즉시 Font 변경
                // =================================================

                ApplyFontToAllTexts(
                    targetLanguage
                );

                PlayerPrefs.SetString(
                    "ForcedTargetLang",
                    targetCode
                );

                PlayerPrefs.Save();

                Debug.Log(
                    $"[Localization] 언어 변경: {code}"
                );

                return;
            }
        }

        Debug.LogWarning(
            $"[Localization] 언어를 찾을 수 없습니다: {targetCode}"
        );
    }

    // =========================================================
    // 언어 변경 이벤트
    // =========================================================

    private void OnLanguageChanged(
        Locale newLocale)
    {
        if (!isInitialized)
            return;

        if (newLocale == null)
            return;

        Debug.Log(
            $"[Localization] 언어 변경 감지: " +
            $"{newLocale.Identifier.Code}"
        );

        // =====================================================
        // Locale에서 현재 TargetLanguage 갱신
        // =====================================================

        UpdateTargetLanguageFromLocale(
            newLocale
        );

        // =====================================================
        // 모든 TMP Font 변경
        // =====================================================

        ApplyFontToAllTexts(
            targetLanguage
        );

        // =====================================================
        // 모든 텍스트 갱신
        // =====================================================

        RefreshAllTexts();
    }

    // =========================================================
    // Locale → TargetLanguage
    // =========================================================

    private void UpdateTargetLanguageFromLocale(
        Locale locale)
    {
        if (locale == null)
            return;

        string code =
            locale.Identifier.Code;

        if (code.StartsWith("ko"))
        {
            targetLanguage =
                TargetLanguage.Korean;
        }
        else if (code.StartsWith("en"))
        {
            targetLanguage =
                TargetLanguage.English;
        }
        else if (code.StartsWith("ja"))
        {
            targetLanguage =
                TargetLanguage.Japanese;
        }
        else if (code.Equals(
                     "zh-CN",
                     System.StringComparison.OrdinalIgnoreCase))
        {
            targetLanguage =
                TargetLanguage.ChineseSimplified;
        }
        else if (code.Equals(
                     "zh-TW",
                     System.StringComparison.OrdinalIgnoreCase))
        {
            targetLanguage =
                TargetLanguage.ChineseTraditional;
        }
    }

    // =========================================================
    // 모든 텍스트 갱신
    // =========================================================

    private void RefreshAllTexts()
    {
        List<KeyValuePair<TextMeshProUGUI, string>> textList =
            new List<KeyValuePair<TextMeshProUGUI, string>>(
                localizedTexts
            );

        foreach (var pair in textList)
        {
            TextMeshProUGUI tmp =
                pair.Key;

            string key =
                pair.Value;

            if (tmp == null)
                continue;

            // =================================================
            // Localization 완전 제외
            // =================================================

            if (IsExcludedTMP(tmp))
                continue;

            if (string.IsNullOrEmpty(key))
                continue;

            // Font 적용
            ApplyCurrentFont(tmp);

            StartCoroutine(
                TranslateTextCoroutine(
                    tmp,
                    key
                )
            );
        }

        CleanupNullReferences();

        // =====================================================
        // CardView 갱신
        // =====================================================

        CardView[] cardViews =
            FindObjectsByType<CardView>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

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
        List<TextMeshProUGUI> removeList =
            new List<TextMeshProUGUI>();

        foreach (var pair in localizedTexts)
        {
            if (pair.Key == null)
            {
                removeList.Add(
                    pair.Key
                );
            }
        }

        foreach (var tmp in removeList)
        {
            localizedTexts.Remove(tmp);

            if (lastLocalizedValues.ContainsKey(tmp))
            {
                lastLocalizedValues.Remove(tmp);
            }
        }
    }

    // =========================================================
    // 씬 전환
    // =========================================================

    private void OnSceneLoaded(
        Scene scene,
        LoadSceneMode mode)
    {
        // 이전 씬의 TMP 제거
        localizedTexts.Clear();

        lastLocalizedValues.Clear();

        dynamicScanTimer = 0f;

        Debug.Log(
            $"[Localization] 씬 변경 → Dictionary 초기화: " +
            $"{scene.name}"
        );

        if (isInitialized)
        {
            StartCoroutine(
                ScanSceneAfterLoad()
            );
        }
    }

    // =========================================================
    // 씬 로딩 직후 검색
    // =========================================================

    private IEnumerator ScanSceneAfterLoad()
    {
        // 새 씬의 Awake / OnEnable / Start가
        // 어느 정도 끝난 다음 검색
        yield return null;

        // 새 씬 TMP에 현재 Font 적용
        ApplyFontToAllTexts(
            targetLanguage
        );

        ScanExistingTexts();
    }

    // =========================================================
    // Dictionary 반환
    // =========================================================

    public Dictionary<TextMeshProUGUI, string>
        GetTextDictionary
    {
        get
        {
            return localizedTexts;
        }
    }
}