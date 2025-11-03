using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperPuzzleEngine
{
    public class TrailColorChanger : MonoBehaviour
    {
        public Gradient[] colors;
        public Material[] materials;

        void Start()
        {
            Material targetMat = GetComponentInParent<MeshRenderer>().material;

            for (int i = 0; i < colors.Length; i++)
            {
                if (targetMat.name.Contains(materials[i].name))
                {
                    GetComponent<TrailRenderer>().colorGradient = colors[i];
                    break;
                }
            }
        }
    }
}