using UnityEngine;
using UnityEngine.UI;

public class OptionManager : MonoBehaviour
{
    [Header("Volume Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider voiceSlider;
    [SerializeField] private Slider sfxSlider;

    private void Start()
    {
        InitSlider();
        AddListener();
    }

    // 저장값 불러오기
    private void InitSlider()
    {
        masterSlider.value = PlayerPrefs.GetFloat("Master", 100f);
        bgmSlider.value = PlayerPrefs.GetFloat("BGM", 100f);
        voiceSlider.value = PlayerPrefs.GetFloat("Voice", 100f);
        sfxSlider.value = PlayerPrefs.GetFloat("UI", 100f);
    }

    // 슬라이더 이벤트 연결
    private void AddListener()
    {
        masterSlider.onValueChanged.AddListener(ChangeMasterVolume);
        bgmSlider.onValueChanged.AddListener(ChangeBGMVolume);
        voiceSlider.onValueChanged.AddListener(ChangeVoiceVolume);
        sfxSlider.onValueChanged.AddListener(ChangeSFXVolume);
    }

    // 마스터
    private void ChangeMasterVolume(float value)
    {
        AudioManager.Instance.ChangeAudioVolume(AudioSort.Master, value);
    }

    // 배경음
    private void ChangeBGMVolume(float value)
    {
        AudioManager.Instance.ChangeAudioVolume(AudioSort.BGM, value);
    }

    // 음성
    private void ChangeVoiceVolume(float value)
    {
        AudioManager.Instance.ChangeAudioVolume(AudioSort.Voice, value);
    }

    // 효과음
    private void ChangeSFXVolume(float value)
    {
        AudioManager.Instance.ChangeAudioVolume(AudioSort.UI, value);
    }
}