using HyperPuzzleEngine;
using UnityEngine;

namespace HyperPuzzleEngine
{
    public class AudioButton : MonoBehaviour
    {
        private GameObject offImage;
        private SoundsManagerForTemplate soundsManager;

        public string saveStringPrefix = "audio";

        void Start()
        {
            offImage = transform.GetChild(0).gameObject;
            soundsManager = GetComponentInParent<SoundsManagerForTemplate>();

            if (soundsManager == null) return;

            SetSoundManager();
            SetOffImage();
        }

        private void SetSoundManager()
        {
            if (PlayerPrefs.GetInt(saveStringPrefix + soundsManager.gameObject.name, 0) == 0)
                soundsManager.thisTemplateCanPlaySounds = true;
            else
                soundsManager.thisTemplateCanPlaySounds = false;
        }

        private void SetOffImage()
        {
            if (PlayerPrefs.GetInt(saveStringPrefix + soundsManager.gameObject.name, 0) == 0)
                offImage.SetActive(false);
            else
                offImage.SetActive(true);
        }

        public void ChangeAudioSetting()
        {
            if (soundsManager == null) return;

            if (PlayerPrefs.GetInt(saveStringPrefix + soundsManager.gameObject.name, 0) == 0)
                PlayerPrefs.SetInt(saveStringPrefix + soundsManager.gameObject.name, 1);
            else
                PlayerPrefs.SetInt(saveStringPrefix + soundsManager.gameObject.name, 0);

            SetSoundManager();
            SetOffImage();
        }

        public void ChangeAudioSetting(bool isOn)
        {
            int soundValue = 0;
            if (!isOn)
                soundValue = 1;

            PlayerPrefs.SetInt(saveStringPrefix + soundsManager.gameObject.name, soundValue);

            SetOffImage();
        }
    }
}