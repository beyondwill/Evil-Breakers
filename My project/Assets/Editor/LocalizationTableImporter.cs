using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization.Tables;

public class LocalizationTableImporter : EditorWindow
{
    [Header("CSV")]
    [SerializeField]
    private string csvFolderPath = "Assets/Resources/CSV"; // 스크린샷 경로 반영

    [Header("Localization")]
    [SerializeField]
    private string tableCollectionName = "Goza"; // 스크린샷 테이블 이름 반영

    [MenuItem("Tools/Import CSV to Localization Table")]
    public static void ShowWindow()
    {
        GetWindow<LocalizationTableImporter>("CSV to Localization Table");
    }

    private void OnGUI()
    {
        GUILayout.Label("CSV → Localization Table Importer", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);

        csvFolderPath = EditorGUILayout.TextField("CSV Folder Path", csvFolderPath);
        tableCollectionName = EditorGUILayout.TextField("Table Collection", tableCollectionName);

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Import to Tables", GUILayout.Height(40)))
        {
            ImportToTables();
        }
    }

    private void ImportToTables()
    {
        // 1. Localization Table 찾기
        string[] guids = AssetDatabase.FindAssets("t:StringTable");
        var targetGuids = new System.Collections.Generic.List<string>();

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            StringTable table = AssetDatabase.LoadAssetAtPath<StringTable>(path);
            if (table != null)
            {
                if (table.TableCollectionName.Contains(tableCollectionName) || Path.GetFileNameWithoutExtension(path).Contains(tableCollectionName))
                {
                    targetGuids.Add(guid);
                    Debug.Log($"[Localization Import] 매칭된 테이블 발견: {path} (Locale: {table.LocaleIdentifier.Code})");
                }
            }
        }

        if (targetGuids.Count == 0)
        {
            EditorUtility.DisplayDialog("오류", $"'{tableCollectionName}'과 일치하는 String Table을 찾을 수 없습니다.", "확인");
            return;
        }

        // 2. CSV/텍스트 폴더 확인 및 파일 로드
        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        string fullFolderPath = Path.Combine(projectRoot, csvFolderPath);

        if (!Directory.Exists(fullFolderPath))
        {
            EditorUtility.DisplayDialog("오류", $"CSV 폴더를 찾을 수 없습니다.\n\n{fullFolderPath}", "확인");
            return;
        }

        string[] csvFiles = Directory.GetFiles(fullFolderPath, "*.csv", SearchOption.TopDirectoryOnly);
        if (csvFiles.Length == 0)
        {
            // 확장자가 .txt이거나 다를 수도 있으므로 전체 파일 검색 시도
            csvFiles = Directory.GetFiles(fullFolderPath, "*.*", SearchOption.TopDirectoryOnly);
        }

        if (csvFiles.Length == 0)
        {
            EditorUtility.DisplayDialog("알림", "해당 폴더에 파일이 없습니다.", "확인");
            return;
        }

        int totalImportedCount = 0;
        int totalSkippedCount = 0;

