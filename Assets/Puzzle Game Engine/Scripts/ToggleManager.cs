using HyperPuzzleEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperPuzzleEngine
{
    public class ToggleManager : MonoBehaviour
    {
        public string soundSaveString;
        public string vibrationSaveString;

        bool isSoundOn;
        bool isVibrationOn;

        public Animation soundToggleAnim;
        public Animation vibrationToggleAnim;

        private void Awake()
        {
            CheckSound();
            CheckVibration();
        }

        public void ChangeToggle_Sound()
        {
            if (isSoundOn)
                PlayerPrefs.SetInt(soundSaveString, 1);
            else
                PlayerPrefs.SetInt(soundSaveString, 0);

            CheckSound();
        }

        public void ChangeToggle_Vibration()
        {
            if (isVibrationOn)
                PlayerPrefs.SetInt(vibrationSaveString, 1);
            else
                PlayerPrefs.SetInt(vibrationSaveString, 0);

            CheckVibration();
        }

        private void CheckSound()
        {
            if (PlayerPrefs.GetInt(soundSaveString, 0) == 0)
                isSoundOn = true;
            else
                isSoundOn = false;

            GetComponentInParent<SoundsManagerForTemplate>().thisTemplateCanPlaySounds = isSoundOn;
            FindObjectOfType<AudioListener>().enabled = isSoundOn;

            if (isSoundOn)
                soundToggleAnim.Play("ToggleOn");
            else
                soundToggleAnim.Play("ToggleOff");
        }

        private void CheckVibration()
        {
            if (PlayerPrefs.GetInt(vibrationSaveString, 0) == 0)
                isVibrationOn = true;
            else
                isVibrationOn = false;

            if (isVibrationOn)
                vibrationToggleAnim.Play("ToggleOn");
            else
                vibrationToggleAnim.Play("ToggleOff");
        }
    }
}