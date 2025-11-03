using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using HyperPuzzleEngine;
using Unity.VisualScripting;
using UnityEngine.TerrainUtils;

namespace HyperPuzzleEngine
{
    public class CheckNeighbours : MonoBehaviour
    {
        CheckNeighboursQueue queueManager;
        ShowcaseParent showcaseParent;

        #region Variables

        [Header("Is This a Slices/Pies Stacking Game")]
        public bool isCheckingForSlices = false;

        [Space]
        [Header("Grid Properties")]
        public bool canMatchStackOnThisGrid = true;
        public bool canHighlightThisGrid = false;

        [Space]
        [Header("Max Count Of Elements")]
        public int maxCountOfStackChildren = 999;
        public int maxCountOfStackChildrenIfHasLockedSlots = 999;

        [Space]
        [Header("Spacing Stack Elements")]
        public int leaveEmptyStack = 0;
        [SerializeField] private Vector3 stackDirection = Vector3.up;
        [SerializeField] private float distanceOfStackedObjects = 0.104f;

        private HighlightMatchingStack currentHighlighter;
        private SphereCaster sphereCaster;
        private Color thisColor;
        private Color latestMatchedColor;
        private ParabolicJump latestJump = null;
        private float timeBetweenStackedObjectFly = 0.05f;
        private float timeBetweenChecks = 0.5f;

        [Space]
        [Header("Matching Stacks")]
        public int countOfStacksNeededToMatch = 11;
        public float timeBetweenMatchingRemoves = 0.05f;
        public GameObject matchParticlePrefab;
        public UnityEvent OnRemovedMatchingStackElements;

        [Space]
        [Header("New Element (Spawns This After a Match")]
        public GameObject spawnNewElementPrefab;
        public Vector3 rotateNewElementAfterSpawn;
        private List<LockInStack> lockableStack = new List<LockInStack>();

        #endregion

        private void Start()
        {
            sphereCaster = GetComponent<SphereCaster>();
            currentHighlighter = GetComponent<HighlightMatchingStack>();
            showcaseParent = GetComponentInParent<ShowcaseParent>();
            queueManager = showcaseParent.GetComponentInChildren<CheckNeighboursQueue>();

            Debug.Log("CALLED ON START");
        }

        bool isMatching = false;

        public bool IsMatchingStacks() { return isMatching; }

        #region Slices

        Color[] GetSliceColors(Transform parentToCheck)
        {
            if (parentToCheck == null) return new Color[0];

            Debug.Log("Grid_" + "GetSliceColors");

            List<Color> foundColors = new List<Color>();

            foreach (ColorManager colorManager in parentToCheck.GetComponentsInChildren<ColorManager>(false))
            {
                if (!foundColors.Contains(colorManager.GetColor()))
                    foundColors.Add(colorManager.GetColor());
            }

            Debug.Log("Grid_" + "FoundColors: " + foundColors.Count);

            return foundColors.ToArray();
        }

        List<Color> GetSliceColorsList(Transform parentToCheck)
        {
            Debug.Log("Grid_" + "GetSliceColors");

            if (parentToCheck == null) return new List<Color>();


            List<Color> foundColors = new List<Color>();

            foreach (ColorManager colorManager in parentToCheck.GetComponentsInChildren<ColorManager>(false))
            {
                if (!foundColors.Contains(colorManager.GetColor()))
                    foundColors.Add(colorManager.GetColor());
            }

            Debug.Log("Grid_" + "FoundColors: " + foundColors.Count);

            return foundColors;
        }

        public bool CanGetMorePies()
        {
            Debug.Log("Grid_" + "CanGetMorePies");

            return transform.GetChild(0).GetComponentsInChildren<ColorManager>(false).Length < maxCountOfStackChildren;
        }

        public bool HasSlices()
        {
            return transform.GetComponentsInChildren<Slice>(false).Length > 0;
        }

        private Transform GetSliceWithColor(Transform slicesHolder, Color matchingColor)
        {
            foreach (ColorManager colorManager in slicesHolder.GetComponentsInChildren<ColorManager>())
            {
                //if (colorManager.GetColor() == matchingColor)
                if (AreColorsSimilar(colorManager.GetColor(), matchingColor))
                {
                    Debug.Log("Grid_FOUND GECI: " + colorManager.gameObject.name);
                    return colorManager.transform;
                }
            }
            return null;
        }

