using TMPro;
using UnityEngine;
using UnityEngine.Events;
using HyperPuzzleEngine;

namespace HyperPuzzleEngine
{
    public class PhysicalButton : MonoBehaviour
    {
        public UnityEvent OnButtonPressed;

        private Animation anim;
        public bool onlyHighlightInLevelCreatorMode = false;
        public Color highlightColor;

        [Space]
        [Header("Value")]
        public bool needValueToPress;
        public int countOfValueAtStart = 3;
        int currentValue = 3;
        public TextMeshPro valueText;
        public UnityEvent OnValueRanOut;
        public UnityEvent OnRightMousePressed;

        [Space]
        [Header("UI Instead of Physical Button")]
        public bool isUsingDealUI = false;
        public GameObject dealUI;

        private ShowcaseParent showcaseParent;

        private void Start()
        {
            showcaseParent = GetComponentInParent<ShowcaseParent>();
            currentValue = countOfValueAtStart;
            if (valueText != null) valueText.text = currentValue.ToString();

            ChooseDealButtonAppearance();

            anim = GetComponent<Animation>();
        }

        private void ChooseDealButtonAppearance()
        {
            if (dealUI != null)
                dealUI.SetActive(isUsingDealUI);

            foreach (MeshRenderer renderer in transform.GetComponentsInChildren<MeshRenderer>())
                renderer.enabled = !isUsingDealUI;
            foreach (TextMeshProUGUI textUI in transform.parent.GetComponentsInChildren<TextMeshProUGUI>())
                textUI.enabled = !isUsingDealUI;
            foreach (TextMeshPro text in transform.parent.GetComponentsInChildren<TextMeshPro>())
                text.enabled = !isUsingDealUI;
            foreach (SpriteRenderer sprite in transform.parent.GetComponentsInChildren<SpriteRenderer>())
                sprite.enabled = !isUsingDealUI;

            GetComponent<Collider>().enabled = !isUsingDealUI;
        }

        #region Mouse Input Events

        private void OnMouseEnter()
        {
            if (IsPlacingAnObject(transform)) return;

            if (GetComponent<ColorManager>() != null)
            {
                Debug.Log("Mouse Over Element");

                GetComponent<ColorManager>().SetCurrentColorAsDefault();
                if (onlyHighlightInLevelCreatorMode && !showcaseParent.IsInGameMode())
                    GetComponent<ColorManager>().ChangeColor(highlightColor);
                else if (!onlyHighlightInLevelCreatorMode)
                    GetComponent<ColorManager>().ChangeColor(highlightColor);
            }
        }

        private bool IsPlacingAnObject(Transform target)
        {
            if (target.GetComponentInParent<ShowcaseParent>() == null)
                return false;

            if (target.GetComponentInParent<ShowcaseParent>().GetComponentInChildren<PlaceSelectedObject>() != null)
            {
                if (target.GetComponentInParent<ShowcaseParent>().GetComponentInChildren<PlaceSelectedObject>().IsPlacingObject())
                    return true;
            }

            return false;
        }

        private void Update()
        {
            if (Input.GetMouseButtonUp(1) && GetComponent<ColorManager>() != null && (GetComponent<ColorManager>().GetColor() == highlightColor))
            {
                if (IsPlacingAnObject(transform)) return;

                if (GetComponent<ColorManager>() != null)
                    GetComponent<ColorManager>().ResetColorToDefault();
                Debug.Log("PressedRightMouse");
                OnRightMousePressed.Invoke();
            }
        }

        private void OnMouseExit()
        {
            if (IsPlacingAnObject(transform)) return;

            if (GetComponent<ColorManager>() != null)
                GetComponent<ColorManager>().ResetColorToDefault();
        }

        private void OnMouseDown()
        {
            if (IsPlacingAnObject(transform)) return;

            PressButton();
        }

        #endregion

        public void PressButton()
        {
            Debug.Log("Pressed Physical Button..");

            if (anim != null)
                anim.Play();

            if (needValueToPress)
            {
                if (currentValue > 0)
                {
                    currentValue--;
                    if (valueText != null) valueText.text = currentValue.ToString();

                    if (currentValue <= 0)
                        OnValueRanOut.Invoke();

                    OnButtonPressed.Invoke();
                }
            }
            else
                OnButtonPressed.Invoke();
        }

        public void ActivateClickedOnPopup()
        {
            ClickedOptionsPopupManager.Instance.ClickedOnObject(transform);
        }
    }
}