using HyperPuzzleEngine;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

namespace HyperPuzzleEngine
{
    public class ContainersManager : MonoBehaviour
    {
        public Transform containersHolderFinalPositions;
        public Transform movementOfContainerStartsFromHere;
        public float containerMovementDurationInMiddle = 0.65f; // Duration of movement in seconds
        public float containerMovementDurationOutOfGame = 0.4f; // Duration of movement in seconds

        [Space]
        public ContainerHolesHolder nonColoredHoles;

        private List<ContainerHolesHolder> containers = new List<ContainerHolesHolder>();
        private HashSet<Transform> reservedFinalPositions = new HashSet<Transform>(); // Keeps track of reserved positions

        void Start()
        {
            InitializeContainers();
            Invoke(nameof(MoveNextContainerToFinalPos), 1f); // Start by attempting to move the next container to its final position
        }

        // This function initializes the containers list by finding all ContainerHolesHolder components in children
        private void InitializeContainers()
        {
            containers.AddRange(transform.GetComponentsInChildren<ContainerHolesHolder>());
        }

        // This function returns the transform of the next empty hole in the container with the specified color
        public Transform ReserveNextEmptyHoleByColor(Color color, bool canSearchForNonColoredHoleAsWell = true)
        {
            #region Colored Holes

            foreach (var container in containers)
            {
                // If the container has reached the final position
                if (container.transform.parent != null && container.transform.parent.parent == containersHolderFinalPositions)
                {
                    // If the container's color matches the input color and it has an empty hole, return the hole
                    if (container.GetContainerColor() == color)
                    {
                        Transform emptyHole = container.GetNextEmptyHole();
                        if (emptyHole != null)
                        {
                            container.ReserveNextEmptyHole();

                            GetComponentInParent<ShowcaseParent>().GetComponentInChildren<ScrewsLeftCounter>().DecreaseCounter();

                            return emptyHole;
                        }
                    }
                }
            }

            #endregion

            #region Non Colored Holes

            if (!canSearchForNonColoredHoleAsWell) return null;

            Transform emptyNonColoredHole = nonColoredHoles.GetNextEmptyHole();
            if (emptyNonColoredHole != null)
            {
                nonColoredHoles.ReserveNextEmptyHole();
                Invoke(nameof(CheckIfAllNonColoredHolesAreFilled), 0.55f);
                return emptyNonColoredHole;
            }

            #endregion

            return null; // No empty hole found in any container with the specified color
        }

        void CheckIfAllNonColoredHolesAreFilled()
        {
            Transform emptyNonColoredHole = nonColoredHoles.GetNextEmptyHole();
            if (emptyNonColoredHole == null)
                GetComponentInParent<LevelManager>().ActivateLevelFailedPanel();
        }

        // This function un-reserves a hole in the container with the specified color
        public bool UnReserveHoleByColor(Color color, Transform hole)
        {
            foreach (var container in containers)
            {
                Renderer containerRenderer = container.GetComponent<Renderer>();
                if (containerRenderer != null && containerRenderer.material.color == color)
                {
                    return container.UnReserveHole(hole);
                }
            }

            return false; // No container with the specified color was found, or failed to un-reserve
        }

        void TryToGetPiecesFromNonColoredHolesHolder()
        {
            foreach (ColorManager colorManager in nonColoredHoles.GetComponentsInChildren<ColorManager>())
            {
                if (colorManager != null)
                {
                    Transform piece = colorManager.transform;
                    Color pieceColor = colorManager.GetColor();
                    Transform reservedHole = ReserveNextEmptyHoleByColor(pieceColor, false);
                    if (reservedHole != null)
                    {
                        piece.GetComponent<ScrewForJam>().UnscrewFromNonColoredHole(reservedHole);
                        Debug.Log($"Moved piece {piece.name} to reserved hole {reservedHole.name}");
                    }
                }
            }
        }

        public void CheckIfContainerIsFull(Transform latestAddedScrew)
        {
            foreach (ContainerHolesHolder container in containers)
            {
                if (container.DoesHaveScrew(latestAddedScrew))
                {
                    if (container.GetNextEmptyHole() == null) // No empty holes left
                    {
                        Debug.Log($"Container {container.name} is full. Moving out of game.");
                        MoveOutOfGame(container.transform);
                    }
                    return;
                }
            }
        }

