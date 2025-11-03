using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HyperPuzzleEngine;
using UnityEngine.Events;
using System.Linq.Expressions;

namespace HyperPuzzleEngine
{
    public class PagesSystem : MonoBehaviour
    {
        private int currentPageIndex = 0;

        [Header("Righ and Left Arrow Buttons")]
        public GameObject previousArrowButton;
        public GameObject nextArrowButton;

        [Space]
        [Header("Page Loaded Events")]
        public UnityEvent OnPage1Loaded;
        public UnityEvent OnPage2Loaded;
        public UnityEvent OnPage3Loaded;
        public UnityEvent OnPage4Loaded;

        public int GetCurrentPageIndex()
        {
            return currentPageIndex;
        }

        private void Start()
        {
            LoadPage(currentPageIndex);
        }

        public void PreviousPage()
        {
            currentPageIndex--;

            LoadPage(currentPageIndex);
        }

        public void NextPage()
        {
            currentPageIndex++;

            LoadPage(currentPageIndex);
        }

        private void LoadPage(int pageIndex)
        {
            previousArrowButton.SetActive(pageIndex != 0);
            nextArrowButton.SetActive(pageIndex != transform.childCount - 1);

            int currentlyOpenedPageIndex = 0;

            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).gameObject.activeInHierarchy)
                {
                    currentlyOpenedPageIndex = i;
                    break;
                }
            }

            if (currentlyOpenedPageIndex != pageIndex)
            {
                if (GetComponent<Animation>() != null)
                    GetComponent<Animation>().Play();
                else
                    DisablePreviousPageAndLoadNewOne();
            }
        }

        public void DisablePreviousPageAndLoadNewOne()
        {
            for (int i = 0; i < transform.childCount; i++)
                transform.GetChild(i).gameObject.SetActive(false);

            transform.GetChild(currentPageIndex).gameObject.SetActive(true);

            switch (currentPageIndex)
            {
                case 0:
                    OnPage1Loaded.Invoke();
                    break;
                case 1:
                    OnPage2Loaded.Invoke();
                    break;
                case 2:
                    OnPage3Loaded.Invoke();
                    break;
                case 3:
                    OnPage4Loaded.Invoke();
                    break;
            }
        }
    }
}