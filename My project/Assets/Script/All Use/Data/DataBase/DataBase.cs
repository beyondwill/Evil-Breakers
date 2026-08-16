using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class DataBase<T> where T : ScriptableObject
{
    [SerializeField] private List<T> dataList = new List<T>();
    protected Dictionary<string, T> _table = new Dictionary<string, T>();

    public void FillDataFromFolder(string folderPath)
    {
#if UNITY_EDITOR
        // 폴더 경로 내의 해당 타입 에셋들을 검색
        // t:Type 형식을 사용하여 정확한 SO만 골라냄
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { folderPath });

        dataList.Clear();
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) dataList.Add(asset);
        }
#endif
    }

    public void InitTable()
    {
        _table.Clear();
        foreach (var data in dataList)
        {
            if (data == null) continue;
            // 파일 이름을 키로 사용 (dataCode를 쓰고 있다면 data.dataCode로 수정)
            _table[data.name] = data;
        }
    }
    public void ClearList()
    {
        dataList.Clear();
        _table.Clear();
    }

    public T Get(string key)
    {
        if (_table.TryGetValue(key, out T value)) return value;
        return null;
    }
}