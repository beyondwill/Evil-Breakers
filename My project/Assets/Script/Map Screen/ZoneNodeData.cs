using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
// 구간
public class Area
{
    public Sprite area_background_spirte;               // 구간 배경화면 스프라이트
    public string area_name;                            // 구간 이름
    public List<EnemyCharacterInfo> enemyList;          // 구간 등장 적 리스트
}

[CreateAssetMenu(fileName = "ZoneNodeData", menuName = "ScriptableObjects/ZoneNodeData")]
public class ZoneNodeData : ScriptableObject
{
    public HexNode.ZoneType zone_type;
    public string zone_name;
    public Sprite zone_icon_sprite;
    public List<AudioClip> zoneBGMList;
    public List<Area> areaList;

//    public List<EnemyCharacterInfo> all_enemyList;

//    // 에디터 초기화
//#if UNITY_EDITOR
//    private void OnValidate()
//    {
//        SyncEnemyList();
//    }
//#endif

//    public void SyncEnemyList()
//    {
//        if (areaList == null)
//        {
//            all_enemyList = new List<EnemyCharacterInfo>();
//            return;
//        }

//        HashSet<EnemyCharacterInfo> set = new HashSet<EnemyCharacterInfo>();

//        foreach (var area in areaList)
//        {
//            if (area?.enemyList == null) continue;

//            foreach (var enemy in area.enemyList)
//            {
//                if (enemy != null)
//                    set.Add(enemy);
//            }
//        }

//        all_enemyList = new List<EnemyCharacterInfo>(set);
//    }
}