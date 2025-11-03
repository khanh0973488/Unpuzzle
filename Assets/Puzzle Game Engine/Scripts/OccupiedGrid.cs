using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using HyperPuzzleEngine;

namespace HyperPuzzleEngine
{
    [ExecuteAlways]
    public class OccupiedGrid : MonoBehaviour
    {
        public Vector3 occupiedOffset;
        public bool setGridOccupied = false;
        public bool setGridFree = false;

        public enum LockType
        {
            Locked,
            Occupied
        }
        [Space]
        [Header("Lock Type")]
        public LockType lockType = LockType.Occupied;
        public GameObject lockedGridPrefab;
        public GameObject occupiedGridPrefab;

        [Space]
        [SerializeField] private bool canDealToThisGrid = true;

        private void Update()
        {
            if (setGridFree)
            {
                setGridFree = false;

                GameObject occupiedChild = GetOccupiedChildGrid();

                if (occupiedChild != null)
                    DestroyImmediate(occupiedChild);
            }
            else if (setGridOccupied)
            {
                setGridOccupied = false;

                GameObject occupiedChild = GetOccupiedChildGrid();

                if (occupiedChild == null)
                {
                    if (lockType == LockType.Locked)
                    {
                        occupiedChild = Instantiate(lockedGridPrefab, transform);

                        foreach (Collider collider in occupiedChild.GetComponentsInChildren<Collider>())
                        {
                            if (GetComponentInParent<GridGenerator>() != null)
                                collider.enabled = GetComponentInParent<GridGenerator>().isLockedGridClickable;
                        }
                    }
                    else if (lockType == LockType.Occupied)
                        occupiedChild = Instantiate(occupiedGridPrefab, transform);

                    occupiedChild.transform.localPosition = Vector3.zero;
                    occupiedChild.transform.localScale = Vector3.one;
                    occupiedChild.transform.localRotation = Quaternion.identity;
                    occupiedChild.transform.position += occupiedOffset;


                    if (GetComponent<CreateNewVisualAtStart>() != null)
                    {
                        foreach (MeshRenderer renderer in occupiedChild.GetComponentsInChildren<MeshRenderer>())
                            renderer.enabled = false;
                        foreach (TextMeshProUGUI text in occupiedChild.GetComponentsInChildren<TextMeshProUGUI>())
                            text.enabled = false;
                        foreach (TextMeshPro text in occupiedChild.GetComponentsInChildren<TextMeshPro>())
                            text.enabled = false;
                        foreach (Image image in occupiedChild.GetComponentsInChildren<Image>())
                            image.enabled = false;
                        foreach (SpriteRenderer sprite in occupiedChild.GetComponentsInChildren<SpriteRenderer>())
                            sprite.enabled = false;
                        foreach (Collider collider in occupiedChild.GetComponentsInChildren<Collider>())
                            collider.enabled = false;
                    }
                }
            }
        }

        private GameObject GetOccupiedChildGrid()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).gameObject.name.ToLower().Contains(occupiedGridPrefab.name.ToLower()) ||
                    transform.GetChild(i).gameObject.name.ToLower().Contains(lockedGridPrefab.name.ToLower()) ||
                    transform.GetChild(i).gameObject.name.ToLower().Contains("occupied") ||
                    transform.GetChild(i).gameObject.name.ToLower().Contains("locked"))
                {
                    return transform.GetChild(i).gameObject;
                }
            }
            return null;
        }

        public bool IsOccupied()
        {
            if (GetOccupiedChildGrid() == null)
                return false;
            else
                return true;
        }

        public bool CanDealCard()
        {
            return canDealToThisGrid;
        }

        public void UnlockThisGrid(GameObject buyableGrid)
        {
            if (buyableGrid != null)
                Destroy(buyableGrid);
            setGridFree = true;
        }

        public void ReverseGridOccupy()
        {
            if (IsOccupied())
            {
                setGridFree = true;
            }
            else
            {
                setGridOccupied = true;
            }
        }

        [Space]
        [Header("Overwrite Values")]
        public bool canOverwriteValuesByUI = true;
        public UnityEvent OnOverwriteValue;

        public void OverwriteValue_LockType(TMP_Dropdown dropdown)
        {
            if (!canOverwriteValuesByUI) return;

            lockType = (LockType)dropdown.value;

            OnOverwriteValue.Invoke();
        }
    }
}
