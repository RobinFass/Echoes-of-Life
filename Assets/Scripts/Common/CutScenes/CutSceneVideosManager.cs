using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Video;

namespace Common.CutScenes
{
    public class CutSceneVideosManager : MonoBehaviour
    {
        [SerializeField] private VideoPlayer[] videos;

        private static GameInput Input => GameInput.Instance;
        private bool skipRequested;
        
        private void Awake()
        {
            foreach (var video in videos)
            {
                video.gameObject.SetActive(false);
            }
        }
        
        private void Start()
        {
            Input.OnDashEvent += SkipCutscene;
            PlayCutscene();
        }

        private void SkipCutscene(object sender, EventArgs e)
        {
            skipRequested = true;
        }

        private async void PlayCutscene()
        {
            foreach (var video in videos)
            {
                video.gameObject.SetActive(true);
                video.Play();
                float duration = (float)video.length + 2f;
                const int steps = 1000;
                for (int i = 0; i < steps; i++)
                {
                    if (skipRequested)
                    {
                        skipRequested = false;
                        break;
                    }
                    await Task.Delay((int)(duration * 1000 / steps));
                }
                video.gameObject.SetActive(false);
            }
            SceneLoader.LoadScene(Scenes.GameScene);
        }
    }
}
