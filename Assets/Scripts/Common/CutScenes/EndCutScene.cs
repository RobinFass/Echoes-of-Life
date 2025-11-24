using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Video;

namespace CutScenes
{
    public class EndCutScene : MonoBehaviour
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
            PlayEndingCutscene();
        }

        private void SkipCutscene(object sender, EventArgs e)
        {
            skipRequested = true;
        }

        private async void PlayEndingCutscene()
        {
            int index = (GameManager.Instance.State == GameState.Won) ? 0 : 1;
            var video = videos[index];

            video.gameObject.SetActive(true);
            video.Play();

            var duration = (float)video.length + 2f;
            var steps = 50;

            for (var i = 0; i < steps; i++)
            {
                if (skipRequested)
                    break;

                await Task.Delay((int)(duration * 1000 / steps));
            }

            video.gameObject.SetActive(false);

            if (index == 0)
                GameManager.levelNumber = 0;

            SceneLoader.LoadScene(Scenes.HomeScene);
        }

    }
}