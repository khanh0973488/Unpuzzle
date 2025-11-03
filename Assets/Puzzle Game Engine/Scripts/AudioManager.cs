using UnityEngine;
using HyperPuzzleEngine;
using System;

namespace HyperPuzzleEngine
{
    public class AudioManager : MonoBehaviour
    {
        #region Create Instance

        public static AudioManager Instance;

        private void Awake()
        {
            Instance = this;
        }

        #endregion

        private bool canPlaySounds = true;

        public GameObject soundEffectSource;
        public AudioClip[] soundEffects;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Break();
            }
        }

        public bool IsPlaying(AudioClip soundClip)
        {
            foreach (AudioSource sources in soundEffectSource.GetComponents<AudioSource>())
            {
                if (sources.clip == soundClip)
                {
                    return sources.isPlaying;
                }
            }
            return false;
        }

        public void PlaySoundEffect(string nameOfSoundClip)
        {
            if (canPlaySounds)
            {
                AudioClip clipToPlay = null;

                for (int i = 0; i < soundEffects.Length; i++)
                {
                    if (soundEffects[i].name == nameOfSoundClip)
                    {
                        clipToPlay = soundEffects[i];
                        break;
                    }
                }

                if (clipToPlay != null)
                {
                    AudioSource newSource = soundEffectSource.AddComponent<AudioSource>();
                    newSource.clip = clipToPlay;
                    newSource.Play();
                }
            }
        }

        public void PlaySoundEffect(AudioClip soundClip)
        {
            if (canPlaySounds)
            {
                if (soundClip != null)
                {
                    AudioSource newSource = soundEffectSource.AddComponent<AudioSource>();
                    newSource.clip = soundClip;
                    newSource.Play();

                    Destroy(newSource, soundClip.length + 0.5f);
                }
            }
        }

        public void DisableAllSoundEffects()
        {
            canPlaySounds = false;
            foreach (AudioSource source in soundEffectSource.GetComponents<AudioSource>())
            {
                Destroy(source);
            }
        }
    }
}