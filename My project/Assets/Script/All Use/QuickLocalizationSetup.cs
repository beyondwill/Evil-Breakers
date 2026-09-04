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
    private TargetLanguage targetLanguage =
        TargetLanguage.English;


    private bool isInitialized = false;


    // =========================================================
    // TMP → 원본 번역 Key
    // =========================================================

    private Dictionary<TextMeshProUGUI, string> localizedTexts =
        new Dictionary<TextMeshProUGUI, string>();


    // =========================================================
    // 런타임 새 TMP 검색 설정
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
            SceneManager.sceneLoaded -=
                OnSceneLoaded;


            LocalizationSettings.SelectedLocaleChanged -=
                OnLanguageChanged;


            Instance = null;
        }
    }


    // =========================================================
    // Update
    // =========================================================
    //
    // 런타임에서 Instantiate 된 TMP를 자동으로 발견한다.
    //
    // 기존에 등록된 TMP는 Dictionary에 있으므로
    // 다시 번역하지 않는다.
    //
    // =========================================================

    private void Update()
    {
        if (!isInitialized)
            return;


        dynamicScanTimer +=
            Time.unscaledDeltaTime;


        if (dynamicScanTimer >= dynamicScanInterval)
        {
            dynamicScanTimer = 0f;


            ScanNewTexts();
        }
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
            GetLanguageCode(
                targetLanguage
            );


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


        // =====================================================
        // 초기 씬에 이미 존재하는 TMP 검색
        // 비활성화된 오브젝트도 포함
        // =====================================================

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
    // 기존 TMP 검색
    // =========================================================
    //
    // FindObjectsInactive.Include를 사용해서
    // SetActive(false) 상태의 UI도 검색한다.
    //
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


            string currentText =
                tmp.text;


            if (string.IsNullOrEmpty(currentText))
                continue;


            if (currentText.Contains(
                    "No translation found"))
                continue;


            // =================================================
            // 이미 등록된 TMP
            // =================================================

            if (localizedTexts.ContainsKey(tmp))
            {
                // 이미 등록되어 있어도
                // 현재 언어로 다시 번역한다.

                TryTranslateText(
                    tmp,
                    localizedTexts[tmp]
                );


                continue;
            }


            // =================================================
            // 처음 발견된 TMP
            // =================================================
            //
            // 현재 TMP의 text를 Localization Key로 사용
            //

            RegisterText(
                tmp,
                currentText
            );
        }


        CleanupNullReferences();
    }


    // =========================================================
    // 새로 생성된 TMP 검색
    // =========================================================
    //
    // Update()에서 일정 시간마다 호출된다.
    //
    // 이미 localizedTexts에 들어있는 TMP는 무시한다.
    //
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
            // 이미 등록된 TMP
            // =================================================

            if (localizedTexts.ContainsKey(tmp))
                continue;


            string currentText =
                tmp.text;


            // =================================================
            // 아직 텍스트가 없는 TMP
            // =================================================

            if (string.IsNullOrEmpty(currentText))
                continue;


            // =================================================
            // Localization 오류 문자열
            // =================================================

            if (currentText.Contains(
                    "No translation found"))
                continue;


            // =================================================
            // 새 TMP 발견
            // =================================================

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
    // 동적 TMP 등록
    // =========================================================
    //
    // 런타임에서 TMP를 생성한 경우에도 사용 가능:
    //
    // QuickLocalizationSetup.Instance.RegisterText(
    //     tmp,
    //     "CARD_RAGE_NAME"
    // );
    //
    // =========================================================

    public void RegisterText(
        TextMeshProUGUI tmp,
        string key)
    {
        if (tmp == null)
            return;


        if (string.IsNullOrEmpty(key))
            return;


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


        // =====================================================
        // Localization 초기화 전이면
        // Dictionary에만 등록
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


        if (string.IsNullOrEmpty(key))
            return;


        if (!isInitialized)
            return;


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


        if (string.IsNullOrEmpty(key))
            yield break;


        if (!isInitialized)
            yield break;


        var operation =
            LocalizationSettings
                .StringDatabase
                .GetLocalizedStringAsync(
                    tableName,
                    key
                );


        yield return operation;


        // =====================================================
        // TMP가 번역 도중 파괴되었을 경우
        // =====================================================

        if (tmp == null)
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
            tmp.text =
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
            GetLanguageCode(
                language
            );


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


                targetLanguage =
                    language;


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


        RefreshAllTexts();
    }


    // =========================================================
    // 모든 텍스트 갱신
    // =========================================================

    private void RefreshAllTexts()
    {
        // =====================================================
        // Dictionary 복사
        // =====================================================

        List<KeyValuePair<TextMeshProUGUI, string>>
            textList =
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


            if (string.IsNullOrEmpty(key))
                continue;


            // =================================================
            // 비동기 번역
            // =================================================

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


        Debug.Log(
            $"[Localization] 씬 변경 → Dictionary 초기화: " +
            $"{scene.name}"
        );


        // =====================================================
        // 새 씬의 TMP 검색
        // =====================================================

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