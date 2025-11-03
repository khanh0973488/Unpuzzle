using TMPro;
using UnityEngine;
using UnityEngine.UI;
using HyperPuzzleEngine;

namespace HyperPuzzleEngine
{
    public class SliderValueUI : MonoBehaviour
    {
        public Slider slider;

        private TextMeshProUGUI tmpro;

        private void Start()
        {
            tmpro = GetComponent<TextMeshProUGUI>();

            UpdateTextBasedOnSliderValue();
        }

        public void UpdateTextBasedOnSliderValue()
        {
            if (slider.wholeNumbers)
                tmpro.text = slider.value.ToString();
            else
                tmpro.text = slider.value.ToString("N2");
        }
    }
}