using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DataManager))]
public class DataManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 1. 기존 변수들(all_data 등)을 인스펙터에 기본으로 띄워줍니다.
        base.OnInspectorGUI();

        DataManager dataManager = (DataManager)target;

        GUILayout.Space(15); // 약간의 여백

        // 가로로 버튼 배치 시작
        GUILayout.BeginHorizontal();

        // 2. 데이터 저장 버튼 (하늘색)
        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("데이터 저장 (Save)", GUILayout.Height(35)))
        {
            dataManager.SaveData();
        }

        // 3. 데이터 불러오기 버튼 (연두색)
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("데이터 불러오기 (Load)", GUILayout.Height(35)))
        {
            dataManager.LoadData();
        }

        GUILayout.EndHorizontal();

        // GUI 색상 초기화
        GUI.backgroundColor = Color.white;
    }
}