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
    // TMP → Localization Key
    // =========================================================

    private Dictionary<TextMeshProUGUI, string> localizedTexts =
        new Dictionary<TextMeshProUGUI, string>();


    // =========================================================
    // TMP → 마지막으로 Localization 시스템이 적용한 문자열
    //
    // 이걸 이용해서
    //
    // tmp.text = "CARD_FIRE_NAME"
    //
    // 처럼 외부 코드가 TMP를 변경했는지 감지한다.
    // =========================================================

    private Dictionary<TextMeshProUGUI, string> lastLocalizedValues =
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
        SceneManager.sceneLoaded +=
            OnSceneLoaded;


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
    // 1. 이미 등록된 TMP
    //    → 매 프레임 text 변경 여부 확인
    //
    // 2. 새 TMP
    //    → 0.1초마다 전체 검색
    //
    // =========================================================

    private void Update()
    {
        if (!isInitialized)
            return;


        // =====================================================
        // 이미 등록된 TMP는 매 프레임 확인
        // =====================================================

        CheckRegisteredTexts();


        // =====================================================
        // 새 TMP 검색은 0.1초마다
        // =====================================================

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


        // 초기 씬에 이미 존재하는 TMP 검색
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
    //
    // 새로 Instantiate 된 TMP를 찾는다.
    //
    // 이 함수 자체는 0.1초마다 실행된다.
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
    // 등록된 TMP의 text 변경 감지
    // =========================================================
    //
    // 중요!
    //
    // 이 함수는 Update()에서 매 프레임 실행된다.
    //
    // 예:
    //
    // 기존:
    // CARD_FIRE_NAME
    //
    // 번역 결과:
    // Fire
    //
    // 이후 다른 코드에서:
    // CARD_ICE_NAME
    //
    // 으로 변경하면 즉시 감지한다.
    //
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


            if (string.IsNullOrEmpty(key))
                continue;


            // =================================================
            // 우리가 마지막으로 적용했던 번역 문자열
            // =================================================

            string lastValue = "";


            lastLocalizedValues.TryGetValue(
                tmp,
                out lastValue
            );


            // =================================================
            // 현재 TMP.text가
            // 우리가 마지막으로 넣었던 값과 같음
            //
            // → 정상 상태
            // =================================================

            if (tmp.text == lastValue)
                continue;


            // =================================================
            // 아직 번역되지 않은 상태
            //
            // 예:
            //
            // tmp.text = "CARD_FIRE_NAME"
            // key      = "CARD_FIRE_NAME"
            // =================================================

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


            // =================================================
            // 여기까지 왔다면
            //
            // 다른 코드가 TMP.text를 변경한 것
            // =================================================

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


            // =================================================
            // 새로운 Key로 변경
            // =================================================

            localizedTexts[tmp] =
                newKey;


            // =================================================
            // 새 Key 번역
            // =================================================

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
        else
        {
            localizedTexts[tmp] =
                key;
        }


        // =====================================================
        // 초기화 전이면
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


        // TMP가 번역 도중 파괴되었을 경우
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
            // =================================================
            // Dictionary Key이 현재 번역 요청 Key와
            // 동일한지 확인
            //
            // 번역 요청 중 다른 Key로 바뀌었을 경우
            // 오래된 번역 결과가 덮어쓰는 것을 방지
            // =================================================

            if (localizedTexts.TryGetValue(
                    tmp,
                    out string currentKey
                ))
            {
                if (currentKey != key)
                    yield break;
            }


            // =================================================
            // 실제 적용
            // =================================================

            tmp.text =
                localizedString;


            // =================================================
            // 우리가 적용한 값 기록
            // =================================================

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


            if (string.IsNullOrEmpty(key))
                continue;


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