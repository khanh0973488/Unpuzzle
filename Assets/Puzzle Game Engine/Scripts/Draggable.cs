using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using HyperPuzzleEngine;

namespace HyperPuzzleEngine
{
    public class Draggable : MonoBehaviour, IDragHandler, IEndDragHandler
    {
        public Vector3 dragOffset;

        [Space]
        [Header("Drag Properties")]
        public bool cannotDragWhileCheckingNeighbours = false;
        public bool cannotDragWhileStackPieceIsJumping = false;
        public bool onlyCanDragIfHasChildren = false;
        public bool disableMeshRendererOnDrag = false;
        public bool destroyAfterDragFinished = true;
        public bool snapBackToStartPosAfterDragFinished = false;
        public bool isSnappingToGrid = false;
        public bool startMovingAtMouseClick = true;
        public bool canPlaceAnywhere = false;

        [Space]
        [SerializeField] private float movementSpeed = 5f; // Speed at which the object moves back to its original position

        private Vector3 originalPosition;
        private Vector3 mousePosition;
        private bool isMovingBack = false;

        private Camera mainCam;

        [Space]
        public bool isHeightRelativeToStartPos = true;
        public float fixedHeight = 1f;


        private SphereCaster sphereCaster;
        private GameObject tempCollidingGrid = null;

        [Space]
        [Header("Grid Highlight")]
        public Color gridHighlightedColor;
        public GameObject gridHighlightPrefab = null;
        GameObject tempGridHighlightObj = null;
        public Vector3 gridHighlightPlacementOffset;

        private bool canDrag = true;
        private ActiveStackContainersRespawn tempSpawnerParent;
        private CheckNeighboursQueue neighboursCheckingQueue;

        private CheckForMoreTargetPositions checkForMoreTargets;

        void Start()
        {
            neighboursCheckingQueue = GetComponentInParent<ShowcaseParent>().GetComponentInChildren<CheckNeighboursQueue>();
            checkForMoreTargets = GetComponentInParent<CheckForMoreTargetPositions>();

            mainCam = Camera.main;

            sphereCaster = GetComponent<SphereCaster>();

            // Save the original position of the object
            originalPosition = transform.position;
            tempSpawnerParent = GetComponentInParent<ActiveStackContainersRespawn>();

            if (gridHighlightPrefab != null)
            {
                tempGridHighlightObj = Instantiate(gridHighlightPrefab, null);
                tempGridHighlightObj.SetActive(false);
            }
        }

        private void Update()
        {
            if (Input.GetMouseButtonUp(0))
                HighlightGrid(false);
        }

        Vector3 GetMousePos()
        {
            return mainCam.WorldToScreenPoint(transform.position);
        }

        private void HighlightGrid(bool isSettingOn = true)
        {
            if (isSettingOn)
            {
                if (GetComponent<MeshRenderer>() != null && GetComponent<MeshRenderer>().material.color == gridHighlightedColor) return;

                GameObject showcaseParent = GetComponentInParent<ShowcaseParent>().gameObject;
                foreach (Draggable draggable in showcaseParent.GetComponentsInChildren<Draggable>())
                {
                    if (draggable != this)
                        draggable.HighlightGrid(false);
                }

                if (tempCollidingGrid != null && tempCollidingGrid.GetComponent<CheckNeighbours>() != null && tempCollidingGrid.GetComponent<ColorManager>() != null)
                    tempCollidingGrid.GetComponent<ColorManager>().ChangeColor(gridHighlightedColor);
                if (gridHighlightPrefab != null && tempCollidingGrid != null)
                {
                    tempGridHighlightObj.transform.position = tempCollidingGrid.transform.position;
                    tempGridHighlightObj.SetActive(true);
                    tempGridHighlightObj.transform.position += gridHighlightPlacementOffset;
                }
            }
            else
            {
                GameObject showcaseParent = GetComponentInParent<ShowcaseParent>().gameObject;
                foreach (CheckNeighbours grid in showcaseParent.GetComponentsInChildren<CheckNeighbours>())
                {
                    grid.GetComponent<ColorManager>().ResetColorToDefault();
                }

                if (gridHighlightPrefab != null && tempGridHighlightObj != null)
                {
                    tempGridHighlightObj.SetActive(false);
                }
            }
        }

        #region Input Handling (Click and Drag Events)