        #region Move Containers

        public void MoveNextContainerToFinalPos()
        {
            Debug.Log("ContainerCheck_No more containers to move.");

            if (transform.childCount == 0)
            {
                Debug.Log("ContainerCheck_No more containers to move.");
                return; // No children left to move
            }

            Transform nextContainer = transform.GetChild(transform.childCount - 1);
            Transform targetPosition = GetAvailableFinalPosition();

            if (targetPosition == null)
            {
                Debug.Log("ContainerCheck_No available final positions to move the container to.");
                return; // No available positions to move to
            }

            // Reserve the position for this container
            reservedFinalPositions.Add(targetPosition);

            Debug.Log("ContainerCheck_ Reserved Final Pos");

            // Start the coroutine to move the container to the final position
            StartCoroutine(MoveContainer(nextContainer, targetPosition));
        }

        // Find an available final position among the children of containersHolderFinalPositions
        private Transform GetAvailableFinalPosition()
        {
            for (int i = containersHolderFinalPositions.childCount - 1; i >= 0; i--)
            {
                Transform potentialPosition = containersHolderFinalPositions.transform.GetChild(i);
                if (potentialPosition.childCount == 0 && !reservedFinalPositions.Contains(potentialPosition))
                {
                    return potentialPosition; // Found an available position
                }
            }

            return null; // No available positions
        }

        // Coroutine to move the container smoothly from the start position to the target position
        private IEnumerator MoveContainer(Transform container, Transform targetPosition)
        {
            GetComponentInParent<SoundsManagerForTemplate>().PlaySound_ScrewJam_ContainerIn();

            container.SetParent(null); // Unparent the container before moving
            container.position = movementOfContainerStartsFromHere.position; // Set initial position to the start point

            Vector3 startPosition = movementOfContainerStartsFromHere.position;
            Vector3 endPosition = targetPosition.position;

            float elapsedTime = 0f;

            while (elapsedTime < containerMovementDurationInMiddle)
            {
                float t = elapsedTime / containerMovementDurationInMiddle;
                t = t * t * (3f - 2f * t); // Smoothstep interpolation for smoother movement
                container.position = Vector3.Lerp(startPosition, endPosition, t);
                elapsedTime += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            container.position = endPosition; // Ensure the final position is exactly at the target
            container.SetParent(targetPosition); // Set the container as a child of the target position

            Debug.Log($"Container {container.name} moved to final position {targetPosition.name}");

            MoveNextContainerToFinalPos();
            TryToGetPiecesFromNonColoredHolesHolder();
        }

        public void MoveOutOfGame(Transform container)
        {
            if (container.parent == null) return;

            reservedFinalPositions.Remove(container.parent); // Free up the reserved position
            container.transform.SetParent(null);

            GetComponentInParent<SoundsManagerForTemplate>().PlaySound_ScrewJam_ContainerOut();

            StartCoroutine(MoveOutCoroutine(container));
            GetComponentInParent<CollectedStacksCounter>().IncreaseCollectedPieces(1, container.gameObject.GetInstanceID());
            //GetComponentInParent<CollectedPiecesCounter>().IncreaseCollectedCounter();

            MoveNextContainerToFinalPos();
        }

        private IEnumerator MoveOutCoroutine(Transform container)
        {
            //container.SetParent(null); // Unparent the container

            Vector3 startPosition = container.position;
            Vector3 targetPosition = startPosition + new Vector3(0f, 0f, 3f); // Move 3 units further along the Z axis

            float elapsedTime = 0f;

            while (elapsedTime < containerMovementDurationOutOfGame)
            {
                float t = elapsedTime / containerMovementDurationOutOfGame;
                t = t * t * (3f - 2f * t); // Smoothstep interpolation for smoother movement
                container.position = Vector3.Lerp(startPosition, targetPosition, t);
                elapsedTime += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            GetComponentInParent<SoundsManagerForTemplate>().PlaySound_ScrewJam_ContainerFilled();

            container.position = targetPosition; // Ensure the final position is exactly at the target
            Debug.Log($"Container {container.name} moved out of game and destroyed.");

            containers.Remove(container.GetComponent<ContainerHolesHolder>());
            Destroy(container.gameObject); // Destroy the container after moving it out of the game
        }

        #endregion
    }
}