using UnityEngine;

namespace UI
{
    public class MapUI : MonoBehaviour
    {
        public static MapUI Instance { get; private set; }
    
        private void Awake()
        {
            Instance = this;
            Hide();
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }
    
        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