        private Slice GetEmptySlice(Transform slicesHolder)
        {
            foreach (Slice slice in slicesHolder.GetComponentsInChildren<Slice>(true))
            {
                if (!slice.gameObject.activeInHierarchy)
                {
                    slice.transform.parent.gameObject.SetActive(true);
                    return slice;
                }
            }

            return null;
        }

        public void StartCheckMatchingSlicesInStack(bool isForcedCheck = false)
        {
            Debug.Log("Grid_" + "StartCheckMatchingSlicesInStack");

            if (GetComponentInChildren<SlicesOrderNextToEachOther>() != null)
                GetComponentInChildren<SlicesOrderNextToEachOther>().TryToOrderSlices();

            Transform slicesHolder = transform.GetChild(0).GetChild(0);
            if (GetSliceColors(slicesHolder).Length != 1) return;

            if (canHighlightThisGrid && currentHighlighter != null)
                currentHighlighter.Highlight(false);

            if (!canMatchStackOnThisGrid && !isForcedCheck && !canHighlightThisGrid) return;

            if (transform.childCount <= 0) return;

            if (GetComponentsInChildren<Slice>(false).Length < countOfStacksNeededToMatch) return;

            Vector3 sliceLocalPos = GetComponentInChildren<Slice>(true).transform.localPosition;

            foreach (Slice slice in GetComponentsInChildren<Slice>(true))
            {
                if (slice.transform.localPosition != sliceLocalPos)
                    return;
            }

            TryToDisableHighlight();

            latestMatchedColor = transform.GetChild(0).GetComponentInChildren<ColorManager>().GetColor();

            foreach (ColorManager sliceColor in transform.GetChild(0).GetComponentsInChildren<ColorManager>(true))
                Destroy(sliceColor);

            StartCoroutine(RemoveMatchingStackElementsSlices(slicesHolder));
        }

        IEnumerator RemoveMatchingStackElementsSlices(Transform slicesHolder, int lastIndex = 0)
        {
            isMatching = true;

            Debug.Log("Grid_" + "RemoveMatchingStackElementsSlices");

            TryToSpawnCompletedEffect();

            if (GetComponentInParent<SoundsManagerForTemplate>() != null)
                GetComponentInParent<SoundsManagerForTemplate>().PlaySound_Slices_Filled();

            yield return new WaitForSeconds(0.4f);

            if (canHighlightThisGrid && currentHighlighter != null)
                currentHighlighter.Highlight(false);

            List<Transform> matching = new List<Transform>();

            for (int i = slicesHolder.childCount - 1; i >= lastIndex; i--)
                matching.Add(slicesHolder.GetChild(i));

            for (int i = 0; i < matching.Count; i++)
            {
                if (matching[i] != null)
                {
                    IncreaseCollectedCount_Piece();
                    matching[i].gameObject.AddComponent<ScaleDownAndDestroy>();
                    if (showcaseParent.GetComponentInChildren<CollectedPiecesCounter>() != null)
                        showcaseParent.GetComponentInChildren<CollectedPiecesCounter>().IncreaseCollectedCounter(1, true);

                    if (GetComponentInParent<SoundsManagerForTemplate>() != null)
                        GetComponentInParent<SoundsManagerForTemplate>().PlaySound_Stack_Match();
                }

                if (i == matching.Count - 1)
                    OnRemovedMatchingStackElements.Invoke();

                yield return new WaitForSeconds(0.05f);
            }

            slicesHolder.GetComponentInParent<CheckNeighbours>().transform.GetChild(0).gameObject.AddComponent<ScaleDownAndDestroy>();

            isMatching = false;

            if (GetComponentInParent<CheckForMoreTargetPositions>() != null)
                GetComponentInParent<CheckForMoreTargetPositions>().CheckIfAnyTargetIsFree();
        }

