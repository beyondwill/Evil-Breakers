using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloseUI : MonoBehaviour
{
    // 닫기 버튼 누르면
    public void CloseButtonClick()
    {
        gameObject.SetActive(false);
    }
}
