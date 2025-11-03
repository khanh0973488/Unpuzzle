using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace HyperPuzzleEngine
{
    [RequireComponent(typeof(Rigidbody))]
    public class ScrewManager : MonoBehaviour
    {
        public string nameOfHolePrefabToSpawn = "HolePrefab_Big";
        public bool unparentHolePrefabAtSpawn = true;

        private Rigidbody rb; // The shape's Rigidbody
        private List<HingeJoint> activeHinges = new List<HingeJoint>(); // List to keep track of active hinges
        private List<GameObject> screws = new List<GameObject>(); // List to keep track of active hinges

        private bool canCheckForScrews = true;

        List<Collider> colliders = new List<Collider>();

        public void ResetScrews()
        {
            StopAllCoroutines();

            canCheckForScrews = false;
            foreach (HingeJoint hinge in activeHinges)
                Destroy(hinge);
            activeHinges.Clear();
            screws.Clear();

            foreach (Collider collider in colliders)
            {
                collider.isTrigger = true;
                collider.enabled = true;
            }
        }

        IEnumerator Start()
        {
            colliders.Clear();
            foreach (Collider collider in GetComponents<Collider>())
                colliders.Add(collider);

            rb = GetComponent<Rigidbody>();
            rb.isKinematic = true; // Initially, the shape is kinematic

            foreach (Collider collider in colliders)
            {
                collider.isTrigger = true;
                collider.enabled = false;
            }

            yield return new WaitForSeconds(0.1f);

            foreach (Collider collider in colliders)
                collider.enabled = true;

            yield return new WaitForSeconds(0.1f);

            foreach (Collider collider in colliders)
                collider.isTrigger = false;

            yield return new WaitForSeconds(0.1f);

            UpdateHingesAndCheckForLastScrew();

            yield return new WaitForSeconds(1f);
            canCheckForScrews = false;
        }

        private HingeJoint SetupHingeJoint(GameObject screw)
        {
            HingeJoint hinge = gameObject.AddComponent<HingeJoint>();
            hinge.autoConfigureConnectedAnchor = false;

            hinge.axis = Vector3.forward; // Adjust as needed

            return hinge;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.name.ToLower().Contains("screw")) // Make sure screws are named appropriately
            {
                if (!canCheckForScrews) return;

                Screw currentScrew = other.gameObject.GetComponent<Screw>();
                if (currentScrew == null) return;

                GameObject collidingScrew = currentScrew.gameObject;

                if (screws.Contains(collidingScrew) || collidingScrew == currentlyReplacedScrewObject) return;

                if (screws.Contains(collidingScrew)) return;

                Debug.Log("COLLIDING WITH SCREW!!!");

                if (transform.Find(other.name) != null)
                    transform.Find(other.name).parent = transform.parent;

                var hinge = SetupHingeJoint(other.gameObject);
                if (hinge != null)
                {
                    activeHinges.Add(hinge);
                    screws.Add(collidingScrew);

                    hinge.connectedBody = collidingScrew.GetComponent<Rigidbody>();
                    collidingScrew.transform.parent = transform.parent;

                    hinge.anchor = transform.InverseTransformPoint(collidingScrew.transform.position);

                    currentScrew.SetScrewManager(this);
                    SpawnHole(other.transform.position);
                }
            }
        }

        Vector3 holePosOffset = new Vector3(0f, 0f, -0.06f);
        float holeDiameter = 0.2f;

        void SpawnHole(Vector3 screwPosition)
        {
            GameObject newHole = Instantiate(Resources.Load(nameOfHolePrefabToSpawn) as GameObject, screwPosition + holePosOffset, Quaternion.identity);
            //newHole.transform.localScale = Vector3.one * holeDiameter;
            if (!unparentHolePrefabAtSpawn)
                newHole.transform.parent = transform;
        }

        public void RemoveScrew(GameObject screw)
        {
            Debug.Log("Trying To Remove Screw: " + screw.gameObject.name);

            // Find and remove the corresponding hinge joint
            HingeJoint hingeToRemove = null;
            foreach (var hinge in activeHinges)
            {
                Debug.Log("Trying To Remove Screw; There are hinges: " + hinge.gameObject.name);

                if (hinge.connectedBody == currentDummyScrewObject.GetComponent<Rigidbody>())
                {
                    hingeToRemove = hinge;
                    break;
                }
            }

            if (hingeToRemove != null)
            {
                activeHinges.Remove(hingeToRemove);
                Destroy(hingeToRemove);
            }
            else
                return;

            if (currentlyReplacedScrewObject != null && screws.Contains(currentlyReplacedScrewObject))
                screws.Remove(currentlyReplacedScrewObject);

            if (currentDummyScrewObject != null && screws.Contains(currentDummyScrewObject))
            {
                screws.Remove(currentDummyScrewObject);
                Destroy(currentDummyScrewObject);
            }
            currentDummyScrewObject = currentlyReplacedScrewObject = null;

            screw.GetComponent<Screw>().SetScrewManager(null);

            //Destroy(screw); // Removes the screw from the scene
            UpdateHingesAndCheckForLastScrew();
        }

        private void UpdateHingesAndCheckForLastScrew()
        {
            if (activeHinges.Count == 1)
            {
                rb.isKinematic = false; // Allows the shape to start moving according to physics
                activeHinges[0].useLimits = false; // Allows free rotation
                rb.position = rb.position; // Reaffirm the Rigidbody's position to avoid jumps
            }
            else if (activeHinges.Count < 1)
                rb.constraints = RigidbodyConstraints.None;
        }

        #region Dummy Screw Management

        GameObject currentDummyScrewObject;
        GameObject currentlyReplacedScrewObject;
        HingeJoint currentHingeJoint;

        public void ReplaceToDummyScrew(Screw screwToReplace)
        {
            currentlyReplacedScrewObject = screwToReplace.gameObject;
            currentDummyScrewObject = Instantiate(new GameObject("DummyScrew"),
                currentlyReplacedScrewObject.transform.position, currentlyReplacedScrewObject.transform.rotation);
            currentDummyScrewObject.AddComponent<Rigidbody>().isKinematic = true;

            for (int i = 0; i < activeHinges.Count; i++)
            {
                if (activeHinges[i].connectedBody == currentlyReplacedScrewObject.GetComponent<Rigidbody>())
                {
                    currentHingeJoint = activeHinges[i];
                    screws[i] = currentDummyScrewObject;
                    break;
                }
            }

            if (currentHingeJoint != null)
                currentHingeJoint.connectedBody = currentDummyScrewObject.GetComponent<Rigidbody>();
        }

        public void UndoDummyScrewReplace()
        {
            if (currentDummyScrewObject == null || currentlyReplacedScrewObject == null)
            {
                Debug.Log("NOTHING TO REPLACE_DUMMY");
                return;
            }

            for (int i = 0; i < activeHinges.Count; i++)
            {
                if (activeHinges[i].connectedBody == currentDummyScrewObject.GetComponent<Rigidbody>())
                {
                    currentHingeJoint = activeHinges[i];
                    screws[i] = currentlyReplacedScrewObject;
                    break;
                }
            }
            currentlyReplacedScrewObject.transform.position = currentDummyScrewObject.transform.position;
            currentlyReplacedScrewObject.transform.rotation = currentDummyScrewObject.transform.rotation;

            currentHingeJoint.connectedBody = currentlyReplacedScrewObject.GetComponent<Rigidbody>();

            Destroy(currentDummyScrewObject);

            currentDummyScrewObject = null;
            currentlyReplacedScrewObject = null;
        }

        #endregion
    }
}