        public bool IsThereActiveSlice(Transform slicesHolder)
        {
            for (int i = 0; i < slicesHolder.childCount; i++)
            {
                if (slicesHolder.GetChild(i).gameObject.activeInHierarchy)
                    return true;
            }

            return false;
        }

        #endregion

        public List<int> GetTopChildPoints(Transform parentToCheck)
        {
            List<int> emptyList;
            emptyList = new List<int>();
            emptyList.Add(-1);

            if (parentToCheck.childCount == 0)
                return emptyList;

            if (parentToCheck.GetChild(parentToCheck.childCount - 1).gameObject.name.ToLower().Contains("occupied") ||
                parentToCheck.GetComponentInChildren<UnlockableGrid>() != null)
                return emptyList;

            DominoPointsManager pointsManager = parentToCheck.GetChild(parentToCheck.childCount - 1).GetComponent<DominoPointsManager>();

            if (pointsManager != null)
                return pointsManager.GetAllMatchablePoints();
            //return pointsManager.GetMatchablePoint();
            else
                return emptyList;
        }

        public Color GetTopChildColor(Transform parentToCheck)
        {
            Debug.Log("Grid_" + "GetTopChildColor");

            if (parentToCheck.childCount == 0)
                return Color.white;

            if (parentToCheck.GetChild(parentToCheck.childCount - 1).gameObject.name.ToLower().Contains("occupied") ||
                parentToCheck.GetComponentInChildren<UnlockableGrid>() != null)
                return Color.white;

            MeshRenderer rendererOfTopChild = parentToCheck.GetChild(parentToCheck.childCount - 1).GetComponent<MeshRenderer>();

            if (rendererOfTopChild != null && rendererOfTopChild.material != null)
                return rendererOfTopChild.material.color;
            else
                return Color.white;
        }
        Color GetSecondFromTopChildColor(Transform parentToCheck)
        {
            Debug.Log("Grid_" + "GetSecondFromTopChildColor");

            Color firstColor = Color.white;
            Color secondColor = Color.white;

            if (parentToCheck.childCount == 0)
                return Color.white;

            if (parentToCheck.GetChild(parentToCheck.childCount - 1).gameObject.name.ToLower().Contains("occupied") ||
                parentToCheck.GetComponentInChildren<UnlockableGrid>() != null)
                return Color.white;

            MeshRenderer rendererOfTopChild = parentToCheck.GetChild(parentToCheck.childCount - 1).GetComponent<MeshRenderer>();

            if (rendererOfTopChild != null && rendererOfTopChild.material != null)
                firstColor = rendererOfTopChild.material.color;
            else
                return Color.white;

            MeshRenderer rendererOfSecondTopChild;

            if (transform.childCount >= 2)
            {
                for (int i = parentToCheck.childCount - 2; i >= 0; i--)
                {
                    rendererOfSecondTopChild = parentToCheck.GetChild(i).GetComponent<MeshRenderer>();

                    if (rendererOfSecondTopChild.material.color != firstColor)
                    {
                        return rendererOfSecondTopChild.material.color;
                    }
                }
            }

            return Color.white;
        }

        public bool CanGetMoreStack()
        {
            Debug.Log("Grid_" + "CanGetMoreStack");

            return transform.childCount < maxCountOfStackChildren;
        }

        public Vector3 GetNextPosition()
        {
            if (transform.childCount > 1)
                return transform.GetChild(0).position + (stackDirection * distanceOfStackedObjects * (transform.childCount - 1 + leaveEmptyStack));
            else
                return transform.position + (stackDirection * distanceOfStackedObjects * (transform.childCount + leaveEmptyStack));
        }

        public void StartCheckNeighbourColorsCoroutine(float startDelay = 0f, bool isQueued = false)
        {
            Debug.Log("Grid_" + "StartCheckNeighbourColorsCoroutine");

            StartCoroutine(CheckNeighbourColors(startDelay, isQueued));
        }

