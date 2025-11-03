using HyperPuzzleEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperPuzzleEngine
{
    public class SetMaterialAtStart : MonoBehaviour
{
    public static int materialIndexAtStart = 0;

    public Material[] materialsList;

    void Awake()
    {
        materialIndexAtStart = 0;

        if (GetComponent<ColorManager>() == null)
            GetComponent<MeshRenderer>().material = materialsList[materialIndexAtStart];
    }

    public Material GetCurrentMaterial()
    {
        return materialsList[materialIndexAtStart];
    }
}
}