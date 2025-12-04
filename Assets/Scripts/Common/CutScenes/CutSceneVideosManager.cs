using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Video;

namespace CutScenes
{
    public class CutSceneVideosManager : MonoBehaviour
    {
        [SerializeField] private VideoPlayer[] videos;

        private GameInput input => GameInput.Instance;
        private bool skipRequested = false;
        
        private void Awake()
        {
            foreach (var video in videos)
            {
                video.gameObject.SetActive(false);
            }
        }
        
        private void Start()
        {
            input.OnDashEvent += SkipCutscene;
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
                if (skipRequested)
                {
                    skipRequested = false;
                    break;
                }

                video.gameObject.SetActive(true);
                video.Play();

                var duration = (float)video.length + 2f;
                var steps = 1000;
                for (var i = 0; i < steps; i++)
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