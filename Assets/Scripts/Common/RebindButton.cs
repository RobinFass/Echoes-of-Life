using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Common
{
    public class RebindButton : MonoBehaviour
    {
        [SerializeField] private InputActionReference actionReference;
        [SerializeField] private TextMeshProUGUI bindingDisplayText;

        private void Start() => UpdateDisplay();

        public void OnClick_StartRebind()
        {
            RebindManager.Instance.StartRebind(actionReference, name, bindingDisplayText);
        }

        private void UpdateDisplay()
        {
            if (!actionReference || actionReference.action == null || !bindingDisplayText)
            {
                if (bindingDisplayText) bindingDisplayText.text = "";
                return;
            }
            var action = actionReference.action;
            if (!string.IsNullOrEmpty(name))
            {
                for (int i = 0; i < action.bindings.Count; i++)
                {
                    var b = action.bindings[i];
                    if (!b.isPartOfComposite || !string.Equals(b.name, name, StringComparison.OrdinalIgnoreCase)) continue;
                    bindingDisplayText.text = action.GetBindingDisplayString(i);
                    return;
                }
                for (int i = 0; i < action.bindings.Count; i++)
                {
                    var b = action.bindings[i];
                    if (b.isPartOfComposite || string.IsNullOrEmpty(b.effectivePath)) continue;
                    bindingDisplayText.text = action.GetBindingDisplayString(i);
                    return;
                }
            }
            bindingDisplayText.text = "";
        }
    }
}
