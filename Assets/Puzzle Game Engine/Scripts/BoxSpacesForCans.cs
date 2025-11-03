using HyperPuzzleEngine;
using System.Collections.Generic;
using UnityEngine;

namespace HyperPuzzleEngine
{
    public class BoxSpacesForCans : MonoBehaviour
    {
        public Transform canHolders; // Public transform for CanHolders
        public string boxFilledAnimName = "BoxFilledUpWithCansAnim";

        private List<Transform> canPosList = new List<Transform>(); // List to hold CanPos transforms
        private HashSet<Transform> occupiedPositions = new HashSet<Transform>(); // Set of occupied positions
        private AnimationPlayer animPlayer; // Reference to animator for BoxFilledUpAnim

        void Start()
        {
            // Get all the CanPos transforms from the hierarchy
            InitializeCanPositions();

            // Find the Animator component on this GameObject
            animPlayer = GetComponent<AnimationPlayer>();
        }

        void InitializeCanPositions()
        {
            if (canHolders == null)
            {
                Debug.LogError("CanHolders is not assigned!");
                return;
            }

            // Loop through each Row and gather all CanPos objects
            foreach (Transform row in canHolders)
            {
                foreach (Transform canPos in row)
                {
                    if (canPos != null && canPos.name.Contains("CanPos"))
                    {
                        canPosList.Add(canPos);
                    }
                }
            }
        }

        public Transform GetNextEmptyCanPos()
        {
            foreach (Transform canPos in canPosList)
            {
                // If this CanPos is not occupied
                if (!occupiedPositions.Contains(canPos))
                {
                    //occupiedPositions.Add(canPos); // Mark this CanPos as occupied
                    Debug.Log($"Returning CanPos: {canPos.name}");

                    // Check if this was the last position in the list
                    if (occupiedPositions.Count == canPosList.Count)
                    {
                        OnBoxPositionsFilled(); // Call method when all positions are filled
                    }

                    return canPos;
                }
            }

            // No empty CanPos available
            return null;
        }

        [HideInInspector] public bool isBeingFilled = false;

        private void OnBoxPositionsFilled()
        {
            isBeingFilled = true;

            Debug.Log("All positions filled. Playing BoxFilledUpAnim.");

            GetComponentInParent<SoundsManagerForTemplate>().PlaySound_BottleJam_BoxFilled();

            if (animPlayer != null)
            {
                animPlayer.PlayAnimation(boxFilledAnimName);
            }
            else
            {
                Debug.LogWarning("Animation Player component not found!");
            }

            GetComponentInParent<CollectedStacksCounter>().IncreaseCollectedPieces();
        }

        public void SetCanPosOccupied(Transform posToSetOccupied)
        {
            occupiedPositions.Add(posToSetOccupied); // Mark this CanPos as occupied

            // Check if this was the last position in the list
            if (occupiedPositions.Count == canPosList.Count)
            {
                OnBoxPositionsFilled(); // Call method when all positions are filled
            }
        }

        public void RemoveCurrentBoxFromFrontLine()
        {
            GetComponentInParent<BoxJumpController>().ReleaseSlot(transform.parent.GetSiblingIndex());
            transform.SetParent(null);
            Destroy(gameObject, 0.2f);
        }

        public void StopCheckingForLevelFailed()
        {
            GetComponentInParent<BoxJumpController>().StopCheckingForLevelFailed();
        }
    }
}