using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class TitleManager : MonoBehaviour
{
    // 테스트 옵션
    [Header("Test")]
    public bool skip_title_sequence = false;

    // 외부 요소
    [Header("UI")]
    public CanvasGroup logoBox;
    public CanvasGroup titleBox;

    // 텍스트 변수
    [Header("Text")]
    public TextMeshProUGUI logoText;

    // 음악 변수
    [Header("BGM")]
    [SerializeField] private AudioClip title_bgm;


    void Start()
    {
        Init();

        if (skip_title_sequence)
        {
            SkipSequence();
        }
        else
        {
            PlaySequence();
        }
    }


    void Init()
    {
        logoBox.alpha = 0f;
        titleBox.alpha = 0f;

        titleBox.gameObject.SetActive(false);
    }


    void PlaySequence()
    {
        Sequence seq = DOTween.Sequence();

        seq.AppendInterval(1f);

        seq.AppendCallback(() => AudioManager.Instance.PlayBGM(title_bgm));
        seq.Append(logoBox.DOFade(1f, 1f));
        seq.Join(logoText.DOFade(1f, 1f));

        seq.AppendInterval(1.5f);

        seq.Append(logoBox.DOFade(0f, 1f));

        seq.AppendCallback(() =>
        {
            logoBox.gameObject.SetActive(false);
            titleBox.gameObject.SetActive(true);
        });

        seq.Append(titleBox.DOFade(1f, 1f));
    }


    void SkipSequence()
    {
        AudioManager.Instance.PlayBGM(title_bgm);

        logoBox.gameObject.SetActive(false);

        titleBox.gameObject.SetActive(true);
        titleBox.alpha = 1f;
    }


    // 게임 시작
    public void GameStart()
    {
        if (DataManager.Instance.GetAllData.GetCurrentState() == CurrentState.MainScreen)
        {
            SceneManager.LoadScene("Main Scene");
        }
        else
        {
            if (DataManager.Instance.GetBattleData.enemyCharacterList.Count == 0)
            {
                SceneManager.LoadScene("Map Scene");
            }
            else
            {
                SceneManager.LoadScene("Battle Scene");
            }
        }
    }


    // 새 게임 시작
    public void NewGameStart()
    {
        // StreamingAssets 폴더
        string streamingAssetsPath = Application.streamingAssetsPath;

        // 원본 JSON 경로
        string defaultPath = Path.Combine(
            streamingAssetsPath,
            "Default_file.json"
        );

        // 실제 세이브 파일 경로
        string savePath = Path.Combine(
            streamingAssetsPath,
            "savefile.json"
        );

        // 원본 JSON이 존재하는지 확인
        if (!File.Exists(defaultPath))
        {
            Debug.LogError("Default_file.json을 찾을 수 없습니다.");
            Debug.LogError("경로: " + defaultPath);
            return;
        }

        try
        {
            // 원본 JSON 읽기
            string defaultJson = File.ReadAllText(defaultPath);

            // savefile.json에 덮어쓰기
            File.WriteAllText(savePath, defaultJson);

            Debug.Log("새 게임 데이터로 초기화했습니다.");
            Debug.Log("저장 위치: " + savePath);

            // 새로 저장된 데이터를 DataManager에 적용
            DataManager.Instance.LoadData();

            // 새 게임 시작
            SceneManager.LoadScene("Main Scene");
        }
        catch (System.Exception e)
        {
            Debug.LogError("새 게임 데이터 복사 중 오류 발생: " + e);
        }
    }
}