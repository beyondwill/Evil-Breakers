using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DataManager))]
public class DataManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 기존 Inspector
        base.OnInspectorGUI();


        DataManager dataManager =
            (DataManager)target;


        GUILayout.Space(15);


        // =====================================================
        // Save / Load
        // =====================================================

        GUILayout.BeginHorizontal();


        // -----------------------------------------------------
        // 데이터 저장
        // -----------------------------------------------------

        GUI.backgroundColor =
            Color.cyan;


        if (GUILayout.Button(
            "데이터 저장 (Save)",
            GUILayout.Height(35)))
        {
            dataManager.SaveData();
        }


        // -----------------------------------------------------
        // 데이터 불러오기
        // -----------------------------------------------------

        GUI.backgroundColor =
            Color.green;


        if (GUILayout.Button(
            "데이터 불러오기 (Load)",
            GUILayout.Height(35)))
        {
            dataManager.LoadData();
        }


        GUILayout.EndHorizontal();


        // =====================================================
        // 기본 JSON 생성
        // =====================================================

        GUILayout.Space(5);


        GUI.backgroundColor =
            Color.yellow;


        if (GUILayout.Button(
            "기본 Json 생성",
            GUILayout.Height(35)))
        {
            bool confirm =
                EditorUtility.DisplayDialog(
                    "기본 Json 생성",
                    "현재 데이터를 default.json으로 생성하시겠습니까?",
                    "생성",
                    "취소"
                );


            if (confirm)
            {
                dataManager.CreateDefaultJson();
            }
        }


        // =====================================================
        // GUI 색상 초기화
        // =====================================================

        GUI.backgroundColor =
            Color.white;
    }
}