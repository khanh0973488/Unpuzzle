using UnityEngine;
using UnityEngine.Events;
using TMPro;
using HyperPuzzleEngine;

namespace HyperPuzzleEngine
{
    public class DropdownChangeEvents : MonoBehaviour
    {
        public UnityEvent OnValueIs0, OnValueIs1, OnValueIs2, OnValueIs3;

        public void InvokeEvent(TMP_Dropdown dropdown)
        {
            switch (dropdown.value)
            {
                case 0:
                    OnValueIs0.Invoke();
                    break;
                case 1:
                    OnValueIs1.Invoke();
                    break;
                case 2:
                    OnValueIs2.Invoke();
                    break;
                case 3:
                    OnValueIs3.Invoke();
                    break;
            }
        }
    }
}
