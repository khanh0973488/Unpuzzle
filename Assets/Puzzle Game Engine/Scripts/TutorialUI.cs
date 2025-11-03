using UnityEngine;
using HyperPuzzleEngine;

namespace HyperPuzzleEngine
{
    public class TutorialUI : MonoBehaviour
    {
        public bool showTutorialOnFocused = true;
        public float tutorialDuration = 3f;

        [Space]
        public GameObject panel;

        public Animation[] flashingAnimationsUI;

        private void Start()
        {
            if (showTutorialOnFocused)

                ShowTutorial(true);
        }

        public void ShowTutorial(bool isHidingAfterTimeIsUp = false)
        {
            CancelInvoke(nameof(HideTutorial));

            panel.SetActive(true);

            foreach (Animation flashingAnim in flashingAnimationsUI)
            {
                if (flashingAnim != null)
                    flashingAnim.gameObject.SetActive(true);
            }

            if (isHidingAfterTimeIsUp)
                Invoke(nameof(HideTutorial), tutorialDuration);
        }

        public void HideTutorial()
        {
            panel.SetActive(false);

            foreach (Animation flashingAnim in flashingAnimationsUI)
            {
                if (flashingAnim != null)
                    flashingAnim.gameObject.SetActive(false);
            }
        }
    }
}
