using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using HyperPuzzleEngine;

namespace HyperPuzzleEngine
{
    public class DealStackButton : MonoBehaviour
    {
        public Vector3 selectableAfterPlacedLevitateAmountLocalOffset = new Vector3(0f, 1f, 0f);

        public bool startDealingFromStartPos = true;
        public bool canSelectStackOnceDealt = true;

        private ActiveStackContainersRespawn stackContainerHolder;

        private Animation anim;

        bool isDealing = false;

        [Space]
        [Header("Deal UI Instead of Physical Button")]
        public bool isUsingDealUI = false;
        public GameObject dealUI;

        private void Start()
        {
            stackContainerHolder = GetComponentInParent<ShowcaseParent>().GetComponentInChildren<ActiveStackContainersRespawn>();

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

            GetComponent<Collider>().enabled = !isUsingDealUI;
        }

        private void OnMouseDown()
        {
            TryToDeal();
        }

        public void TryToDeal()
        {
            Debug.Log("Trying to Deal..");

            if (GetComponentInParent<ShowcaseParent>().IsSelectableLevitating()) return;
            if (isDealing) return;

            anim.Play();

            StartCoroutine(DealCards());
        }

        IEnumerator DealCards()
        {
            isDealing = true;

            Debug.Log("DEAL_Starting To Deal Cards.");

            if (GetComponentInParent<SoundsManagerForTemplate>() != null)
                GetComponentInParent<SoundsManagerForTemplate>().PlaySound_Button_Pressed();

            yield return new WaitForSeconds(0.5f);

            GridChildrenAreSelectable[] activeGrids = GetComponentInParent<ShowcaseParent>().GetActiveGridsWhichCanBeDealtCardToo();

            Debug.Log("DEAL_There are " + activeGrids.Length + " Active Grids.");

            for (int i = 0; i < activeGrids.Length; i++)
            {
                Transform[] stackHolder = GetStack(i);

                for (int j = (stackHolder.Length) - (1); j >= 0; j--)
                {
                    Transform currentStack = stackHolder[j];

                    if (activeGrids[i].CanGetMoreStack(activeGrids[i].GetComponent<LockedSlotsWithKey>() != null))
                    {
                        currentStack.parent = activeGrids[i].transform;
                        if (startDealingFromStartPos)
                            currentStack.transform.position = transform.position + Vector3.up;
                        currentStack.GetComponentInChildren<ParabolicJump>().SimpleJump(activeGrids[i].GetNextEmptyPos(), false);

                        if (GetComponentInParent<SoundsManagerForTemplate>() != null)
                            GetComponentInParent<SoundsManagerForTemplate>().PlaySound_Button_Dealing();

                        if (canSelectStackOnceDealt)
                        {
                            if (currentStack.gameObject.GetComponent<ColorManager>() != null)
                            {
                                if (currentStack.gameObject.GetComponent<SelectableAfterPlaced>() == null)
                                {
                                    currentStack.gameObject.AddComponent<SelectableAfterPlaced>();
                                    currentStack.gameObject.GetComponent<SelectableAfterPlaced>().levitateAmountLocalOffset = selectableAfterPlacedLevitateAmountLocalOffset;
                                }
                            }
                            else
                            {
                                if (currentStack.GetChild(0).gameObject.GetComponent<SelectableAfterPlaced>() == null)
                                {
                                    currentStack.GetChild(0).gameObject.AddComponent<SelectableAfterPlaced>();
                                    currentStack.GetChild(0).gameObject.GetComponent<SelectableAfterPlaced>().levitateAmountLocalOffset = selectableAfterPlacedLevitateAmountLocalOffset;
                                }
                            }
                        }

                    }

                    yield return new WaitForSeconds(0.05f);
                }

                stackContainerHolder.GetComponentsInChildren<StackContainerRandomSpawn>()[i].spawn = true;
                yield return new WaitForSeconds(0.1f);
            }

            Debug.Log("DEAL_Finished Dealing Cards.");
            isDealing = false;
        }

        private Transform[] GetStack(int index)
        {
            List<Transform> stack = new List<Transform>();

            Transform stackHolder = stackContainerHolder.GetComponentsInChildren<StackContainerRandomSpawn>()[index].transform;

            for (int i = 0; i < stackHolder.childCount; i++)
            {
                stack.Add(stackHolder.GetChild(i).transform);
            }

            Debug.Log("DEAL_Returning " + stack.Count + " Stack.");

            return stack.ToArray();
        }

        public bool IsDealingCards()
        {
            return isDealing;
        }
    }
}
