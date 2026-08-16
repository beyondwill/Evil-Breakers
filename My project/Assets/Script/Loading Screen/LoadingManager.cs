using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{
    public Slider progressBar;

    void Start()
    {
        // 음악 끄기
        AudioManager.Instance.FadeOutBGM();
        progressBar.value = 0f; // 시작값 초기화
        StartCoroutine(LoadScene());
    }

    IEnumerator LoadScene()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(LoadingData.next_scene);
        op.allowSceneActivation = false;

        float currentProgress = 0f;

        while (!op.isDone)
        {
            // 실제 진행률 (0 ~ 1)
            float targetProgress = Mathf.Clamp01(op.progress / 0.9f);

            // 부드럽게 따라가기
            currentProgress = Mathf.Lerp(currentProgress, targetProgress, Time.deltaTime * 5f);
            progressBar.value = currentProgress;

            // 거의 다 찼을 때 100%로 보정
            if (targetProgress >= 1f && currentProgress >= 0.99f)
            {
                progressBar.value = 1f;

                yield return new WaitForSeconds(1f); // 연출용
                op.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}