        public IEnumerator CheckNeighbourColors(float delay = 0f, bool isQueued = false)
        {
            yield return new WaitForSeconds(delay);

            Debug.Log("Grid_" + "CheckNeighbourColors");

            if (!isQueued)
            {
                if (queueManager != null)
                    queueManager.AddToQueue(this);
            }
            else
            {
                //Really checking this time

                GameObject[] collidingObjects = sphereCaster.CastSphere(transform.position);

                for (int i = 0; i < collidingObjects.Length; i++)
                {
                    yield return CheckIfTopColorsMatch(collidingObjects[i]);
                }

                if (queueManager != null)
                    queueManager.RemoveFromQueue(this);
            }
        }

        void OnDestroy()
        {
            if (queueManager != null)
                queueManager.RemoveFromQueue(this);
        }

        void OnDisable()
        {
            if (queueManager != null)
                queueManager.RemoveFromQueue(this);
        }

        IEnumerator CheckIfTopColorsMatch(GameObject hit)
        {
            Debug.Log("Grid_" + "CheckIfTopColorsMatch");

            bool hasAtLeastOneMatch = false;

            GameObject startGrid = null;
            GameObject targetGrid = null;

            #region Decide Which Stack Jumps

            bool thisStackHasMoreColors = false;
            bool otherStackHasMoreColors = false;

            Color[] thisStackColors;
            Transform slicesHolder = null;
            if (transform.childCount > 0 && transform.GetComponentInChildren<ColorManager>() != null && isCheckingForSlices && isCheckingForSlices && transform.GetChild(0).childCount > 0)
                slicesHolder = transform.GetChild(0).GetChild(0);
            Transform otherSlicesHolder = null;
            if (hit.transform.childCount > 0 && hit.transform.GetComponentInChildren<ColorManager>() != null && isCheckingForSlices && hit.transform.GetChild(0).childCount > 0)
                otherSlicesHolder = hit.transform.GetChild(0).GetChild(0);

            Color thisTopColor;
            Color thisSecondTopColor;

            if (isCheckingForSlices)
            {
                //Get all colors of this stack (slices)

                thisStackColors = GetSliceColors(slicesHolder);
                if (thisStackColors.Length > 1)
                    thisStackHasMoreColors = true;
            }
            else
            {
                //Check if this stack has more colors

                thisTopColor = GetTopChildColor(transform);
                thisSecondTopColor = GetSecondFromTopChildColor(transform);
                if (thisTopColor != thisSecondTopColor && thisTopColor != Color.white && thisSecondTopColor != Color.white)
                    thisStackHasMoreColors = true;
            }

            if (!thisStackHasMoreColors)
            {
                if (isCheckingForSlices)
                {
                    //Check if other stack has more colors

                    Color[] otherStackColors = GetSliceColors(otherSlicesHolder);
                    if (otherStackColors.Length > 1)
                        otherStackHasMoreColors = true;

                    #region Deciding Jumping Stack Logic

                    if (!otherStackHasMoreColors)
                    {
                        //If none of the 2 stacks have more colors, then jumping is decided randomly
                        Debug.Log("DECIDE_NONE HAS MORE COLORS");

                        if (Random.Range(0, 2) == 0)
                        {
                            //Other stack jumps
                            startGrid = hit;
                            targetGrid = gameObject;
                        }
                        else
                        {
                            //This stack jumps
                            startGrid = gameObject;
                            targetGrid = hit;
                        }
                    }
                    else
                    {
                        //If other stack has 2 different colors, then other stack jumps to this stack
                        Debug.Log("DECIDE_2 HAS MORE COLORS");

                        startGrid = hit;
                        targetGrid = gameObject;
                    }

                    #endregion
                }
                else
                {
                    //Check if other stack has more colors

                    Color otherTopColor = GetTopChildColor(hit.transform);
                    Color otherSecondTopColor = GetSecondFromTopChildColor(hit.transform);
                    if (otherTopColor != otherSecondTopColor && otherTopColor != Color.white && otherSecondTopColor != Color.white)
                        otherStackHasMoreColors = true;

                    #region Deciding Jumping Stack Logic

                    if (!otherStackHasMoreColors)
                    {
                        //If none of the 2 stacks have more colors, then jumping is decided randomly

                        if (Random.Range(0, 2) == 0)
                        {
                            //Other stack jumps
                            startGrid = hit;
                            targetGrid = gameObject;
                        }
                        else
                        {
                            //This stack jumps
                            startGrid = gameObject;
                            targetGrid = hit;
                        }
                    }
                    else
                    {
                        //If other stack has 2 different colors, then other stack jumps to the other stack

                        startGrid = hit;
                        targetGrid = gameObject;
                    }

                    #endregion
                }
            }
            else
            {
                //If this stack has 2 different colors, then this stack jumps to the other stack
                Debug.Log("DECIDE_1 HAS MORE COLORS");

                startGrid = gameObject;
                targetGrid = hit;
            }

            #endregion

            if (isCheckingForSlices && otherSlicesHolder != null && slicesHolder != null)
            {
                Transform startGridSlicesHolder = startGrid.transform.GetChild(0).GetChild(0);
                Transform targetGridSlicesHolder = targetGridSlicesHolder = targetGrid.transform.GetChild(0).GetChild(0);

                List<Color> thisColorsList;
                List<Color> hitColorsList;

                do
                {
                    thisColorsList = GetSliceColorsList(targetGridSlicesHolder);
                    hitColorsList = GetSliceColorsList(startGridSlicesHolder);

                    if (IsThereActiveSlice(startGridSlicesHolder))
                    {
                        bool thereIsMatch = false;
                        Color matchingColor = Color.white;

                        for (int i = 0; i < hitColorsList.Count; i++)
                        {
                            if (thisColorsList.Contains(hitColorsList[i]))
                            {
                                matchingColor = hitColorsList[i];
                                thereIsMatch = true;
                                break;
                            }
                        }

                        if (thereIsMatch)
                        {
                            Transform newChild = GetSliceWithColor(startGridSlicesHolder, matchingColor);

                            if (targetGrid.GetComponent<CheckNeighbours>().CanGetMorePies())
                            {
                                Debug.Log("Has At Least One Match.");
                                hasAtLeastOneMatch = true;

                                GameObject newMovingSlice = GetEmptySlice(targetGridSlicesHolder).gameObject;
                                newMovingSlice.GetComponent<ColorManager>().ChangeColor(matchingColor);

                                if (newMovingSlice.GetComponent<Slice>() != null)
                                {
                                    if (newMovingSlice.GetComponent<Slice>() != null && newChild != null)
                                    {
                                        newMovingSlice.GetComponent<Slice>().MoveSlice(newChild, newChild.transform.position, newChild.transform.rotation);
                                    }
                                }
                            }
                            else
                            {
                                break;
                            }

                            yield return new WaitForSeconds(timeBetweenStackedObjectFly);
                        }
                        else
                            break;
                    }
                } while (IsThereActiveSlice(startGridSlicesHolder));
            }
            else if (!isCheckingForSlices)
            {
                do
                {
                    if (startGrid.transform.childCount > 0)
                    {
                        thisColor = GetTopChildColor(targetGrid.transform);
                        Color hitColor = GetTopChildColor(startGrid.transform);

                        Debug.Log("Colors: " + thisColor + "___" + hitColor);

                        if (thisColor == Color.white || hitColor == Color.white)
                            break;

                        //if (hitColor != thisColor)
                        //{
                        //    //Check If The Colors are Only Slightly Different
                        //    float colorDifferenceThreshold = 0.05f;

                        //    float diffR = Mathf.Abs(hitColor.r - thisColor.r);
                        //    float diffG = Mathf.Abs(hitColor.g - thisColor.g);
                        //    float diffB = Mathf.Abs(hitColor.b - thisColor.b);

                        //    if (diffR <= colorDifferenceThreshold
                        //        && diffG <= colorDifferenceThreshold
                        //        && diffB <= colorDifferenceThreshold)
                        //    {
                        //        Debug.Log("Colors Difference Is Very Low! They Are Similar");

                        //        hitColor = thisColor;
                        //    }
                        //}

                        //if (hitColor == thisColor)
                        if (AreColorsSimilar(hitColor, thisColor))
                        {
                            Transform newChild = startGrid.transform.GetChild(startGrid.transform.childCount - 1);

                            if (targetGrid.GetComponent<CheckNeighbours>() != null &&
                                targetGrid.GetComponent<CheckNeighbours>().CanGetMoreStack())
                            {
                                Debug.Log("Has At Least One Match.");
                                hasAtLeastOneMatch = true;

                                if (newChild.GetComponent<ParabolicJump>() != null)
                                    newChild.GetComponent<ParabolicJump>().parentAtStartOfJump = newChild.parent;
                                newChild.parent = targetGrid.transform;

                                Vector3 targetPosition = targetGrid.transform.GetChild(0).position/*localPosition*/ + (Vector3.up * distanceOfStackedObjects * (targetGrid.transform.childCount - 1));

                                if (newChild.GetComponent<ParabolicJump>() != null)
                                    newChild.GetComponent<ParabolicJump>().SimpleJump(targetPosition, false);
                            }
                            else
                            {
                                //Fly Back
                                if (newChild.GetComponent<SelectableAfterPlaced>() != null)
                                    newChild.GetComponent<SelectableAfterPlaced>().ChangeLevitation(Vector3.one * 444f);
                            }

                            yield return new WaitForSeconds(timeBetweenStackedObjectFly);
                        }
                        else
                            break;
                    }
                } while (startGrid.transform.childCount > 0);
            }

            if (hasAtLeastOneMatch)
                yield return new WaitForSeconds(timeBetweenChecks);
        }

