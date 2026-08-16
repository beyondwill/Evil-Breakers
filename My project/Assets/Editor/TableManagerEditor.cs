using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TableManager))]
public class TableManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI(); // 기존 변수들 표시

        TableManager manager = (TableManager)target;

        GUILayout.Space(20);

        // 1. 로드 버튼 (기존)
        if (GUILayout.Button("전체 데이터 자동 로드 (리스트 채우기)", GUILayout.Height(40)))
        {
            Undo.RecordObject(manager, "Load Tables From Folder");
            manager.LoadAllTablesFromFolder();
            EditorUtility.SetDirty(manager);
        }

        GUILayout.Space(5);

        // 2. 삭제 버튼 (신규)
        if (GUILayout.Button("전체 데이터 삭제 (리스트 비우기)", GUILayout.Height(30)))
        {
            // 실수 방지를 위한 팝업 창
            if (EditorUtility.DisplayDialog("데이터 삭제 경고",
                "모든 테이블의 리스트를 비우시겠습니까? (원본 SO 파일은 삭제되지 않습니다)", "예", "아니오"))
            {
                Undo.RecordObject(manager, "Clear All Tables");
                manager.ClearAllTables();
                EditorUtility.SetDirty(manager);
            }
        }

        GUI.backgroundColor = Color.white;
    }
}