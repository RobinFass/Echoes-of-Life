using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Video;

namespace CutScenes
{
    public class CutSceneVideosManager : MonoBehaviour
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
            foreach (var video in videos)
            {
                video.gameObject.SetActive(true);
                video.Play();
                await Task.Delay((int)video.length*1000);
                //await Task.Delay(1000);
                video.gameObject.SetActive(false);
            }
            SceneLoader.LoadScene(Scenes.GameScene);
        }
    }
}