        public void StartCheckMatchingColorsInStack(bool isForcedCheck = false)
        {
            Debug.Log("Grid_" + "StartCheckMatchingColorsInStack");

            if (canHighlightThisGrid && currentHighlighter != null)
                currentHighlighter.Highlight(false);

            if (!canMatchStackOnThisGrid && !isForcedCheck && !canHighlightThisGrid) return;

            if (transform.childCount <= 0) return;

            if (transform.GetChild(transform.childCount - 1).GetComponent<ColorManager>() == null) return;

            int countOfSameColorsConsequently = 0;

            Color tempColor = Color.white;
            tempColor = GetTopChildColor(transform);

            TryToDisableHighlight();

            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                if (transform.GetChild(i).GetComponent<ColorManager>() != null &&
                    AreColorsSimilar(tempColor, transform.GetChild(i).GetComponent<ColorManager>().GetColor()))
                //(tempColor == transform.GetChild(i).GetComponent<ColorManager>().GetColor()))
                {
                    countOfSameColorsConsequently++;
                }
                else
                {
                    if (countOfSameColorsConsequently >= countOfStacksNeededToMatch)
                    {
                        if (canHighlightThisGrid && currentHighlighter != null)
                            currentHighlighter.Highlight(true);

                        if (!canMatchStackOnThisGrid && !isForcedCheck) return;

                        StartCoroutine(RemoveMatchingStackElements(i));
                        break;
                    }

                    if (transform.GetChild(i).GetComponent<ColorManager>() != null)
                        tempColor = transform.GetChild(i).GetComponent<ColorManager>().GetColor();
                    countOfSameColorsConsequently = 0;
                }

                if (i == 0 && countOfSameColorsConsequently >= countOfStacksNeededToMatch)
                {
                    if (canHighlightThisGrid && currentHighlighter != null)
                        currentHighlighter.Highlight(true);

                    if (!canMatchStackOnThisGrid && !isForcedCheck) return;

                    StartCoroutine(RemoveMatchingStackElements(i));
                    break;
                }
            }
        }

