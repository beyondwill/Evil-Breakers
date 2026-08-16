using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakingText : MonoBehaviour
{
    public ShowText showText;
    public Color c;

    // 텍스트 생성
    public void TextInit(string s)
    {
        Instantiate(showText, transform).ShowTextInit(s, c);
    }

    public void TextInit(int s)
    {
        TextInit(s.ToString());
    }
}
