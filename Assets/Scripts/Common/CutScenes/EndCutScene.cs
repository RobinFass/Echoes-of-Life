using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Video;

namespace Common.CutScenes
{
    public class EndCutScene : MonoBehaviour
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
            float duration = (float)video.length + 2f;
            const int steps = 1000;
            for (int i = 0; i < steps; i++)
            {
                if (skipRequested) break;
                await Task.Delay((int)(duration * 1000 / steps));
            }
            video.gameObject.SetActive(false);
            if (index == 0) GameManager.LevelNumber = 0;
            SceneLoader.LoadScene(Scenes.HomeScene);
        }
    }
}