        private bool AreColorsSimilar(Color color1, Color color2)
        {
            if (color1 == color2)
                return true;

            if (color1 != color2)
            {
                //Check If The Colors are Only Slightly Different
                float colorDifferenceThreshold = 0.05f;

                float diffR = Mathf.Abs(color1.r - color2.r);
                float diffG = Mathf.Abs(color1.g - color2.g);
                float diffB = Mathf.Abs(color1.b - thisColor.b);

                if (diffR <= colorDifferenceThreshold
                    && diffG <= colorDifferenceThreshold
                    && diffB <= colorDifferenceThreshold)
                {
                    Debug.Log("Colors Difference Is Very Low! They Are Similar");

                    return true;
                }
            }

            return false;
        }

        public void TryToDisableHighlight()
        {
            Debug.Log("Grid_" + "TryToDisableHighlight");

            if (canHighlightThisGrid && (transform.childCount < countOfStacksNeededToMatch))
                currentHighlighter.Highlight(false);
        }

        public void StartRemovingAllPiecesInStack()
        {
            StartCoroutine(RemoveMatchingStackElements(0, true));
        }

        IEnumerator RemoveMatchingStackElements(int lastIndex, bool removeRegardlessOfColor = false)
        {
            isMatching = true;

            Debug.Log("Grid_" + "RemoveMatchingStackElements");

            if (canHighlightThisGrid && currentHighlighter != null)
                currentHighlighter.Highlight(false);

            List<Transform> matching = new List<Transform>();

            Color tempMatchingColor = transform.GetChild(transform.childCount - 1).GetComponent<ColorManager>().GetColor();

            for (int i = transform.childCount - 1; i >= lastIndex; i--)
            {
                //if ((transform.GetChild(i).GetComponent<ColorManager>().GetColor() == tempMatchingColor)
                if (AreColorsSimilar(transform.GetChild(i).GetComponent<ColorManager>().GetColor(), tempMatchingColor)
                    || removeRegardlessOfColor)
                    matching.Add(transform.GetChild(i));
                else
                    break;
            }


            for (int i = matching.Count - 1; i >= lastIndex; i--)
            {
                if (matching[i].GetComponent<LockInStack>() == null)
                {
                    if (GetComponentInParent<Rotate>() != null)
                        matching[i].parent = GetComponentInParent<Rotate>().transform;
                    else
                        matching[i].parent = null;
                }
            }


            for (int i = 0; i < matching.Count; i++)
            {
                if (matching[i] != null)
                {
                    latestMatchedColor = matching[i].GetComponent<ColorManager>().GetColor();

                    if (matching[i].GetComponent<LockInStack>() == null)
                    {
                        matching[i].gameObject.AddComponent<ScaleDownAndDestroy>();
                        IncreaseCollectedCount_Piece();
                    }
                    else
                    {
                        lockableStack.Add(matching[i].GetComponent<LockInStack>());
                    }

                    if (GetComponentInParent<SoundsManagerForTemplate>() != null)
                        GetComponentInParent<SoundsManagerForTemplate>().PlaySound_Stack_Match();

                    TryToSpawnMatchParticle(matching[i].position, matching[i].GetComponent<ColorManager>().GetColor());

                    if (showcaseParent.GetComponentInChildren<CollectedPiecesCounter>() != null)
                        showcaseParent.GetComponentInChildren<CollectedPiecesCounter>().IncreaseCollectedCounter(1, true);
                }

                if (i == matching.Count - 1)
                    OnRemovedMatchingStackElements.Invoke();

                yield return new WaitForSeconds(timeBetweenMatchingRemoves);
            }


            foreach (CheckNeighbours check in transform.parent.GetComponentsInChildren<CheckNeighbours>())
                check.StartCheckNeighbourColorsCoroutine();

            foreach (ProgressBar stackProgressBar in showcaseParent.GetComponentsInChildren<ProgressBar>())
                stackProgressBar.CheckProgress();

            isMatching = false;

            if (GetComponentInParent<CheckForMoreTargetPositions>() != null)
                GetComponentInParent<CheckForMoreTargetPositions>().CheckIfAnyTargetIsFree();
        }

