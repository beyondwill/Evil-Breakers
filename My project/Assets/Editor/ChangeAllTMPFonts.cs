using UnityEngine;
using UnityEditor;
using TMPro;
using System.IO;

public class ChangeAllTMPFonts : EditorWindow
{
    private TMP_FontAsset newFont;

    [MenuItem("Tools/Change All TMP Fonts")]
    public static void ShowWindow()
    {
        GetWindow<ChangeAllTMPFonts>("TMP Font Changer");
    }

    private void OnGUI()
    {
        GUILayout.Label("프로젝트 전체 TMP 글꼴 변경", EditorStyles.boldLabel);

        newFont = (TMP_FontAsset)EditorGUILayout.ObjectField(
            "새 글꼴",
            newFont,
            typeof(TMP_FontAsset),
            false
        );

        GUILayout.Space(10);

        if (newFont == null)
        {
            EditorGUILayout.HelpBox(
                "변경할 TMP Font Asset을 넣어주세요.",
                MessageType.Warning
            );
            return;
        }

        if (GUILayout.Button("모든 Prefab 글꼴 변경"))
        {
            ChangeAllPrefabs();
        }

        if (GUILayout.Button("모든 Scene 글꼴 변경"))
        {
            ChangeAllScenes();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Prefab + Scene 전부 변경"))
        {
            ChangeAllPrefabs();
            ChangeAllScenes();

            AssetDatabase.SaveAssets();

            Debug.Log("모든 TMP 글꼴 변경 완료!");
        }
    }

    private void ChangeAllPrefabs()
    {
        string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab");

        int changedCount = 0;

        foreach (string guid in prefabGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            GameObject prefab =
                PrefabUtility.LoadPrefabContents(path);

            TMP_Text[] texts =
                prefab.GetComponentsInChildren<TMP_Text>(true);

            bool changed = false;

            foreach (TMP_Text text in texts)
            {
                if (text.font != newFont)
                {
                    text.font = newFont;

                    EditorUtility.SetDirty(text);

                    changed = true;
                    changedCount++;
                }
            }

            if (changed)
            {
                PrefabUtility.SaveAsPrefabAsset(prefab, path);
            }

            PrefabUtility.UnloadPrefabContents(prefab);
        }

        AssetDatabase.SaveAssets();

        Debug.Log($"Prefab 글꼴 변경 완료: {changedCount}개");
    }

    private void ChangeAllScenes()
    {
        string[] sceneGUIDs = AssetDatabase.FindAssets("t:Scene");

        int changedCount = 0;

        foreach (string guid in sceneGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(
                path,
                UnityEditor.SceneManagement.OpenSceneMode.Single
            );

            TMP_Text[] texts =
                Object.FindObjectsByType<TMP_Text>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                );

            bool changed = false;

            foreach (TMP_Text text in texts)
            {
                if (text.font != newFont)
                {
                    text.font = newFont;

                    EditorUtility.SetDirty(text);

                    changed = true;
                    changedCount++;
                }
            }

            if (changed)
            {
                UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
                    UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
                );
            }
        }

        Debug.Log($"Scene 글꼴 변경 완료: {changedCount}개");
    }
}