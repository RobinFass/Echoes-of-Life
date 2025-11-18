using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Video;

namespace CutScenes
{
    public class EndCutScene : MonoBehaviour
    {
        [SerializeField] private VideoPlayer[] videos;

        private void Awake()
        {
            foreach (var video in videos)
            {
                video.gameObject.SetActive(false);
            }
        }

        private async void Start()
        {
            if(GameManager.Instance.State == GameState.Won)
            {
                videos[0].gameObject.SetActive(true);
                videos[0].Play();
                await Task.Delay((int)videos[0].length*1000);
                videos[0].gameObject.SetActive(false);
                GameManager.levelNumber = 0;
            }
            else
            {
                videos[1].gameObject.SetActive(true);
                videos[1].Play();
                await Task.Delay((int)videos[1].length*1000);
                videos[1].gameObject.SetActive(false);
            }
            SceneLoader.LoadScene(Scenes.HomeScene);
        }
    }
}