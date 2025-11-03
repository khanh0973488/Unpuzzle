using UnityEngine;
using HyperPuzzleEngine;

namespace HyperPuzzleEngine
{
    [ExecuteAlways]
    public class SoundsManagerForTemplate : MonoBehaviour
    {
        public bool updateToWavs = false;
        public AudioClip[] wavs;

        public bool thisTemplateCanPlaySounds = true;

        #region Public Variables To Assign Audioclips

        [Space]
        [Header("Stack Sounds")]
        [SerializeField] AudioClip soundsEffect_Stack_Appear;
        [SerializeField] AudioClip soundsEffect_Stack_Match;
        [SerializeField] AudioClip soundsEffect_Stack_StartedToJump;
        [SerializeField] AudioClip soundsEffect_Stack_Jumped;
        [SerializeField] AudioClip soundsEffect_Stack_Selected;
        [SerializeField] AudioClip soundsEffect_Stack_Moved;
        [SerializeField] AudioClip soundEffect_Stack_Clickable_FailedMatch;
        [SerializeField] AudioClip soundEffect_Stack_LockIn_Match;

        [Space]
        [Header("Grid Sounds")]
        [SerializeField] AudioClip soundsEffect_Grid_Unlocked;
        [SerializeField] AudioClip soundsEffect_Grid_FailedToUnlock;

        [Space]
        [Header("Button Sounds")]
        [SerializeField] AudioClip soundsEffect_Button_Pressed;
        [SerializeField] AudioClip soundsEffect_Button_Dealing;

        [Space]
        [Header("Level Sounds")]
        [SerializeField] AudioClip soundsEffect_Level_Won;
        [SerializeField] AudioClip soundsEffect_Level_Failed;

        [Space]
        [Header("Slices Sounds")]
        [SerializeField] AudioClip soundsEffect_Slices_Filled;
        [SerializeField] AudioClip soundsEffect_Slices_PieceMoved;

        [Space]
        [Header("Key Unlock Sounds")]
        [SerializeField] AudioClip soundsEffect_Key_Clicked;
        [SerializeField] AudioClip soundsEffect_Key_Unlocked;

        [Space]
        [Header("Skill Sounds")]
        [SerializeField] AudioClip soundsEffect_Skill_Hammer_Smash;

        [Space]
        [Header("Block Sounds")]
        [SerializeField] AudioClip soundsEffect_Block_Tapped;
        [SerializeField] AudioClip soundsEffect_Block_HitObstacle;
        [SerializeField] AudioClip soundsEffect_Block_DestroyedByObstacle;
        [SerializeField] AudioClip soundsEffect_Block_Cleared;

        [Space]
        [Header("Screw Jam Sounds")]
        [SerializeField] AudioClip soundsEffect_ScrewJam_ScrewOut;
        [SerializeField] AudioClip soundsEffect_ScrewJam_ScrewIn;
        [SerializeField] AudioClip soundsEffect_ScrewJam_ContainerFilled;
        [SerializeField] AudioClip soundsEffect_ScrewJam_ContainerIn;
        [SerializeField] AudioClip soundsEffect_ScrewJam_ContainerOut;

        [Space]
        [Header("Bottle Jam Sounds")]
        [SerializeField]
        AudioClip soundsEffect_BottleJam_BoxJump;
        [SerializeField] AudioClip soundsEffect_BottleJam_BoxFilled;
        [SerializeField] AudioClip soundsEffect_BottleJam_BottleJumpEnd;

        #endregion

        public void SetSoundEffects(bool isOn)
        {
            thisTemplateCanPlaySounds = isOn;

            if (GetComponentInChildren<AudioButton>() != null)
                GetComponentInChildren<AudioButton>().ChangeAudioSetting(isOn);
        }

        #region Stack Sounds

        public void PlaySound_Stack_Appear()
        {
            if (!thisTemplateCanPlaySounds) return;

            AudioManager.Instance.PlaySoundEffect(soundsEffect_Stack_Appear);
        }

        public void PlaySound_Stack_Match()
        {
            if (!thisTemplateCanPlaySounds) return;

            AudioManager.Instance.PlaySoundEffect(soundsEffect_Stack_Match);
        }

        public void PlaySound_Stack_Jumped()
        {
            if (!thisTemplateCanPlaySounds) return;

            AudioManager.Instance.PlaySoundEffect(soundsEffect_Stack_Jumped);
        }

        public void PlaySound_Stack_StartedToJump()
        {
            if (!thisTemplateCanPlaySounds) return;

            AudioManager.Instance.PlaySoundEffect(soundsEffect_Stack_StartedToJump);
        }

        public void PlaySound_Stack_Selected()
        {
            if (!thisTemplateCanPlaySounds) return;

            AudioManager.Instance.PlaySoundEffect(soundsEffect_Stack_Selected);
        }

        public void PlaySound_Stack_Moved()
        {
            if (!thisTemplateCanPlaySounds) return;

            AudioManager.Instance.PlaySoundEffect(soundsEffect_Stack_Moved);
        }

        public void PlaySound_Stack_Clickable_FailedMatch()
        {
            if (!thisTemplateCanPlaySounds) return;
            if (AudioManager.Instance.IsPlaying(soundEffect_Stack_Clickable_FailedMatch)) return;

            AudioManager.Instance.PlaySoundEffect(soundEffect_Stack_Clickable_FailedMatch);
        }

        public void PlaySound_Stack_LockIn_Match()
        {
            if (!thisTemplateCanPlaySounds) return;

            AudioManager.Instance.PlaySoundEffect(soundEffect_Stack_LockIn_Match);
        }