        private void OnMouseDown()
        {
            if (!canDrag) return;

            if (onlyCanDragIfHasChildren && transform.childCount < 1) return;

            if (cannotDragWhileCheckingNeighbours && neighboursCheckingQueue.GetQueue().Count > 0) return;

            if (cannotDragWhileStackPieceIsJumping && GetComponentInParent<ShowcaseParent>().IsAnyStackJumping()) return;

            mousePosition = Input.mousePosition - GetMousePos();

            if (GetComponentInParent<SoundsManagerForTemplate>() != null)
                GetComponentInParent<SoundsManagerForTemplate>().PlaySound_Stack_Selected();

            if (startMovingAtMouseClick)
                FollowCursor();
        }

        private void OnMouseUp()
        {
            if (!canDrag) return;

            if (onlyCanDragIfHasChildren && transform.childCount < 1) return;

            if (cannotDragWhileCheckingNeighbours && neighboursCheckingQueue.GetQueue().Count > 0) return;

            if (cannotDragWhileStackPieceIsJumping && GetComponentInParent<ShowcaseParent>().IsAnyStackJumping()) return;

            if (!canPlaceAnywhere)
            {
                if (tempCollidingGrid != null && tempCollidingGrid.GetComponent<CheckNeighbours>() != null)
                {
                    tempCollidingGrid.GetComponent<ColorManager>().ResetColorToDefault();

                    StartCoroutine(MoveToPosition(tempCollidingGrid.GetComponent<CheckNeighbours>().GetNextPosition()));
                }
                else
                    StartCoroutine(MoveBackToOriginalPosition());
            }
            else if (canPlaceAnywhere)
            {
                StartCoroutine(MoveToPosition(transform.position - (fixedHeight * Vector3.up)));
            }

            if (GetComponentInParent<SoundsManagerForTemplate>() != null)
                GetComponentInParent<SoundsManagerForTemplate>().PlaySound_Stack_Moved();

            HighlightGrid(false);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!canDrag) return;

            if (onlyCanDragIfHasChildren && transform.childCount < 1) return;

            if (isMovingBack) return; // Don't drag if it's moving back to original position

            if (cannotDragWhileCheckingNeighbours && neighboursCheckingQueue.GetQueue().Count > 0) return;

            if (cannotDragWhileStackPieceIsJumping && GetComponentInParent<ShowcaseParent>().IsAnyStackJumping()) return;


            if (disableMeshRendererOnDrag)
                GetComponent<MeshRenderer>().enabled = false;

            FollowCursor();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!canDrag) return;

