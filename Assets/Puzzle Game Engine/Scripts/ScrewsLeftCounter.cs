using HyperPuzzleEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace HyperPuzzleEngine
{
    public class ScrewsLeftCounter : MonoBehaviour
{
    public string prefix = "LEFT: ";

    TextMeshProUGUI counterText;
    ShowcaseParent showcaseParent;

    int countOfScrewsLeft;

    private void Start()
    {
        counterText = GetComponentInChildren<TextMeshProUGUI>();
        showcaseParent = GetComponentInParent<ShowcaseParent>();

        countOfScrewsLeft = showcaseParent.GetComponentsInChildren<ScrewForJam>().Length;
        counterText.text = prefix + countOfScrewsLeft.ToString();
    }

    public void DecreaseCounter()
    {
        countOfScrewsLeft--;
        counterText.text = prefix + countOfScrewsLeft.ToString();
    }
}
}