        foreach (string filePath in csvFiles)
        {
            // .meta 파일 제외
            if (filePath.EndsWith(".meta")) continue;

            string csvText = ReadCSV(filePath);
            if (string.IsNullOrEmpty(csvText)) continue;

            using (StringReader reader = new StringReader(csvText))
            {
                int lineIndex = 0;

                while (true)
                {
                    string line = reader.ReadLine();
                    if (line == null) break;
                    lineIndex++;

                    // 첫 3줄(Index/KOR 타입 정보 등) 메타데이터 행 스킵
                    if (lineIndex <= 3) continue;
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    string[] values = ParseCSVLine(line);
                    if (values == null || values.Length < 1)
                    {
                        totalSkippedCount++;
                        continue;
                    }

                    string key = GetValue(values, 0); // Index (예: 50001)
                    if (string.IsNullOrEmpty(key))
                    {
                        totalSkippedCount++;
                        continue;
                    }

                    string kor = GetValue(values, 1);
                    string eng = GetValue(values, 2);
                    string jpn = GetValue(values, 3);
                    string cn = GetValue(values, 4);
                    string tw = GetValue(values, 5);

                    // 3. 각 언어별 String Table에 값 주입
                    foreach (string guid in targetGuids)
                    {
                        string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                        StringTable table = AssetDatabase.LoadAssetAtPath<StringTable>(assetPath);
                        if (table == null) continue;

                        string localeCode = table.LocaleIdentifier.Code;

                        if (string.Equals(localeCode, "ko", StringComparison.OrdinalIgnoreCase) || localeCode.StartsWith("ko"))
                        {
                            UpdateTableEntry(table, key, kor);
                        }
                        else if (string.Equals(localeCode, "en", StringComparison.OrdinalIgnoreCase) || localeCode.StartsWith("en"))
                        {
                            UpdateTableEntry(table, key, eng);
                        }
                        else if (string.Equals(localeCode, "ja", StringComparison.OrdinalIgnoreCase) || localeCode.StartsWith("ja"))
                        {
                            UpdateTableEntry(table, key, jpn);
                        }
                        else if (string.Equals(localeCode, "zh-Hans", StringComparison.OrdinalIgnoreCase) || localeCode == "zh-CN")
                        {
                            UpdateTableEntry(table, key, cn);
                        }
                        else if (string.Equals(localeCode, "zh-Hant", StringComparison.OrdinalIgnoreCase) || localeCode == "zh-TW")
                        {
                            UpdateTableEntry(table, key, tw);
                        }
                    }

                    totalImportedCount++;
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("완료", $"Localization Table 업데이트 완료!\n\n처리된 데이터: {totalImportedCount}\n건너뛴 데이터: {totalSkippedCount}", "확인");
    }

    /// <summary>
    /// UTF-8, UTF-16 LE(Unicode Text), UTF-8 BOM 인코딩을 모두 자동 파싱
    /// </summary>
    private string ReadCSV(string filePath)
    {
        byte[] bytes = File.ReadAllBytes(filePath);
        if (bytes == null || bytes.Length == 0) return string.Empty;

        // 1. UTF-16 LE (FF FE) / BE (FE FF) BOM 체크 (엑셀 Unicode Text 저장 방식 대응)
        if (bytes.Length >= 2 && ((bytes[0] == 0xFF && bytes[1] == 0xFE) || (bytes[0] == 0xFE && bytes[1] == 0xFF)))
        {
            return Encoding.Unicode.GetString(bytes);
        }

        // 2. UTF-8 BOM 체크
        if (HasUTF8BOM(bytes))
        {
            return Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
        }

        // 3. 일반 UTF-8 읽기 시도 후 탭/콤마 미검출 시 Unicode 재시도
        try
        {
            string text = Encoding.UTF8.GetString(bytes);
            if (!text.Contains("\t") && !text.Contains(",") && bytes.Length > 4)
            {
                return Encoding.Unicode.GetString(bytes);
            }
            return text;
        }
        catch
        {
            return Encoding.Unicode.GetString(bytes);
        }
    }

    private bool HasUTF8BOM(byte[] bytes)
    {
        return bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF;
    }

    private string GetValue(string[] values, int index)
    {
        if (values == null || index < 0 || index >= values.Length) return string.Empty;
        // 공백 및 윈도우 줄바꿈 제어문자(\r) 제거
        return values[index].Trim().TrimEnd('\r');
    }

    private string[] ParseCSVLine(string line)
    {
        if (string.IsNullOrEmpty(line)) return Array.Empty<string>();

        // 탭(\t)으로 구분된 데이터 포맷 대응 (Unicode Text)
        if (line.Contains("\t"))
        {
            return line.Split('\t');
        }

        // 콤마(,) 및 따옴표 파싱 로직
        var result = new System.Collections.Generic.List<string>();
        StringBuilder current = new StringBuilder();
        bool insideQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"')
            {
                if (insideQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    insideQuotes = !insideQuotes;
                }
                continue;
            }

            if (c == ',' && !insideQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
                continue;
            }

            current.Append(c);
        }

        result.Add(current.ToString());
        return result.ToArray();
    }

    private void UpdateTableEntry(StringTable table, string key, string value)
    {
        if (table == null) return;

        var entry = table.GetEntry(key);
        if (entry != null)
        {
            entry.Value = value;
        }
        else
        {
            table.AddEntry(key, value);
        }

        EditorUtility.SetDirty(table);
        if (table.SharedData != null)
        {
            EditorUtility.SetDirty(table.SharedData);
        }
    }
}