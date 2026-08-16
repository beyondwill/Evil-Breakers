using UnityEngine;

[CreateAssetMenu(fileName = "Location Info", menuName = "Main/Location Info")]
public class LocationInfo : DataEntity
{
    public string location_name;        // 지역명

    public string location_info;        // 지역 설명
    public Sprite location_image;       // 지역 이미지

    public int location_rank;           // 위험도/등급

    public int reward_money;            // 보상 돈
    public HexMapDataSO HMDS;           // 헥스 맵 데이타 SO
}