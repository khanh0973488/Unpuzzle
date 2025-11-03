using HyperPuzzleEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperPuzzleEngine
{
    public class RotatingBlade : MonoBehaviour
    {
        public bool spawnParticleOnDestroy = true;
        public bool increaseCollectedPiecesCounterOnDestroy = true;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.name.Contains("MovableCube"))
            {
                // Get the Block component to trigger OnThisCanBeCleared event
                Block blockComponent = other.GetComponent<Block>();
                if (blockComponent != null)
                {
                    blockComponent.OnThisCanBeCleared.Invoke();
                }

                if (spawnParticleOnDestroy)
                {
                    GameObject spawnedParticle = Instantiate(Resources.Load("MovableCubeDestroyParticle") as GameObject, transform);
                    spawnedParticle.transform.parent = null;
                    spawnedParticle.transform.position = other.transform.position;
                    spawnedParticle.GetComponent<ParticleSystemRenderer>().material = other.GetComponent<MeshRenderer>().material;
                    Destroy(spawnedParticle, 2f);
                }

                if (increaseCollectedPiecesCounterOnDestroy)
                {
                    Block targetBlockComponent = other.GetComponent<Block>();
                    if (targetBlockComponent != null)
                    {
                        targetBlockComponent.IncreaseCollectedCount();
                    }
                    else
                    {
                        // Fallback to direct counter if no Block component
                        CollectedStacksCounter counter = GetComponentInParent<CollectedStacksCounter>();
                        if (counter != null)
                        {
                            counter.IncreaseCollectedPieces(1, other.gameObject.GetInstanceID());
                        }
                    }
                }

                GetComponentInParent<SoundsManagerForTemplate>().PlaySound_Block_DestroyedByObstacle();

                ReduceLockedBlockCounter();

                Destroy(other.gameObject);
            }
        }

        private void ReduceLockedBlockCounter()
        {
            if (!LockBlock.TryMarkDecreased(LockBlock.DecreaseSource.Blade)) return;
            foreach (LockBlock lockedBlock in GetComponentInParent<ShowcaseParent>().GetComponentsInChildren<LockBlock>())
                lockedBlock.ReduceLockedCounter();
        }
    }
}