using UnityEngine;
using UnityEngine.EventSystems;

public class SliderReleaseSound : MonoBehaviour, IPointerUpHandler
{
    public AudioSort audio_sort;
    public AudioClip clip;

    public void OnPointerUp(PointerEventData eventData)
    {
        AudioManager.Instance.PlaySoundOnce(audio_sort, clip);
    }
}