using HyperPuzzleEngine;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace HyperPuzzleEngine
{
    public class MovesConstraint : MonoBehaviour
    {
        public bool setUpMovesAutomatically = false;
        [Range(0f, 2f)]
        public float moreThanMinimumMovesByMultiplier = 0;

        [Space]
        public int countOfMovesAtStart = 10;
        public int playTickingAnimAtMoves = 5;
        public string textPrefix = "Moves: ";

        private int tempMoves;
        private TextMeshPro thisText;
        private Animation anim;

        public UnityEvent OnOutOfMoves;

        private void Start()
        {
            anim = GetComponent<Animation>();
            thisText = GetComponent<TextMeshPro>();
            tempMoves = countOfMovesAtStart;

            UpdateMoves(0);

            if (setUpMovesAutomatically)
                Invoke(nameof(SetUpMaxMovesAuutomaticallyByCollectables), 0.1f);
        }

        public void UpdateMoves(int addMoves)
        {
            tempMoves += addMoves;
            
            // Đảm bảo move không thể âm
            if (tempMoves < 0)
                tempMoves = 0;
            
            thisText.text = textPrefix + tempMoves.ToString();

            if (tempMoves <= playTickingAnimAtMoves && !anim.isPlaying)
                anim.Play();

            // Khi move về 0, gọi hàm mới trong LevelManager
            if (tempMoves <= 0)
            {
                LevelManager levelManager = GetComponentInParent<LevelManager>();
                if (levelManager != null)
                {
                    levelManager.OnMovesReachedZero();
                }
                else
                {
                    // Fallback: gọi event cũ nếu không tìm thấy LevelManager
                    OnOutOfMoves.Invoke();
                }
            }
        }

        void SetUpMaxMovesAuutomaticallyByCollectables()
        {
            tempMoves = countOfMovesAtStart = Mathf.RoundToInt(GetComponentInParent<CollectedStacksCounter>().GetCurrentlyNeededToCollect() * moreThanMinimumMovesByMultiplier);
            UpdateMoves(0);
        }
    }
}
