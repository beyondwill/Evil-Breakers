using CartoonFX;
using UnityEngine;

public class DamageText : MonoBehaviour
{
    [SerializeField] private CFXR_ParticleText particleText;

    public void Start()
    {
        Show(20);        
    }

    public void Show(int value)
    {
        particleText.UpdateText(value.ToString());
    }

    public void Show(string text)
    {
        particleText.UpdateText(text);
    }
}