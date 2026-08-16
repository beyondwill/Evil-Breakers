using System.Collections.Generic;
using UnityEngine;
public class HexNode
{
    // 노드 타입
    public enum NodeType
    {
        Empty,
        Start,
        Normal,
        Elite,
        Shop,
        Event,
        Trap,
        Boss
    }

    // 구역 타입
    public enum ZoneType
    {
        Street,
        Subway,
        Beach,
        Hospital,
        Skyscraper
    }

    public Vector2Int coord;        // 좌표 
    
    public Vector2 uiPos;           // UI 위치

    public Vector2 originalPos;     // 원본 UI 위치

    public int distance;            // 원점으로부터의 거리

    public NodeType type;           // 노드 타입

    public ZoneType zone;           // 구역 타입

    public bool isVisited = false;  // 방문했는가?

    public bool isRevealed = false; // 드러났는가?

    public List<HexNode> links = new();
}