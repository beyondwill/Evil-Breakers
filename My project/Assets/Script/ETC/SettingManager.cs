using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingManager : MonoBehaviour
{
    // 게임 종료
    public void GamePause()
    {
        Time.timeScale = 0f;
    }
    
    // 게임 재개
    public void GameResume()
    {
        Time.timeScale = 1f;
    }

    // 씬 다시 불러오기
    public void ReloadScene()
    {
        SceneLoader.LoadInstance(SceneType.Battle);
    }

    // 게임 끝내기
    public void EndGame()
    {
        Debug.Log("게임 종료");

        #if UNITY_EDITOR
                EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}