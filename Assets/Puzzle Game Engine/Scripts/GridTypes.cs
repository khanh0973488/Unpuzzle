using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HyperPuzzleEngine;

namespace HyperPuzzleEngine
{
    [CreateAssetMenu(fileName = "Grid Types")]
    public class GridTypes : ScriptableObject
    {
        public enum GridType
        {
            Square,
            Hexagon,
            Rectangle,
            Coin
        }

        [Header("Grid Prefabs")]
        [SerializeField]
        public GameObject gridPrefabSquare;
        [SerializeField]
        public GameObject gridPrefabHexagon;
        [SerializeField]
        public GameObject gridPrefabRectangle;
        [SerializeField]
        public GameObject gridPrefabCoin;
    }
}