using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HyperPuzzleEngine;

namespace HyperPuzzleEngine
{
    public class TutorialMovement : MonoBehaviour
    {
        public float disableAfterSecondsPassed = 10f;

        public bool isOnlyPlayingOnce = true;
        public Animation[] keyAnims;
        public List<char> keysNeedToBePressed;

        IEnumerator Start()
        {
            if (isOnlyPlayingOnce && PlayerPrefs.GetInt("MovementTutorialDone", 0) == 1)
                gameObject.SetActive(false);

            yield return new WaitForSeconds(disableAfterSecondsPassed);
            PlayerPrefs.SetInt("MovementTutorialDone", 1);
            gameObject.SetActive(false);
        }

        private void Update()
        {
            CheckUserInputs();
        }

        private void CheckUserInputs()
        {
            if (Input.GetKeyDown(KeyCode.W))
                PressedKey('W');
            if (Input.GetKeyDown(KeyCode.A))
                PressedKey('A');
            if (Input.GetKeyDown(KeyCode.S))
                PressedKey('S');
            if (Input.GetKeyDown(KeyCode.D))
                PressedKey('D');

            if (Input.GetKeyDown(KeyCode.RightArrow))
                PressedKey('R');
            if (Input.GetKeyDown(KeyCode.LeftArrow))
                PressedKey('L');
            if (Input.GetKeyDown(KeyCode.UpArrow))
                PressedKey('U');
            if (Input.GetKeyDown(KeyCode.DownArrow))
                PressedKey('B');
        }

        private void PressedKey(char pressedCharacter)
        {
            for (int i = 0; i < keyAnims.Length; i++)
            {
                if (keyAnims[i].gameObject.name.Contains(pressedCharacter))
                {
                    if (keysNeedToBePressed.Contains(pressedCharacter))
                    {
                        keyAnims[i].Play();
                        keysNeedToBePressed.Remove(pressedCharacter);
                    }
                    break;
                }
            }

            if (keysNeedToBePressed.Count <= 0)
            {
                PlayerPrefs.SetInt("MovementTutorialDone", 1);
                gameObject.SetActive(false);
            }
        }
    }
}