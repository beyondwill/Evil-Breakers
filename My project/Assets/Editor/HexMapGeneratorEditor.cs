using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HexMapGenerator))]
public class HexMapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        HexMapGenerator gen =
            (HexMapGenerator)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Generate Map"))        {
            gen.ClearMap();
            EditorApplication.delayCall += () =>
            {
                gen.Generate();
            };
        }

        if (
            GUILayout.Button(
                "Toggle Random Offset"
            )
        )
        {
            gen.ToggleRandomOffset();
        }

        if (
            GUILayout.Button(
                "Clear Map"
            )
)
        {
            gen.ClearMap();
        }
    }
}