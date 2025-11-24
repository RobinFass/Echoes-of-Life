
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Common
{
    public class RebindManager : MonoBehaviour
    {
        private const string PrefKeyPrefix = "rebinds_";
        private static RebindManager instance;

        public static RebindManager Instance
        {
            get
            {
                if (!instance)
                {
                    var go = new GameObject("RebindManager");
                    instance = go.AddComponent<RebindManager>();
                    DontDestroyOnLoad(go);
                }

                return instance;
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
            var bindingIndex = GetBindingIndexForPart(action, partName);
            if (bindingIndex == -1)
            {
                Debug.LogError(
                    $"StartRebind: binding pour la partie '{partName}' introuvable sur l'action '{action.name}'");
                return;
            }

            if (displayText != null) displayText.text = "...";
            StartCoroutine(RebindCoroutine(action, bindingIndex, displayText));
        }

        private IEnumerator RebindCoroutine(InputAction action, int bindingIndex, TextMeshProUGUI displayText)
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
                if (displayText != null)
                {
                    var newIndex = bindingIndex;
                    displayText.text = action.GetBindingDisplayString(newIndex);
                }
            });

            rebind.Start();
        }

        private int GetBindingIndexForPart(InputAction action, string part)
        {
            if (action == null || string.IsNullOrEmpty(part)) return -1;

            for (var i = 0; i < action.bindings.Count; i++)
            {
                var b = action.bindings[i];
                if (b.isPartOfComposite && string.Equals(b.name, part, System.StringComparison.OrdinalIgnoreCase))
                    return i;
            }

            for (var i = 0; i < action.bindings.Count; i++)
                if (!action.bindings[i].isPartOfComposite && string.IsNullOrEmpty(action.bindings[i].name))
                    return i;

            return -1;
        }

        private void SaveOverridesForMap(InputActionMap map)
        {
            if (map == null) return;
            var json = map.SaveBindingOverridesAsJson();
            PlayerPrefs.SetString(PrefKeyPrefix + map.id, json);
            PlayerPrefs.Save();
        }

        private void LoadOverridesForMap(InputActionMap map)
        {
            if (map == null) return;
            var key = PrefKeyPrefix + map.id;
            if (!PlayerPrefs.HasKey(key)) return;
            var json = PlayerPrefs.GetString(key);
            if (!string.IsNullOrEmpty(json))
                map.LoadBindingOverridesFromJson(json);
        }

        // Appliquer les overrides sauvegardés sur un map (déjà présent)
        public void ApplySavedOverridesToMap(InputActionMap map)
        {
            LoadOverridesForMap(map);
        }

        // Nouvelle méthode : appliquer les overrides à toutes les maps d'un asset
        public void ApplySavedOverridesToAsset(InputActionAsset asset)
        {
            if (asset == null) return;
            foreach (var map in asset.actionMaps)
                LoadOverridesForMap(map);
        }

        // Nouvelle méthode : appliquer les overrides pour une action (utile si vous créez des instances runtime)
        public void ApplySavedOverridesToAction(InputAction action)
        {
            if (action?.actionMap == null) return;
            LoadOverridesForMap(action.actionMap);
        }
    }
}