        private void TryToSpawnMatchParticle(Vector3 positionToSpawn, Color particleColor)
        {
            if (matchParticlePrefab != null)
            {
                GameObject currentParticle = Instantiate(matchParticlePrefab, positionToSpawn, Quaternion.identity);
                currentParticle.GetComponentInChildren<ParticleSystemRenderer>().material.color = particleColor;
                Destroy(currentParticle, 1f);
            }
        }

        private void TryToRespawnActiveStackContainers()
        {
            showcaseParent.GetComponentInChildren<ActiveStackContainersRandomManualRespawn>().TryToRespawn();
        }

        public void TryToSpawnCompletedEffect()
        {
            if (GetComponentInChildren<MatchCompletedEffectSpawner>() != null)
            {
                foreach (MatchCompletedEffectSpawner effectSpawner in GetComponentsInChildren<MatchCompletedEffectSpawner>())
                {
                    effectSpawner.SpawnEffect();
                }
            }
        }

        public void SpawnNewFirstStackElement()
        {
            if (showcaseParent.GetComponentInChildren<ActiveStackContainersRandomManualRespawn>() != null)
            {
                Debug.Log("Grid_" + "SpawnNewFirstStackElement_V222");

                GameObject[] newSpawnedElements;

                Color newColor = GetComponentInParent<UnlockedColors>().GetNextColor(latestMatchedColor);

                showcaseParent.GetComponentInChildren<ActiveStackContainersRandomManualRespawn>().RespawnAtPosition(transform, 4, newColor);

                Invoke(nameof(TryToRespawnActiveStackContainers), 1f);
            }
            else
            {
                Debug.Log("Grid_" + "SpawnNewFirstStackElement");

                GameObject newSpawnedElement = Instantiate(spawnNewElementPrefab, transform.position, Quaternion.identity);
                newSpawnedElement.transform.parent = transform;
                newSpawnedElement.transform.Rotate(rotateNewElementAfterSpawn);

                if (GetComponent<GridChildrenAreSelectable>() != null)
                    newSpawnedElement.transform.position = GetComponent<GridChildrenAreSelectable>().GetNextEmptyPos() + newSpawnedElement.GetComponent<ParabolicJump>().targetPositionOffset;
                else
                    newSpawnedElement.transform.position = transform.GetChild(0).position/*localPosition*/ + (Vector3.up * distanceOfStackedObjects * (transform.childCount - 1)) + newSpawnedElement.GetComponent<ParabolicJump>().targetPositionOffset;

                Color newColor = GetComponentInParent<UnlockedColors>().GetNextColor(latestMatchedColor);
                newSpawnedElement.GetComponent<ColorManager>().ChangeColor(newColor);

                if (showcaseParent.GetComponentInChildren<DealStackButton>() != null)
                {
                    if (showcaseParent.GetComponentInChildren<DealStackButton>().canSelectStackOnceDealt)
                        newSpawnedElement.AddComponent<SelectableAfterPlaced>();
                }

                if (newSpawnedElement.GetComponent<ColorManager>() != null)
                    newSpawnedElement.GetComponent<ColorManager>().SetUpNumberInStackBasedOnColor(newColor);
            }
        }

