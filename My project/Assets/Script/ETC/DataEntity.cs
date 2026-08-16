using UnityEngine;

public abstract class DataEntity : ScriptableObject
{
    public string data_code;

    // 인스펙터에서 값이 수정될 때 실행됨
#if UNITY_EDITOR
    protected virtual void OnValidate()
    {
        // 코드가 비어있다면 파일 이름을 자동으로 채워줌
        if (string.IsNullOrEmpty(data_code))
        {
            data_code = name;
        }
    }
#endif
}