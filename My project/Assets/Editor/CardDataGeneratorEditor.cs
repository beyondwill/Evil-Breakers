using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class CardDataGeneratorEditor : EditorWindow
{
    // ============================================================
    // 경로
    // ============================================================

    private const string CSV_FOLDER_PATH =
        "Assets/CSV";

    private const string CARD_FOLDER_PATH =
        "Assets/Scriptable Object/Player Card Info Scriptable Objects";

    // ============================================================
    // CSV
    // ============================================================

    private TextAsset csvFile;

    // ============================================================
    // 메뉴
    // ============================================================

    [MenuItem("Tools/Card Data Generator")]
    public static void ShowWindow()
    {
        CardDataGeneratorEditor window =
            GetWindow<CardDataGeneratorEditor>(
                "Card Data Generator"
            );

        window.minSize =
            new Vector2(600f, 400f);
    }

    // ============================================================
    // GUI
    // ============================================================

    private void OnGUI()
    {
        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField(
            "Card Data Generator",
            EditorStyles.boldLabel
        );

        EditorGUILayout.Space(10);

        DrawCSVSection();

        EditorGUILayout.Space(20);

        DrawCreateButton();

        EditorGUILayout.Space(20);

        EditorGUILayout.HelpBox(
            "CSV 구조:\n" +
            "1번째 줄 = 한글 설명\n" +
            "2번째 줄 = 실제 컬럼명\n" +
            "3번째 줄부터 = 카드 데이터\n\n" +
            "CSV에 존재하는 컬럼만 카드 Asset에 반영합니다.\n" +
            "CSV에 없는 값은 자동으로 추가하거나 변경하지 않습니다.\n\n" +
            "카드 Asset이 이미 존재하면 기존 Asset을 갱신합니다.\n" +
            "존재하지 않으면 새 Asset을 생성합니다.\n\n" +
            "UTF-8 / UTF-8 BOM / UTF-16 / Windows 한국어 CSV를 지원합니다.",
            MessageType.Info
        );
    }

    // ============================================================
    // CSV 선택
    // ============================================================

    private void DrawCSVSection()
    {
        EditorGUILayout.BeginVertical(
            EditorStyles.helpBox
        );

        EditorGUILayout.LabelField(
            "CSV",
            EditorStyles.boldLabel
        );

        EditorGUILayout.Space(5);

        csvFile =
            (TextAsset)EditorGUILayout.ObjectField(
                "Card CSV",
                csvFile,
                typeof(TextAsset),
                false
            );

        EditorGUILayout.EndVertical();
    }

    // ============================================================
    // 생성 버튼
    // ============================================================

    private void DrawCreateButton()
    {
        GUI.enabled =
            csvFile != null;

        if (GUILayout.Button(
            "Generate / Update Cards",
            GUILayout.Height(45f)
        ))
        {
            GenerateCards();
        }

        GUI.enabled = true;
    }

    // ============================================================
    // 카드 전체 생성 / 갱신
    // ============================================================

    private void GenerateCards()
    {
        if (csvFile == null)
        {
            Debug.LogError(
                "Card CSV가 지정되지 않았습니다."
            );

            return;
        }

        // --------------------------------------------------------
        // 폴더 생성
        // --------------------------------------------------------

        CreateFolder(
            CSV_FOLDER_PATH
        );

        CreateFolder(
            CARD_FOLDER_PATH
        );

        // --------------------------------------------------------
        // CSV 읽기
        // --------------------------------------------------------

        string csvText =
            ReadCSVText(
                csvFile
            );

        if (string.IsNullOrEmpty(csvText))
        {
            Debug.LogError(
                "CSV 내용을 읽을 수 없습니다."
            );

            return;
        }

        // --------------------------------------------------------
        // 줄 분리
        // --------------------------------------------------------

        string[] lines =
            csvText.Split(
                new[] { '\r', '\n' },
                StringSplitOptions.RemoveEmptyEntries
            );

        // --------------------------------------------------------
        // 최소 3줄
        //
        // 0 = 한글 설명
        // 1 = 실제 컬럼명
        // 2부터 = 카드 데이터
        // --------------------------------------------------------

        if (lines.Length <= 2)
        {
            Debug.LogError(
                "CSV에 카드 데이터가 없습니다."
            );

            return;
        }

        // --------------------------------------------------------
        // 실제 헤더
        // --------------------------------------------------------

        string[] headers =
            ParseCSVLine(
                lines[1]
            );

        for (int i = 0; i < headers.Length; i++)
        {
            headers[i] =
                NormalizeCSVValue(
                    headers[i]
                );
        }

        // --------------------------------------------------------
        // 헤더 확인
        // --------------------------------------------------------

        Debug.Log(
            "CSV 헤더 :\n" +
            string.Join(
                " | ",
                headers
            )
        );

        int createdCount = 0;
        int updatedCount = 0;
        int failedCount = 0;

        // ========================================================
        // 카드 처리
        // ========================================================

        for (int i = 2; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
                continue;

            string[] values =
                ParseCSVLine(
                    lines[i]
                );

            // ----------------------------------------------------
            // 컬럼 수 검사
            // ----------------------------------------------------

            if (values.Length != headers.Length)
            {
                Debug.LogError(
                    $"CSV {i + 1}번째 줄의 컬럼 수가 맞지 않습니다.\n" +
                    $"헤더 : {headers.Length}개\n" +
                    $"데이터 : {values.Length}개\n" +
                    $"라인 : {lines[i]}"
                );

                failedCount++;

                continue;
            }

            // ----------------------------------------------------
            // Dictionary
            // ----------------------------------------------------

            Dictionary<string, string> data =
                new Dictionary<string, string>(
                    StringComparer.OrdinalIgnoreCase
                );

            for (int j = 0; j < headers.Length; j++)
            {
                string header =
                    NormalizeCSVValue(
                        headers[j]
                    );

                string value =
                    NormalizeCSVValue(
                        values[j]
                    );

                if (string.IsNullOrWhiteSpace(header))
                    continue;

                data[header] =
                    value;
            }

            // ----------------------------------------------------
            // 카드 생성 / 갱신
            // ----------------------------------------------------

            CreateResult result =
                CreateOrUpdateCard(
                    data,
                    i + 1
                );

            switch (result)
            {
                case CreateResult.Created:
                    createdCount++;
                    break;

                case CreateResult.Updated:
                    updatedCount++;
                    break;

                case CreateResult.Failed:
                    failedCount++;
                    break;
            }
        }

        // ========================================================
        // 저장
        // ========================================================

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(
            "========================================\n" +
            $"카드 생성 완료\n" +
            $"새로 생성 : {createdCount}개\n" +
            $"업데이트 : {updatedCount}개\n" +
            $"실패 : {failedCount}개\n" +
            "========================================"
        );
    }

    // ============================================================
    // 결과
    // ============================================================

    private enum CreateResult
    {
        Created,
        Updated,
        Failed
    }

    // ============================================================
    // 카드 생성 / 갱신
    // ============================================================

    private CreateResult CreateOrUpdateCard(
        Dictionary<string, string> data,
        int lineNumber
    )
    {
        try
        {
            // ====================================================
            // 필수값
            //
            // 이 값들은 카드 생성에 반드시 필요하므로
            // CSV에 없으면 실패 처리
            // ====================================================

            string cardClassValue =
                Get(
                    data,
                    "CardClass"
                );

            if (string.IsNullOrWhiteSpace(cardClassValue))
            {
                Debug.LogError(
                    $"CSV {lineNumber}번째 줄 : CardClass가 없습니다."
                );

                return CreateResult.Failed;
            }

            CharacterClass characterClass =
                ParseEnum<CharacterClass>(
                    cardClassValue
                );

            if (characterClass == CharacterClass.None)
            {
                Debug.LogError(
                    $"CSV {lineNumber}번째 줄 : " +
                    $"CardClass 변환 실패\n" +
                    $"CSV 값 = [{cardClassValue}]"
                );

                return CreateResult.Failed;
            }

            // ====================================================
            // CardType
            // ====================================================

            string cardTypeValue =
                Get(
                    data,
                    "CardType"
                );

            if (string.IsNullOrWhiteSpace(cardTypeValue))
            {
                Debug.LogError(
                    $"CSV {lineNumber}번째 줄 : CardType이 없습니다."
                );

                return CreateResult.Failed;
            }

            CardType cardType =
                ParseEnum<CardType>(
                    cardTypeValue
                );

            if (cardType == CardType.None)
            {
                Debug.LogError(
                    $"CSV {lineNumber}번째 줄 : " +
                    $"CardType 변환 실패\n" +
                    $"CSV 값 = [{cardTypeValue}]"
                );

                return CreateResult.Failed;
            }

            // ====================================================
            // CardTarget
            //
            // CSV에 있는 경우에만 반영
            // ====================================================

            bool hasCardTarget =
                HasColumn(
                    data,
                    "CardTarget"
                );

            CardTarget cardTarget =
                default;

            if (hasCardTarget)
            {
                cardTarget =
                    ParseEnum<CardTarget>(
                        Get(
                            data,
                            "CardTarget"
                        )
                    );
            }

            // ====================================================
            // CardGrade
            // ====================================================

            bool hasCardGrade =
                HasColumn(
                    data,
                    "CardGrade"
                );

            CardRarity cardRarity =
                default;

            if (hasCardGrade)
            {
                cardRarity =
                    ParseCardRarity(
                        Get(
                            data,
                            "CardGrade"
                        )
                    );
            }

            // ====================================================
            // CardPeriod
            // ====================================================

            string cardPeriodValue =
                Get(
                    data,
                    "CardPeriod"
                );

            if (string.IsNullOrWhiteSpace(cardPeriodValue))
            {
                Debug.LogError(
                    $"CSV {lineNumber}번째 줄 : " +
                    "CardPeriod가 없습니다."
                );

                return CreateResult.Failed;
            }

            CardPeriod cardPeriod =
                ParseEnum<CardPeriod>(
                    cardPeriodValue
                );

            // ====================================================
            // CardName
            // ====================================================

            string cardName =
                Get(
                    data,
                    "CardName"
                );

            if (string.IsNullOrWhiteSpace(cardName))
            {
                Debug.LogError(
                    $"CSV {lineNumber}번째 줄 : " +
                    "CardName이 없습니다."
                );

                return CreateResult.Failed;
            }

            // ====================================================
            // Asset 경로
            // ====================================================

            string folderPath =
                $"{CARD_FOLDER_PATH}/{characterClass}";

            CreateFolder(
                folderPath
            );

            string safeName =
                MakeSafeFileName(
                    cardName
                );

            if (string.IsNullOrWhiteSpace(safeName))
            {
                Debug.LogError(
                    $"CSV {lineNumber}번째 줄 : " +
                    "유효한 카드 파일명이 없습니다."
                );

                return CreateResult.Failed;
            }

            string assetPath =
                $"{folderPath}/{safeName}.asset";

            // ====================================================
            // 기존 Asset 검색
            // ====================================================

            CardData cardData =
                AssetDatabase.LoadAssetAtPath<CardData>(
                    assetPath
                );

            bool isNew =
                cardData == null;

            // ====================================================
            // 기존 Asset이 없으면 생성
            // ====================================================

            if (isNew)
            {
                cardData =
                    CreateInstance<CardData>();

                AssetDatabase.CreateAsset(
                    cardData,
                    assetPath
                );
            }

            // ====================================================
            // 기본 정보
            //
            // CSV에 있는 컬럼만 변경
            // ====================================================

            cardData.characterClass =
                characterClass;

            cardData.cardType =
                cardType;

            if (hasCardTarget)
            {
                cardData.cardTarget =
                    cardTarget;
            }

            if (hasCardGrade)
            {
                cardData.cardRarity =
                    cardRarity;
            }

            cardData.cardPeriod =
                cardPeriod;

            // ====================================================
            // CardName
            // ====================================================

            cardData.card_name =
                cardName;

            // ====================================================
            // ActionCost
            // ====================================================

            if (HasColumn(
                data,
                "ActionCost"
            ))
            {
                cardData.card_cost =
                    ParseInt(
                        data,
                        "ActionCost"
                    );
            }

            // ====================================================
            // CardBuyCost
            // ====================================================

            if (HasColumn(
                data,
                "CardBuyCost"
            ))
            {
                cardData.buy_card_cost =
                    ParseInt(
                        data,
                        "CardBuyCost"
                    );
            }

            // ====================================================
            // CardSellCost
            // ====================================================

            if (HasColumn(
                data,
                "CardSellCost"
            ))
            {
                cardData.sell_card_cost =
                    ParseInt(
                        data,
                        "CardSellCost"
                    );
            }

            // ====================================================
            // 명중률
            //
            // CardAccuracy 컬럼이 있을 때만 변경
            // ====================================================

            if (HasColumn(
                data,
                "CardAccuracy"
            ))
            {
                string accuracyValue =
                    Get(
                        data,
                        "CardAccuracy"
                    );

                if (!string.IsNullOrWhiteSpace(
                    accuracyValue
                ))
                {
                    cardData.useAccuracy =
                        true;

                    cardData.accuracy =
                        ParseFloat(
                            data,
                            "CardAccuracy"
                        );
                }
                else
                {
                    cardData.useAccuracy =
                        false;

                    cardData.accuracy =
                        0f;
                }
            }

            // ====================================================
            // 카드 텍스트
            //
            // 중요:
            //
            // 여기서는 텍스트를 절대로 변환하지 않음.
            //
            // 예:
            // 단일 대상에게 {DMG10}의 피해를 줍니다.
            //
            // 위 문자열 그대로 저장.
            // ====================================================

            if (HasColumn(
                data,
                "CardText"
            ))
            {
                cardData.card_description =
                    Get(
                        data,
                        "CardText"
                    );
            }

            // ====================================================
            // 효과
            //
            // 중요:
            //
            // CSV에 Effect1_Effect가 없으면
            // 효과를 아예 건드리지 않는다.
            //
            // CSV에 효과 관련 컬럼을 추가했을 때만
            // 그때 효과를 처리한다.
            // ====================================================

            if (HasColumn(
                data,
                "Effect1_Effect"
            ))
            {
                if (cardData.effects == null)
                {
                    cardData.effects =
                        new List<CardEffectEntry>();
                }
                else
                {
                    cardData.effects.Clear();
                }

                CreateEffect(
                    cardData,
                    data,
                    "Effect1"
                );

                CreateEffect(
                    cardData,
                    data,
                    "Effect2"
                );

                CreateEffect(
                    cardData,
                    data,
                    "Effect3"
                );
            }

            // ====================================================
            // 변경 저장
            // ====================================================

            EditorUtility.SetDirty(
                cardData
            );

            // ====================================================
            // 로그
            // ====================================================

            if (isNew)
            {
                Debug.Log(
                    $"카드 생성 : " +
                    $"[{characterClass}] " +
                    $"{cardName}\n" +
                    $"{assetPath}"
                );

                return CreateResult.Created;
            }

            Debug.Log(
                $"카드 업데이트 : " +
                $"[{characterClass}] " +
                $"{cardName}\n" +
                $"{assetPath}"
            );

            return CreateResult.Updated;
        }
        catch (Exception e)
        {
            Debug.LogError(
                $"CSV {lineNumber}번째 줄 처리 실패\n" +
                $"{e}"
            );

            return CreateResult.Failed;
        }
    }

    // ============================================================
    // CardGrade -> CardRarity
    // ============================================================

    private CardRarity ParseCardRarity(
        string value
    )
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return CardRarity.Common;
        }

        value =
            NormalizeCSVValue(
                value
            );

        switch (
            value.ToLowerInvariant()
        )
        {
            case "basic":
                return CardRarity.Basic;

            case "normal":
                return CardRarity.Common;

            case "common":
                return CardRarity.Common;

            case "rare":
                return CardRarity.Rare;

            case "epic":
                return CardRarity.Epic;

            case "legendary":
                return CardRarity.Legendary;

            default:

                Debug.LogWarning(
                    $"알 수 없는 CardGrade : [{value}]\n" +
                    "Common으로 처리합니다."
                );

                return CardRarity.Common;
        }
    }

    // ============================================================
    // 효과 생성
    // ============================================================

    private void CreateEffect(
        CardData cardData,
        Dictionary<string, string> data,
        string prefix
    )
    {
        // --------------------------------------------------------
        // Effect 컬럼 자체가 없으면 아무것도 하지 않음
        // --------------------------------------------------------

        if (!HasColumn(
            data,
            $"{prefix}_Effect"
        ))
        {
            return;
        }

        string effectName =
            Get(
                data,
                $"{prefix}_Effect"
            );

        if (string.IsNullOrWhiteSpace(effectName))
            return;

        CardEffect effect =
            FindAsset<CardEffect>(
                effectName
            );

        CardVisual visual =
            null;

        if (HasColumn(
            data,
            $"{prefix}_Visual"
        ))
        {
            visual =
                FindAsset<CardVisual>(
                    Get(
                        data,
                        $"{prefix}_Visual"
                    )
                );
        }

        CardEffectEntry entry =
            new CardEffectEntry();

        // --------------------------------------------------------
        // Time
        // --------------------------------------------------------

        if (HasColumn(
            data,
            $"{prefix}_Time"
        ))
        {
            entry.time =
                ParseFloat(
                    data,
                    $"{prefix}_Time"
                );
        }

        // --------------------------------------------------------
        // Visual
        // --------------------------------------------------------

        entry.visual =
            visual;

        // --------------------------------------------------------
        // Effect
        // --------------------------------------------------------

        entry.effect =
            effect;

        // --------------------------------------------------------
        // Value
        // --------------------------------------------------------

        if (HasColumn(
            data,
            $"{prefix}_Value"
        ))
        {
            entry.value =
                ParseInt(
                    data,
                    $"{prefix}_Value"
                );
        }

        // --------------------------------------------------------
        // Value2
        // --------------------------------------------------------

        if (HasColumn(
            data,
            $"{prefix}_Value2"
        ))
        {
            entry.value2 =
                ParseInt(
                    data,
                    $"{prefix}_Value2"
                );
        }

        // --------------------------------------------------------
        // FloatValue
        // --------------------------------------------------------

        if (HasColumn(
            data,
            $"{prefix}_FloatValue"
        ))
        {
            entry.floatValue =
                ParseFloat(
                    data,
                    $"{prefix}_FloatValue"
                );
        }

        cardData.effects.Add(
            entry
        );
    }

    // ============================================================
    // Asset 검색
    // ============================================================

    private T FindAsset<T>(
        string assetName
    )
        where T : UnityEngine.Object
    {
        if (string.IsNullOrWhiteSpace(assetName))
            return null;

        string[] guids =
            AssetDatabase.FindAssets(
                $"{assetName} t:{typeof(T).Name}"
            );

        if (guids.Length == 0)
        {
            Debug.LogWarning(
                $"{typeof(T).Name}을 찾을 수 없습니다 : " +
                $"{assetName}"
            );

            return null;
        }

        string path =
            AssetDatabase.GUIDToAssetPath(
                guids[0]
            );

        return AssetDatabase.LoadAssetAtPath<T>(
            path
        );
    }

    // ============================================================
    // 컬럼 존재 여부
    // ============================================================

    private bool HasColumn(
        Dictionary<string, string> data,
        string key
    )
    {
        return data.ContainsKey(
            key
        );
    }

    // ============================================================
    // CSV 값 가져오기
    //
    // 중요:
    // 없는 컬럼이어도 경고하지 않는다.
    // ============================================================

    private string Get(
        Dictionary<string, string> data,
        string key
    )
    {
        if (!data.TryGetValue(
            key,
            out string value
        ))
        {
            return "";
        }

        return NormalizeCSVValue(
            value
        );
    }

    // ============================================================
    // CSV 값 정리
    // ============================================================

    private string NormalizeCSVValue(
        string value
    )
    {
        if (string.IsNullOrEmpty(value))
            return "";

        return value
            .Trim()
            .Trim('\uFEFF')
            .Trim('\u200B')
            .Trim('"')
            .Trim();
    }

    // ============================================================
    // Enum
    // ============================================================

    private T ParseEnum<T>(
        string value
    )
        where T : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return default;
        }

        value =
            NormalizeCSVValue(
                value
            );

        if (Enum.TryParse<T>(
            value,
            true,
            out T result
        ))
        {
            return result;
        }

        Debug.LogWarning(
            $"Enum 변환 실패 : " +
            $"Enum = {typeof(T).Name}, " +
            $"Value = [{value}]"
        );

        return default;
    }

    // ============================================================
    // Int
    // ============================================================

    private int ParseInt(
        Dictionary<string, string> data,
        string key
    )
    {
        string value =
            Get(
                data,
                key
            );

        if (int.TryParse(
            value,
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out int result
        ))
        {
            return result;
        }

        if (!string.IsNullOrWhiteSpace(value))
        {
            Debug.LogWarning(
                $"정수 변환 실패 : " +
                $"{key} = [{value}]"
            );
        }

        return 0;
    }

    // ============================================================
    // Float
    // ============================================================

    private float ParseFloat(
        Dictionary<string, string> data,
        string key
    )
    {
        string value =
            Get(
                data,
                key
            );

        if (float.TryParse(
            value,
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out float result
        ))
        {
            return result;
        }

        if (!string.IsNullOrWhiteSpace(value))
        {
            Debug.LogWarning(
                $"실수 변환 실패 : " +
                $"{key} = [{value}]"
            );
        }

        return 0f;
    }

    // ============================================================
    // CSV 파싱
    // ============================================================

    private string[] ParseCSVLine(
        string line
    )
    {
        List<string> values =
            new List<string>();

        bool insideQuotes =
            false;

        StringBuilder current =
            new StringBuilder();

        for (int i = 0; i < line.Length; i++)
        {
            char c =
                line[i];

            if (c == '"')
            {
                if (
                    insideQuotes &&
                    i + 1 < line.Length &&
                    line[i + 1] == '"'
                )
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    insideQuotes =
                        !insideQuotes;
                }
            }
            else if (
                c == ',' &&
                !insideQuotes
            )
            {
                values.Add(
                    current.ToString()
                );

                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        values.Add(
            current.ToString()
        );

        return values.ToArray();
    }

    // ============================================================
    // CSV 인코딩 자동 처리
    // ============================================================

    private string ReadCSVText(
        TextAsset textAsset
    )
    {
        if (textAsset == null)
            return "";

        // ========================================================
        // TextAsset.bytes 사용
        //
        // File.ReadAllBytes()를 사용하지 않음.
        // Excel이 CSV를 열고 있어도 Sharing Violation이 발생하지 않음.
        // ========================================================

        byte[] bytes =
            textAsset.bytes;

        if (bytes == null || bytes.Length == 0)
        {
            Debug.LogError(
                "CSV 데이터가 비어있습니다."
            );

            return "";
        }

        // ========================================================
        // UTF-8 BOM
        // ========================================================

        if (
            bytes.Length >= 3 &&
            bytes[0] == 0xEF &&
            bytes[1] == 0xBB &&
            bytes[2] == 0xBF
        )
        {
            Debug.Log(
                "CSV 인코딩 : UTF-8 BOM"
            );

            return Encoding.UTF8.GetString(
                bytes,
                3,
                bytes.Length - 3
            );
        }

        // ========================================================
        // UTF-16 LE
        // ========================================================

        if (
            bytes.Length >= 2 &&
            bytes[0] == 0xFF &&
            bytes[1] == 0xFE
        )
        {
            Debug.Log(
                "CSV 인코딩 : UTF-16 LE"
            );

            return Encoding.Unicode.GetString(
                bytes,
                2,
                bytes.Length - 2
            );
        }

        // ========================================================
        // UTF-16 BE
        // ========================================================

        if (
            bytes.Length >= 2 &&
            bytes[0] == 0xFE &&
            bytes[1] == 0xFF
        )
        {
            Debug.Log(
                "CSV 인코딩 : UTF-16 BE"
            );

            return Encoding.BigEndianUnicode.GetString(
                bytes,
                2,
                bytes.Length - 2
            );
        }

        // ========================================================
        // UTF-8
        // ========================================================

        if (IsValidUTF8(bytes))
        {
            Debug.Log(
                "CSV 인코딩 : UTF-8"
            );

            return Encoding.UTF8.GetString(
                bytes
            );
        }

        // ========================================================
        // Windows 한국어 인코딩
        // ========================================================

        string windowsKorean =
            TryDecodeWindowsKorean(
                bytes
            );

        if (!string.IsNullOrEmpty(
            windowsKorean
        ))
        {
            Debug.Log(
                "CSV 인코딩 : Windows 한국어 인코딩 (CP949)"
            );

            return windowsKorean;
        }

        // ========================================================
        // 최후 fallback
        // ========================================================

        Debug.LogWarning(
            "CSV 인코딩을 정확하게 판별하지 못했습니다.\n" +
            "UTF-8로 처리합니다."
        );

        return Encoding.UTF8.GetString(
            bytes
        );
    }

    // ============================================================
    // Windows 한국어 인코딩
    // ============================================================

    private string TryDecodeWindowsKorean(
        byte[] bytes
    )
    {
        // --------------------------------------------------------
        // CP949
        // --------------------------------------------------------

        try
        {
            Encoding encoding =
                Encoding.GetEncoding(
                    949
                );

            return encoding.GetString(
                bytes
            );
        }
        catch
        {
        }

        // --------------------------------------------------------
        // KS_C_5601-1987
        // --------------------------------------------------------

        try
        {
            Encoding encoding =
                Encoding.GetEncoding(
                    "ks_c_5601-1987"
                );

            return encoding.GetString(
                bytes
            );
        }
        catch
        {
        }

        // --------------------------------------------------------
        // EUC-KR
        // --------------------------------------------------------

        try
        {
            Encoding encoding =
                Encoding.GetEncoding(
                    "euc-kr"
                );

            return encoding.GetString(
                bytes
            );
        }
        catch
        {
        }

        return "";
    }

    // ============================================================
    // UTF-8 검사
    // ============================================================

    private bool IsValidUTF8(
        byte[] bytes
    )
    {
        try
        {
            UTF8Encoding utf8 =
                new UTF8Encoding(
                    false,
                    true
                );

            utf8.GetString(
                bytes
            );

            return true;
        }
        catch
        {
            return false;
        }
    }

    // ============================================================
    // 폴더 생성
    // ============================================================

    private void CreateFolder(
        string path
    )
    {
        if (AssetDatabase.IsValidFolder(path))
            return;

        string parent =
            Path.GetDirectoryName(
                path
            ).Replace(
                "\\",
                "/"
            );

        string folderName =
            Path.GetFileName(
                path
            );

        if (!AssetDatabase.IsValidFolder(parent))
        {
            CreateFolder(
                parent
            );
        }

        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder(
                parent,
                folderName
            );
        }
    }

    // ============================================================
    // 파일명 정리
    // ============================================================

    private string MakeSafeFileName(
        string value
    )
    {
        if (string.IsNullOrWhiteSpace(value))
            return "";

        foreach (
            char invalidChar
            in Path.GetInvalidFileNameChars()
        )
        {
            value =
                value.Replace(
                    invalidChar.ToString(),
                    ""
                );
        }

        return value.Trim();
    }
}