        public void TryToUnlockNextColor()
        {
            Debug.Log("Grid_" + "TryToUnlockNextColor");
        }

        public void LockInStacks()
        {
            Debug.Log("LOCKING_V0_" + gameObject.name);

            foreach (LockInStack lockable in lockableStack)
            {
                Debug.Log("LOCKING_V1_" + lockable.name);
                lockable.LockIn();

                if (lockable.transform.parent != transform)
                    lockable.transform.parent = transform;
            }
        }

        public void DisableThis()
        {
            GetComponent<Collider>().enabled = false;
            this.enabled = false;
        }

        public void IncreaseCollectedCount_Stack()
        {
            if (GetComponentInParent<CollectedStacksCounter>() != null)
                GetComponentInParent<CollectedStacksCounter>().IncreaseCollectedStacks();
        }

        public void IncreaseCollectedCount_Piece()
        {
            if (GetComponentInParent<CollectedStacksCounter>() != null)
                GetComponentInParent<CollectedStacksCounter>().IncreaseCollectedPieces();
        }

        public void CheckIfChildrenAreAtCorrectPosition()
        {
            Debug.Log("_POS_Checking..");

            if (GetComponent<GridChildrenAreSelectable>() == null)
                return;

            for (int i = 0; i < transform.childCount; i++)
            {
                Debug.Log("_POS_Checking Child: " + i);

                Transform tempChild = transform.GetChild(i);

                if (tempChild.GetComponentInChildren<ColorManager>() == null)
                    continue;

                Debug.Log("_POS_Actual Child With ColorManager: " + i);

                Vector3 correctPos = GetComponent<GridChildrenAreSelectable>().GetPos(i + 1);

                if (tempChild.position != correctPos)
                    tempChild.GetComponentInChildren<ParabolicJump>().SimpleMove(correctPos, false);
            }
        }
    }
}