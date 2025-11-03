using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using HyperPuzzleEngine;

namespace HyperPuzzleEngine
{
    public class ProgressBar : MonoBehaviour
    {
        public bool isInstantlyUpdating = false;
        public float updateSpeed = 10f;

        [Space]
        public CheckNeighbours gridToCheck;

        private bool canChangeProgress = true;

        public Image progressBar;

        [Range(0f, 1f)]
        [SerializeField] private float currentProgress;

        [Space]
        public bool isUsingDynamicColorIndicator = true;
        public Gradient dynamicColorIndicator;


        [Space]
        [Header("Events")]
        public UnityEvent OnZeroProgress;
        public UnityEvent OnFullProgress;
        public UnityEvent OnProgressChanged;

        private void Start()
        {
            SetProgress(currentProgress);
        }

        public void CheckProgress()
        {

            if (gridToCheck == null) return;

            SetProgress((float)gridToCheck.transform.childCount / (float)gridToCheck.countOfStacksNeededToMatch);
        }

        public void SetProgress(float newProgressValue)
        {
            if (!canChangeProgress) return;

            if (newProgressValue > 0f)
            {
                foreach (Image image in transform.GetComponentsInChildren<Image>())
                    image.enabled = true;
            }

            currentProgress = newProgressValue;
            currentProgress = Mathf.Clamp(currentProgress, 0f, 1f); // prevents going below 0 or above maxProgress

            OnProgressChanged.Invoke();

            if (currentProgress >= 1f) OnFullProgress.Invoke();

            if (currentProgress <= 0f) OnZeroProgress.Invoke();
        }

        public void SetProgressOfCollectedStacksCount(CollectedStacksCounter counter)
        {
            if (!canChangeProgress) return;

            float newProgressValue = (float)counter.GetCurrentlyCollected() / (float)counter.GetCurrentlyNeededToCollect(); ;

            if (newProgressValue > 0f)
            {
                foreach (Image image in transform.GetComponentsInChildren<Image>())
                    image.enabled = true;
            }

            currentProgress = newProgressValue;
            currentProgress = Mathf.Clamp(currentProgress, 0f, 1f); // prevents going below 0 or above maxProgress

            OnProgressChanged.Invoke();

            if (currentProgress >= 1f) OnFullProgress.Invoke();

            if (currentProgress <= 0f) OnZeroProgress.Invoke();
        }

        public void IncreaseProgressBar(float value)
        {
            SetProgress(currentProgress + (float)value);
        }

        private void Update()
        {
            // Smoothly update the progress bar's fill amount
            if (!isInstantlyUpdating)
                progressBar.fillAmount = Mathf.Lerp(progressBar.fillAmount, currentProgress, Time.deltaTime * updateSpeed); // Adjust the 10f to your liking for speed
            else
                progressBar.fillAmount = currentProgress; // Adjust the 10f to your liking for speed

            if (isUsingDynamicColorIndicator && canChangeProgress)
            {
                progressBar.color = dynamicColorIndicator.Evaluate(progressBar.fillAmount);
            }
        }

        public void SetVisibility(bool isEnabled)
        {
            foreach (Image image in transform.GetComponentsInChildren<Image>())
                image.enabled = isEnabled;
        }
    }
}