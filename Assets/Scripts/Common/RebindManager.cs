using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Common
{
    public class RebindManager : MonoBehaviour
    {
        private const string PrefKeyPrefix = "rebinds_";
        private static RebindManager _instance;

        public static RebindManager Instance
        {
            get
            {
                if (_instance) return _instance;
                var go = new GameObject("RebindManager");
                _instance = go.AddComponent<RebindManager>();
                DontDestroyOnLoad(go);
                return _instance;
            }
        }

        public void StartRebind(InputActionReference actionRef, string partName, TextMeshProUGUI displayText = null)
        {
            if (actionRef == null || actionRef.action == null)
            {
                Debug.LogError("StartRebind: actionReference null");
                return;
            }
            var action = actionRef.action;
            LoadOverridesForMap(action.actionMap);
            int bindingIndex = GetBindingIndexForPart(action, partName);
            if (bindingIndex == -1)
            {
                Debug.LogError(
                    $"StartRebind: binding pour la partie '{partName}' introuvable sur l'action '{action.name}'");
                return;
            }
            if (displayText != null) displayText.text = "...";
            StartCoroutine(RebindCoroutine(action, bindingIndex, displayText));
        }

        private static IEnumerator RebindCoroutine(InputAction action, int bindingIndex, TextMeshProUGUI displayText)
        {
            yield return new WaitForEndOfFrame();
            if (action == null) yield break;
            action.Disable();
            var rebind = action.PerformInteractiveRebinding(bindingIndex)
                .WithControlsExcluding("<Mouse>/*")
                .WithControlsExcluding("<Pointer>/*")
                .WithControlsExcluding("<Pen>/*");
            rebind.OnComplete(op =>
            {
                op.Dispose();
                action.Enable();
                SaveOverridesForMap(action.actionMap);
                if (displayText == null) return;
                displayText.text = action.GetBindingDisplayString(bindingIndex);
            });
            rebind.Start();
        }

        private static int GetBindingIndexForPart(InputAction action, string part)
        {
            if (action == null || string.IsNullOrEmpty(part)) return -1;
            for (int i = 0; i < action.bindings.Count; i++)
            {
                var b = action.bindings[i];
                if (b.isPartOfComposite && string.Equals(b.name, part, System.StringComparison.OrdinalIgnoreCase))
                    return i;
            }
            for (int i = 0; i < action.bindings.Count; i++)
                if (!action.bindings[i].isPartOfComposite && string.IsNullOrEmpty(action.bindings[i].name))
                    return i;
            return -1;
        }

        private static void SaveOverridesForMap(InputActionMap map)
        {
            if (map == null) return;
            string json = map.SaveBindingOverridesAsJson();
            PlayerPrefs.SetString(PrefKeyPrefix + map.id, json);
            PlayerPrefs.Save();
        }

        private static void LoadOverridesForMap(InputActionMap map)
        {
            if (map == null) return;
            string key = PrefKeyPrefix + map.id;
            if (!PlayerPrefs.HasKey(key)) return;
            string json = PlayerPrefs.GetString(key);
            if (!string.IsNullOrEmpty(json)) map.LoadBindingOverridesFromJson(json);
        }
        
        public static void ApplySavedOverridesToAsset(InputActionAsset asset)
        {
            if (asset == null) return;
            foreach (var map in asset.actionMaps)
                LoadOverridesForMap(map);
        }
    }
}
