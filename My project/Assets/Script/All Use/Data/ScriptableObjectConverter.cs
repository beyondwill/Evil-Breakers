using Newtonsoft.Json;
using System;
using UnityEngine;

public class ScriptableObjectConverter : JsonConverter
{
    // DataEntity를 상속받은 객체들만 이 컨버터를 거치도록 설정
    public override bool CanConvert(Type objectType)
    {
        return typeof(DataEntity).IsAssignableFrom(objectType);
    }

    // 저장할 때: SO 객체를 -> 내부의 data_code 문자열로 변환하여 기록
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var dataEntity = value as DataEntity;

        // null이 아니라면 객체 자체가 아닌 data_code 문자열을 저장합니다.
        writer.WriteValue(dataEntity != null ? dataEntity.data_code : null);
    }

    // 불러올 때: data_code 문자열을 -> TableManager에서 진짜 SO 객체로 복구
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null) return null;

        string codeKey = (string)reader.Value;
        if (string.IsNullOrEmpty(codeKey)) return null;

        // 읽어온 data_code를 가지고 TableManager에 가서 진짜 SO를 찾아와 연결!
        return TableManager.Instance.GetAny(objectType, codeKey);
    }
}