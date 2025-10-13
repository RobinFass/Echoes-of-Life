public static class SceneLoader
{
    public static void LoadScene(Scenes sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName.ToString());
    }
}
