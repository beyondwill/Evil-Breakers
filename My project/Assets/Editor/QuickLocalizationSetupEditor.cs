using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(QuickLocalizationSetup))]
public class QuickLocalizationSetupEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 기존 Inspector 표시
        DrawDefaultInspector();


        GUILayout.Space(15);


        // =====================================================
        // 실시간 언어 변경
        // =====================================================

        EditorGUILayout.LabelField(
            "실시간 언어 변경",
            EditorStyles.boldLabel);


        GUILayout.Space(5);


        QuickLocalizationSetup setup =
            (QuickLocalizationSetup)target;


        // =====================================================
        // 한국어
        // =====================================================

        if (GUILayout.Button(
                "한국어",
                GUILayout.Height(30)))
        {
            setup.ChangeLanguage(
                QuickLocalizationSetup.TargetLanguage.Korean);
        }


        // =====================================================
        // 영어
        // =====================================================

        if (GUILayout.Button(
                "English",
                GUILayout.Height(30)))
        {
            setup.ChangeLanguage(
                QuickLocalizationSetup.TargetLanguage.English);
        }


        // =====================================================
        // 일본어
        // =====================================================

        if (GUILayout.Button(
                "日本語",
                GUILayout.Height(30)))
        {
            setup.ChangeLanguage(
                QuickLocalizationSetup.TargetLanguage.Japanese);
        }


        // =====================================================
        // 중국어 간체
        // =====================================================

        if (GUILayout.Button(
                "简体中文",
                GUILayout.Height(30)))
        {
            setup.ChangeLanguage(
                QuickLocalizationSetup.TargetLanguage.ChineseSimplified);
        }


        // =====================================================
        // 중국어 번체
        // =====================================================

        if (GUILayout.Button(
                "繁體中文",
                GUILayout.Height(30)))
        {
            setup.ChangeLanguage(
                QuickLocalizationSetup.TargetLanguage.ChineseTraditional);
        }
    }
}