            if (tempCollidingGrid != null && tempCollidingGrid.GetComponent<CheckNeighbours>() != null)
            {
                tempCollidingGrid.GetComponent<ColorManager>().ResetColorToDefault();
                StartCoroutine(MoveToPosition(tempCollidingGrid.GetComponent<CheckNeighbours>().GetNextPosition()));
            }
            else
                StartCoroutine(MoveBackToOriginalPosition());
        }

        #endregion

        #region Finding Closest Object

        private GameObject FindClosestObject(Transform obj1, GameObject[] objectsToCheck)
        {
            GameObject tempClosestObject = null;
            float tempClosestDistance = Mathf.Infinity;

            foreach (GameObject obj in objectsToCheck)
            {
                float tempDistance = Vector3.Distance(obj1.position, obj.transform.position);

                if (tempDistance < tempClosestDistance)
                {
                    tempClosestDistance = tempDistance;
                    tempClosestObject = obj;
                }
            }

            return tempClosestObject;
        }

        private GameObject FindClosestObject(Vector3 pos1, GameObject[] objectsToCheck)
        {
            GameObject tempClosestObject = null;
            float tempClosestDistance = Mathf.Infinity;

            foreach (GameObject obj in objectsToCheck)
            {
                if (obj.transform.childCount > 0) continue;
                if (obj.GetComponentInChildren<UnlockableGrid>(true) != null) continue;

                float tempDistance = Vector3.Distance(pos1, obj.transform.position);

                if (tempDistance < tempClosestDistance)
                {
                    tempClosestDistance = tempDistance;
                    tempClosestObject = obj;
                }
            }

            return tempClosestObject;
        }

        #endregion

        #region Movements

        GameObject latestCollidingGrid = null;

        void FollowCursor()
        {
            //Way1
            Vector3 newPos = mainCam.ScreenToWorldPoint(Input.mousePosition - mousePosition);

            //Way2: New Pos will be set to a hit point from main camera's raycast on layer 7. at mouse position
            RaycastHit hit;
            //Vector3 offsetCorrectionForCursorPos = new Vector3(0f, 0f, -1f);
            if (Physics.Raycast(mainCam.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, 1 << 7))
            {
                newPos = hit.point;
                newPos += dragOffset;
            }

            if (isHeightRelativeToStartPos)
                newPos = new Vector3(newPos.x, originalPosition.y + fixedHeight, newPos.z);
            else
                newPos = new Vector3(newPos.x, fixedHeight, newPos.z);

            if (Vector3.Distance(transform.position, newPos) > 0.02f)
            {
                if (transform.position != newPos)
                {
                    GameObject[] collidingGrids = sphereCaster.CastSphere(newPos);

                    if (collidingGrids.Length > 0)
                    {
                        // OnGridCollision
                        tempCollidingGrid = FindClosestObject(newPos, collidingGrids);

                        if (tempCollidingGrid != null && tempCollidingGrid != latestCollidingGrid)
                        {
                            //foreach (GameObject grid in collidingGrids)
                            //{
                            //    if (grid.GetComponent<ColorManager>() != null)
                            //        grid.GetComponent<ColorManager>().ResetColorToDefault();
                            //}

                            HighlightGrid(true);
                        }

                        latestCollidingGrid = tempCollidingGrid;
                    }
                    else
                    {
                        if (tempCollidingGrid != null && tempCollidingGrid.GetComponent<CheckNeighbours>() != null)
                        {
                            // OnLostGridCollision
                            tempCollidingGrid.GetComponent<ColorManager>().ResetColorToDefault();
                            tempCollidingGrid = null;
                        }
                    }
                }
            }

            if (isSnappingToGrid && tempCollidingGrid != null)
            {
                Vector3 newPos2 = tempCollidingGrid.transform.position;
                transform.position = newPos2;

            }
            else
            {
                transform.position = newPos;
            }
        }

        private IEnumerator MoveBackToOriginalPosition()
        {
            isMovingBack = true;

            // While the object is not at the original position, keep moving towards it
            while (transform.position != originalPosition)
            {
                transform.position = Vector3.MoveTowards(transform.position, originalPosition, movementSpeed * Time.deltaTime);
                yield return null; // Wait for the next frame
            }

            if (disableMeshRendererOnDrag)
                GetComponent<MeshRenderer>().enabled = true;

            isMovingBack = false; // Now the object can be dragged again
        }

        private IEnumerator MoveToPosition(Vector3 targetPos)
        {
            isMovingBack = true;

            // While the object is not at the original position, keep moving towards it
            while (transform.position != targetPos)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, movementSpeed * Time.deltaTime);
                yield return null; // Wait for the next frame
            }

            isMovingBack = false; // Now the object can be dragged again

            //OnMovedToGrid
            if (tempCollidingGrid != null && tempCollidingGrid.GetComponent<CheckNeighbours>() != null)
            {
                canDrag = false;
                if (transform.childCount > 0 || GetComponent<ColorManager>() == null)
                {
                    List<Transform> children = new List<Transform>();
                    for (int i = 0; i < transform.childCount; i++)
                        children.Add(transform.GetChild(i));

                    for (int i = 0; i < children.Count; i++)
                        children[i].transform.parent = tempCollidingGrid.transform;

                    tempCollidingGrid.GetComponent<CheckNeighbours>().StartCheckNeighbourColorsCoroutine();

                    if (!snapBackToStartPosAfterDragFinished)
                        transform.parent = null;

                    if (tempSpawnerParent != null)
                        tempSpawnerParent.TryToRespawn();

                    if (destroyAfterDragFinished)
                        Destroy(gameObject);

                    if (snapBackToStartPosAfterDragFinished)
                    {
                        canDrag = true;
                        transform.position = originalPosition;
                    }
                }
                else
                {
                    transform.parent = tempCollidingGrid.transform;
                    tempCollidingGrid.GetComponent<CheckNeighbours>().StartCheckNeighbourColorsCoroutine();
                }
            }

            if (disableMeshRendererOnDrag)
                GetComponent<MeshRenderer>().enabled = true;

            if (checkForMoreTargets != null)
                checkForMoreTargets.CheckIfAnyTargetIsFree();
        }

        #endregion
    }
}
