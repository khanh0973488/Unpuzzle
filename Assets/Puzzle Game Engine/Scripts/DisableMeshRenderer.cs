using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperPuzzleEngine
{
    public class DisableMeshRenderer : MonoBehaviour
    {
        private MeshRenderer m_Renderer;

        void Awake()
        {
            m_Renderer = GetComponent<MeshRenderer>();
        }

        void Update()
        {
            m_Renderer.enabled = false;
        }
    }
}