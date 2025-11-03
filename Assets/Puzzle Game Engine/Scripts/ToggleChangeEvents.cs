using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using HyperPuzzleEngine;

namespace HyperPuzzleEngine
{
    public class ToggleChangeEvents : MonoBehaviour
    {
        public UnityEvent OnToggleOn, OnToggleOff;

        public void InvokeEvent(Toggle toggle)
        {
            if (toggle.isOn)
                OnToggleOn.Invoke();
            else
                OnToggleOff.Invoke();
        }
    }
}
