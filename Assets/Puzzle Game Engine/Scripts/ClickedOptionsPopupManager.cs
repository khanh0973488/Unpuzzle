using HyperPuzzleEngine;
using TMPro;
using UnityEngine;

namespace HyperPuzzleEngine
{
    public class ClickedOptionsPopupManager : MonoBehaviour
    {
        #region Create Instance

        public static ClickedOptionsPopupManager Instance;

        private void Awake()
        {
            Instance = this;
        }

        #endregion

        [Header("Which Type To Use")]
        public ClickedOptionsPopupManager popUpHexagon;
        public ClickedOptionsPopupManager popUpNut;
        public ClickedOptionsPopupManager popUpScrew;

        [Space]
        [Header("Positioning")]
        public Vector3 distanceOffsetFromTargetDown;
        public Vector3 distanceOffsetFromTargetUp;

        private Transform movableChild;
        private Transform currentTarget;

        bool canClickAgain = true;

        [Space]
        [Header("Buttons To Activate Or Deactivate")]
        public GameObject[] buttonsToActivateOnlyIfLastChildIsSelected;
        public GameObject[] buttonsToActivateOnlyIfNotLastChildIsSelected;

        private void EnableClickingAgain()
        {
            canClickAgain = true;
        }

        public void ClickedOnObject(Transform target)
        {
            if (IsPlacingAnObject(target))
                return;

            if (target.GetComponentInParent<ShowcaseParent>().gameObject.name.ToLower().Contains("hexa"))
            {
                if (this != popUpHexagon)
                {
                    popUpHexagon.ClickedOnObject(target);
                    return;
                }
            }
            else if (target.GetComponentInParent<ShowcaseParent>().gameObject.name.ToLower().Contains("nut"))
            {
                if (this != popUpNut)
                {
                    popUpNut.ClickedOnObject(target);
                    return;
                }
            }
            else if (target.GetComponentInParent<ShowcaseParent>().gameObject.name.ToLower().Contains("jam") ||
                    target.GetComponentInParent<ShowcaseParent>().gameObject.name.ToLower().Contains("screw"))
            {
                if (this != popUpScrew)
                {
                    popUpScrew.ClickedOnObject(target);
                    return;
                }
            }

            if (!canClickAgain) return;
            canClickAgain = false;

            currentTarget = target;

            if (GetComponentInChildren<TMP_InputField>() != null)
            {
                GetComponentInChildren<TMP_InputField>().text = "5";
                GetComponentInChildren<TMP_InputField>().transform.parent.gameObject.SetActive(false);
            }

            transform.localScale = Vector3.one;

            Invoke(nameof(EnableClickingAgain), 0.2f);

            movableChild.localPosition = Vector3.zero;

            if (target.position.z > target.GetComponentInParent<ShowcaseParent>().transform.position.z)
                movableChild.transform.position -= distanceOffsetFromTargetDown;
            else
                movableChild.transform.position += distanceOffsetFromTargetUp;

            transform.position = target.position;

            CheckButtonsToActivate();
        }

        public void HidePopup()
        {
            transform.position = new Vector3(0f, -50f, 0f);
        }

        void Start()
        {
            movableChild = transform.GetChild(0).GetChild(0);
        }

        #region Pressed Buttons

        public void PressedButton_ShowColor()
        {
            if (currentTarget == null) return;
            HideColor currentHideColor = currentTarget.GetComponent<HideColor>();
            if (currentHideColor == null) return;

            currentHideColor.isHidingColor = false;
            currentHideColor.UpdateColorHider();

            HidePopup();
        }

        public void PressedButton_HideColor()
        {
            if (currentTarget == null) return;
            HideColor currentHideColor = currentTarget.GetComponent<HideColor>();
            if (currentHideColor == null) return;

            currentHideColor.isHidingColor = true;
            currentHideColor.UpdateColorHider();

            HidePopup();
        }

        public void PressedButton_Occupied()
        {
            PressedButton_Free();

            if (currentTarget == null) return;
            OccupiedGrid currentOccupiedGrid = currentTarget.GetComponent<OccupiedGrid>();
            if (currentOccupiedGrid == null) return;

            currentOccupiedGrid.lockType = OccupiedGrid.LockType.Occupied;
            currentOccupiedGrid.setGridOccupied = true;
        }

        public void PressedButton_Lock()
        {
            PressedButton_Free();

            if (currentTarget == null) return;
            OccupiedGrid currentOccupiedGrid = currentTarget.GetComponent<OccupiedGrid>();
            if (currentOccupiedGrid == null) return;

            currentOccupiedGrid.lockType = OccupiedGrid.LockType.Locked;
            currentOccupiedGrid.setGridOccupied = true;
        }

        public void PressedButton_Free()
        {
            if (currentTarget == null) return;
            OccupiedGrid currentOccupiedGrid = currentTarget.GetComponent<OccupiedGrid>();
            if (currentOccupiedGrid == null) return;

            currentOccupiedGrid.setGridFree = true;
        }

        public void PressedButton_Delete()
        {
            if (currentTarget == null) return;
            Destroy(currentTarget.gameObject);

            HidePopup();
        }

        public void PressedButton_DeleteParent()
        {
            if (currentTarget == null) return;

            //SpaceChildrenEvenly tempTargetParentOrder = currentTarget.transform.GetComponentInParent<SpaceChildrenEvenly>();
            //tempTargetParentOrder.OrderChildren();
            GameObject objectToDestroy = currentTarget.GetComponentInParent<CheckNeighbours>().gameObject;

            //if (objectToDestroy.GetComponent<GridGenerator>() != null ||
            //   objectToDestroy.name.ToLower().Contains("page1"))
            //    objectToDestroy = currentTarget.parent.gameObject;

            objectToDestroy.transform.parent = null;
            Destroy(objectToDestroy);

            HidePopup();
        }

        public void UpdateLockedPriceOfTarget(TMP_InputField inputField)
        {
            if (currentTarget == null) return;

            int result = 5;
            if (!int.TryParse(inputField.text, out result))
                return;

            if (currentTarget == null) return;
            if (currentTarget.GetComponentInChildren<UnlockableGrid>() == null) return;
            currentTarget.GetComponentInChildren<UnlockableGrid>().ChangePrice(result);
        }

        #endregion

        private void CheckButtonsToActivate()
        {
            if (currentTarget == null) return;

            for (int i = 0; i < buttonsToActivateOnlyIfLastChildIsSelected.Length; i++)
                buttonsToActivateOnlyIfLastChildIsSelected[i].SetActive
                    (currentTarget.GetSiblingIndex() == (currentTarget.parent.childCount - 1));

            for (int i = 0; i < buttonsToActivateOnlyIfNotLastChildIsSelected.Length; i++)
                buttonsToActivateOnlyIfNotLastChildIsSelected[i].SetActive
                    (currentTarget.GetSiblingIndex() != (currentTarget.parent.childCount - 1));
        }

        private bool IsPlacingAnObject(Transform target)
        {
            if (target.GetComponentInParent<ShowcaseParent>().GetComponentInChildren<PlaceSelectedObject>() != null)
            {
                if (target.GetComponentInParent<ShowcaseParent>().GetComponentInChildren<PlaceSelectedObject>().IsPlacingObject())
                    return true;
            }

            return false;
        }
    }
}