using DG.Tweening;
using UnityEngine.SceneManagement;

public enum SceneType
{
    Title,
    Battle,
    Map,
    Loading,
    Main
}


public static class SceneLoader
{
    private const string LOADING_SCENE = "Loading Scene";

    public static void Load(SceneType type)
    {
        DOTween.KillAll();
        LoadingData.next_scene = GetSceneName(type);
        SceneManager.LoadScene(LOADING_SCENE);
    }

    public static void LoadInstance(SceneType type)
    {
        DOTween.KillAll();
        SceneManager.LoadScene(GetSceneName(type));
    }

    private static string GetSceneName(SceneType type)
    {
        return type switch
        {
            SceneType.Title => "Title Scene",
            SceneType.Battle => "Battle Scene",
            SceneType.Map => "Map Scene",
            SceneType.Loading => "Loading Scene",
            SceneType.Main => "Main Scene",
            _ => "Title Scene"
        };
    }
}