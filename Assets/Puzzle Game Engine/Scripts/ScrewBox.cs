using HyperPuzzleEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperPuzzleEngine
{
    public class ScrewBox : MonoBehaviour
    {
        private List<BoltMover> holdingBolts = new List<BoltMover>();

        private bool canAddHoldingBolts = true;

        private void Start()
        {
            GetComponent<MeshRenderer>().enabled = false;
            Invoke("StopAddingHoldingBolts", 0.3f);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!canAddHoldingBolts) return;

            BoltMover currentlyCollidingBoltMover = other.GetComponent<BoltMover>();
            if (currentlyCollidingBoltMover != null)
            {
                if (!holdingBolts.Contains(currentlyCollidingBoltMover))
                    holdingBolts.Add(currentlyCollidingBoltMover);
            }
        }

        void StopAddingHoldingBolts()
        {
            canAddHoldingBolts = false;

            if (holdingBolts.Count < 2)
                Destroy(gameObject);
            else
                GetComponent<MeshRenderer>().enabled = true;
        }

        public void ExplodeScrewBox()
        {
            Transform boxShattered = transform.GetChild(0);

            boxShattered.parent = null;
            boxShattered.gameObject.SetActive(true);

            foreach (DestroyGameobject fragment in boxShattered.GetComponentsInChildren<DestroyGameobject>())
                fragment.DestroyObject(3f);

            Destroy(gameObject);
        }


        public void RemoveHoldingBolt(BoltMover boltToRemove)
        {
            if (holdingBolts.Contains(boltToRemove))
                holdingBolts.Remove(boltToRemove);

            if (holdingBolts.Count <= 1)
                ExplodeScrewBox();
        }
    }
}