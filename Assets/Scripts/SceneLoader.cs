using UnityEngine.SceneManagement;

public static class SceneLoader
{
    public static void LoadScene(Scenes sceneName)
    {
        SceneManager.LoadScene(sceneName.ToString());
    }
}