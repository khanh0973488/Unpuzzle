using UnityEngine;
using HyperPuzzleEngine;

namespace HyperPuzzleEngine
{
    public class ParabolicJumpEnd : MonoBehaviour
    {
        [HideInInspector] public Transform objectToPosition;

        private void OnTriggerEnter(Collider other)
        {
            if (other.transform == objectToPosition)
            {
                other.GetComponent<ParabolicJump>().FinishJumping();
                Destroy(gameObject);
            }
        }
    }
}
