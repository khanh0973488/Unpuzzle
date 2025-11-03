using UnityEngine;
using HyperPuzzleEngine;

namespace HyperPuzzleEngine
{
    [CreateAssetMenu(fileName = "Stack Colors")]
    public class StackColors : ScriptableObject
    {
        public enum StackColor
        {
            Blue,
            Red,
            Yellow,
            Green,
            Orange,
            Purple
        }

        public Color[] colors;

        public Color GetRandomColor()
        {
            return colors[Random.Range(0, colors.Length)];
        }

        public Color GetRandomColor(int maxEnabledIndex)
        {
            if (maxEnabledIndex >= colors.Length)
                maxEnabledIndex = colors.Length - 1;

            return colors[Random.Range(0, maxEnabledIndex)];
        }

        public int GetIndexOfColor(Color color)
        {
            for (int i = 0; i < colors.Length; i++)
            {
                if (colors[i] == color)
                    return i;
            }
            return -1;
        }
    }
}