        #endregion

        #region Grid Sounds

        public void PlaySound_Grid_Unlocked()
        {
            if (!thisTemplateCanPlaySounds) return;

            AudioManager.Instance.PlaySoundEffect(soundsEffect_Grid_Unlocked);
        }

        public void PlaySound_Grid_FailedToUnlock()
        {
            if (!thisTemplateCanPlaySounds) return;

            AudioManager.Instance.PlaySoundEffect(soundsEffect_Grid_FailedToUnlock);
        }

        #endregion

        #region Button Sounds

        public void PlaySound_Button_Pressed()
        {
            if (!thisTemplateCanPlaySounds) return;

            AudioManager.Instance.PlaySoundEffect(soundsEffect_Button_Pressed);
        }

        public void PlaySound_Button_Dealing()
        {
            if (!thisTemplateCanPlaySounds) return;

            AudioManager.Instance.PlaySoundEffect(soundsEffect_Button_Dealing);
        }

        #endregion

        #region Level Sounds

        public void PlaySound_Level_Won()
        {
            if (!thisTemplateCanPlaySounds) return;

            AudioManager.Instance.PlaySoundEffect(soundsEffect_Level_Won);
        }

        public void PlaySound_Level_Failed()
        {
            if (!thisTemplateCanPlaySounds) return;

            AudioManager.Instance.PlaySoundEffect(soundsEffect_Level_Failed);
        }

        #endregion

        #region Slices Sounds

        public void PlaySound_Slices_Filled()
        {
            if (!thisTemplateCanPlaySounds) return;

            AudioManager.Instance.PlaySoundEffect(soundsEffect_Slices_Filled);
        }

        public void PlaySound_Slices_PieceMoved()
        {
            if (!thisTemplateCanPlaySounds) return;

            AudioManager.Instance.PlaySoundEffect(soundsEffect_Slices_PieceMoved);
        }

        #endregion

        #region Key Sounds

        public void PlaySound_Key_Clicked()
        {
            if (!thisTemplateCanPlaySounds) return;

            AudioManager.Instance.PlaySoundEffect(soundsEffect_Key_Clicked);
        }

        public void PlaySound_Key_Unlocked()
        {
            if (!thisTemplateCanPlaySounds) return;

            AudioManager.Instance.PlaySoundEffect(soundsEffect_Key_Unlocked);
        }

        #endregion

        #region Skill Sounds

        public void PlaySound_Skill_Hammer_Smash()
        {
            if (!thisTemplateCanPlaySounds) return;

            AudioManager.Instance.PlaySoundEffect(soundsEffect_Skill_Hammer_Smash);
        }

        #endregion

        #region Block Sounds

        public void PlaySound_Block_HitObstacle()
        {
            if (!thisTemplateCanPlaySounds) return;

            AudioManager.Instance.PlaySoundEffect(soundsEffect_Block_HitObstacle);
        }

        public void PlaySound_Block_DestroyedByObstacle()
        {
            if (!thisTemplateCanPlaySounds) return;

            AudioManager.Instance.PlaySoundEffect(soundsEffect_Block_DestroyedByObstacle);
        }

        public void PlaySound_Block_Cleared()
        {
            if (!thisTemplateCanPlaySounds) return;

            AudioManager.Instance.PlaySoundEffect(soundsEffect_Block_Cleared);
        }

        public void PlaySound_Block_Tapped()
        {
            if (!thisTemplateCanPlaySounds) return;

            AudioManager.Instance.PlaySoundEffect(soundsEffect_Block_Tapped);
        }

        #endregion

        #region Screw Jam Sounds

        public void PlaySound_ScrewJam_ScrewOut()
        {
            if (!thisTemplateCanPlaySounds) return;

            AudioManager.Instance.PlaySoundEffect(soundsEffect_ScrewJam_ScrewOut);
        }

        public void PlaySound_ScrewJam_ScrewIn()
        {
            if (!thisTemplateCanPlaySounds) return;

            AudioManager.Instance.PlaySoundEffect(soundsEffect_ScrewJam_ScrewIn);
        }

        public void PlaySound_ScrewJam_ContainerFilled()
        {
            if (!thisTemplateCanPlaySounds) return;

            AudioManager.Instance.PlaySoundEffect(soundsEffect_ScrewJam_ContainerFilled);
        }

        public void PlaySound_ScrewJam_ContainerIn()
        {
            if (!thisTemplateCanPlaySounds) return;

            AudioManager.Instance.PlaySoundEffect(soundsEffect_ScrewJam_ContainerIn);
        }

        public void PlaySound_ScrewJam_ContainerOut()
        {
            if (!thisTemplateCanPlaySounds) return;

            AudioManager.Instance.PlaySoundEffect(soundsEffect_ScrewJam_ContainerOut);
        }

        #endregion

        #region Bottle Jam Sounds

        public void PlaySound_BottleJam_BoxJump()
        {
            if (!thisTemplateCanPlaySounds) return;

            AudioManager.Instance.PlaySoundEffect(soundsEffect_BottleJam_BoxJump);
        }

        public void PlaySound_BottleJam_BoxFilled()
        {
            if (!thisTemplateCanPlaySounds) return;

            AudioManager.Instance.PlaySoundEffect(soundsEffect_BottleJam_BoxFilled);
        }

        public void PlaySound_BottleJam_BottleJumpEnd()
        {
            if (!thisTemplateCanPlaySounds) return;

            AudioManager.Instance.PlaySoundEffect(soundsEffect_BottleJam_BottleJumpEnd);
        }

        #endregion
    }
}