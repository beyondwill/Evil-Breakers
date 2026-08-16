using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemShowBox : MonoBehaviour
{
    public static ItemShowBox Instance;

    void Awake()
    {
        Instance = this;
    }

    // 외부 요소
    [SerializeField] private GameObject item_option_prefab;

    
}