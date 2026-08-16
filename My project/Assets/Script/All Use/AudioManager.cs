using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

public enum AudioSort
{
    Master,
    BGM,
    BGM2,
    Battle,
    UI,
    Voice,
    Card
}

public enum SFX
{
    BattleStart,
    TurnStart,
    Attack,
    Click
}

[System.Serializable]
public class AudioData
{
    public AudioSort audio_sort;
    public AudioSource audio_source;
}

[System.Serializable]
public class SFXData
{
    public SFX type;
    public AudioClip clip;
    public AudioSort audioSort; // 어떤 채널로 재생할지
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [SerializeField] private AudioMixer audio_mixer;

    [SerializeField] private List<AudioData> audio_data_list;

    [SerializeField] private List<SFXData> sfx_data_list;

    private Tween bgmTween;

    void Start()
    {
        LoadAudioVolume();
    }

    // AudioSource 찾기
    private AudioSource FindAudioSource(AudioSort audio_sort)
    {
        return audio_data_list.Find(x => x.audio_sort == audio_sort).audio_source;
    }

    // SFX 재생 (핵심)
    public void PlaySFX(SFX type)
    {
        SFXData data = sfx_data_list.Find(x => x.type == type);

        if (data == null)
        {
            Debug.LogWarning($"SFX 없음: {type}");
            return;
        }

        AudioSource source = FindAudioSource(data.audioSort);
        source.PlayOneShot(data.clip);
    }

    // 소리 재생
    public void PlaySoundOnce(AudioSort audio_sort, AudioClip audio_clip)
    {
        AudioSource source = FindAudioSource(audio_sort);
        source.PlayOneShot(audio_clip);
    }

    // BGM 재생
    public void PlayBGM(AudioClip audio_clip, float time = 1f)
    {
        AudioSource source = FindAudioSource(AudioSort.BGM);

        if (source.clip == audio_clip && source.isPlaying)
            return;

        if (source.clip == null || !source.isPlaying)
        {
            bgmTween?.Kill();

            source.clip = audio_clip;
            source.loop = true;
            source.volume = 1f;
            source.Play();
            return;
        }

        bgmTween?.Kill();

        bgmTween = source
            .DOFade(0f, time)
            .OnComplete(() =>
            {
                source.clip = audio_clip;
                source.loop = true;
                source.Play();

                source.DOFade(1f, 0f);
            });
    }

    // BGM 페이드 아웃 (끄기)
    public void FadeOutBGM(float duration = 1f)
    {
        AudioSource source = FindAudioSource(AudioSort.BGM);

        if (source == null || !source.isPlaying)
            return;

        bgmTween?.Kill();

        bgmTween = source
            .DOFade(0f, duration)
            .OnComplete(() =>
            {
                source.Stop();
                source.clip = null;
                source.volume = 1f;   // 추가
            });
    }

    // 볼륨 변경
    public void ChangeAudioVolume(AudioSort audio_sort, float volume)
    {
        float normalized = Mathf.Clamp(volume / 100f, 0.0001f, 1f);
        float dB = Mathf.Log10(normalized) * 20f;

        string param = GetGroupName(audio_sort);

        audio_mixer.SetFloat(param, dB);
        PlayerPrefs.SetFloat(param, volume);
    }

    // 볼륨 불러오기
    public void LoadAudioVolume()
    {
        HashSet<string> loaded = new HashSet<string>();

        foreach (AudioSort sort in System.Enum.GetValues(typeof(AudioSort)))
        {
            string key = GetGroupName(sort);

            if (loaded.Contains(key)) continue;
            loaded.Add(key);

            float value = PlayerPrefs.GetFloat(key, 100f);
            ChangeAudioVolume(sort, value);
        }
    }

    // 그룹 이름
    private string GetGroupName(AudioSort sort)
    {
        string name = sort.ToString();
        return new string(name.Where(c => !char.IsDigit(c)).ToArray());
    }
}