using HyperPuzzleEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperPuzzleEngine
{
    public class ShowRemoveIcons : MonoBehaviour
{
    List<NutMoveOnBolt> nuts = new List<NutMoveOnBolt>();
    List<BoltMover> bolts = new List<BoltMover>();

    public void ShowRemoveIconsForNuts()
    {
        nuts = new List<NutMoveOnBolt>();

        Camera cam = Camera.main;

        int i = 0;
        foreach (NutMoveOnBolt nut in GetComponentInParent<ShowcaseParent>().GetComponentsInChildren<NutMoveOnBolt>(false))
        {
            nuts.Add(nut);
            Vector3 posOfNutInCanvas = cam.WorldToScreenPoint(nut.transform.position + nut.transform.up * 0.2f);
            transform.GetChild(i).position = posOfNutInCanvas;
            transform.GetChild(i).gameObject.SetActive(true);
            i++;
        }
    }

    public void DisableAllRemoveIcons()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    public void DisableCurrentRemoveIcon(NutMoveOnBolt currentNut)
    {
        for (int i = 0; i < nuts.Count; i++)
        {
            if (currentNut == nuts[i])
            {
                transform.GetChild(i).gameObject.SetActive(false);
                break;
            }
        }
    }

    public void ShowRemoveIconsForBolts()
    {
        bolts = new List<BoltMover>();

        Camera cam = Camera.main;

        int i = 0;
        foreach (BoltMover bolt in GetComponentInParent<ShowcaseParent>().GetComponentsInChildren<BoltMover>(false))
        {
            bolts.Add(bolt);
            Vector3 posOfBoltInCanvas = cam.WorldToScreenPoint(bolt.transform.Find("BoltStart").position);
            transform.GetChild(i).position = posOfBoltInCanvas;
            transform.GetChild(i).gameObject.SetActive(true);
            i++;
        }
    }
}
}