using HyperPuzzleEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace HyperPuzzleEngine
{
    public class NutMoveOnBolt : MonoBehaviour
    {
        public AudioSource nutFallSound;
        public AudioSource nutRotateSound;

        [Space]
        private bool canMove = false;
        private bool canMoveBack = false;

        Vector3 directionToMove = Vector3.zero;
        Transform boltEnd, boltStart;
        Transform currentTarget;
        RotateConstantly rotator;
        float movementSpeed = 1.8f;

        Vector3 fallBackPos;
        Animation outlineBadAnim;
        ShowcaseParent showcaseParent;

        public void Unscrew()
        {
            if (SkillButton.currentlyUsingSkill != null &&
                (SkillButton.currentlyUsingSkill.skillType == SkillButton.SkillType.NutsRemoveNuts))
            {
                SkillButton.currentlyUsingSkill.RemovedNut(this);
                Explode();
                return;
            }

            if (canMove || canMoveBack) return;

            directionToMove = (boltEnd.position - transform.position).normalized;
            nutRotateSound.Play();
            rotator.StartToRotate();
            currentTarget = boltEnd;
            canMove = true;
        }

        public void Explode()
        {
            Destroy(Instantiate(Resources.Load("NutFragments") as GameObject, transform.position + transform.up * 0.2f, Quaternion.identity), 2f);
            GetComponentInParent<SoundsManagerForTemplate>().PlaySound_Skill_Hammer_Smash();
            Destroy(gameObject);
        }

        public void Screw()
        {
            if (canMove || canMoveBack) return;

            directionToMove = (boltStart.position - transform.position).normalized;
            nutRotateSound.Play();
            rotator.StartToRotate();
            rotator.rotatingDifferentDirection = true;
            currentTarget = boltStart;
            canMove = true;
        }

        IEnumerator Start()
        {
            showcaseParent = GetComponentInParent<ShowcaseParent>();
            yield return null;
        }

        void Update()
        {
            if (!showcaseParent.IsInGameMode()) return;

            if (canMove)
            {
                //transform.Translate(directionToMove * movementSpeed * Time.deltaTime);
                transform.position = Vector3.MoveTowards(transform.position, currentTarget.position, movementSpeed * Time.deltaTime);
            }

            if (canMoveBack)
            {
                transform.position = Vector3.MoveTowards(transform.position, fallBackPos, movementSpeed * 2f * Time.deltaTime);
                if (Vector3.Distance(transform.position, fallBackPos) < 0.05f)
                {
                    transform.position = fallBackPos;
                    canMoveBack = false;
                    nutRotateSound.Stop();
                    rotator.StopRotating();
                }
            }
        }

        bool setParent = false;

        private void OnTriggerEnter(Collider other)
        {
            if (!showcaseParent.IsInGameMode()) return;

            if (!setParent && other.gameObject.GetComponent<BoltMover>() != null)
            {
                transform.parent = other.transform;
                transform.localPosition = new Vector3(0f, 0f, transform.localPosition.z);
                setParent = true;

                if (transform.GetComponentInParent<BoltMover>() != null)
                {
                    outlineBadAnim = transform.Find("Outline_Bad").GetComponent<Animation>();
                    fallBackPos = transform.position;

                    boltEnd = transform.parent.Find("BoltEnd");
                    boltStart = transform.parent.Find("BoltStart");

                    rotator = GetComponent<RotateConstantly>();
                }
            }

            if ((other.gameObject.name.Contains("BoltEnd") || other.gameObject.name.Contains("BoltStart")) &&
                (other.transform.parent == transform.parent))
            {
                MakeNutFall();
            }
            else if (((other.gameObject.name.Contains("Bolt") && !other.gameObject.name.Contains("End") && !other.gameObject.name.Contains("Start"))
                || other.gameObject.name.Contains("Block")) && (!other.gameObject.name.Contains("Nut")))
            {
                if (other.transform != transform.parent)
                {
                    //Different Bolt Collision
                    if (canMove)
                    {
                        canMoveBack = true;
                        rotator.rotatingDifferentDirection = true;
                        nutRotateSound.Play();
                        rotator.StartToRotate();
                    }
                    else
                    {
                        GetComponentInParent<BoltMover>().MoveBack();
                    }

                    canMove = false;

                    nutRotateSound.Stop();
                    rotator.StopRotating();

                    outlineBadAnim.Play();
                    GetComponentInParent<SoundsManagerForTemplate>().PlaySound_Stack_Clickable_FailedMatch();
                }
            }
            else if (other.gameObject.name.Contains("Nut"))
            {
                if (other.transform.parent == transform.parent)
                {
                    //Sibling Nut Collision
                    fallBackPos = transform.position - (other.transform.position - transform.position).normalized * 0.4f;

                    if (canMove)
                    {
                        canMoveBack = true;
                        rotator.rotatingDifferentDirection = true;
                        nutRotateSound.Play();
                        rotator.StartToRotate();
                    }
                    else
                    {
                        GetComponentInParent<BoltMover>().MoveBack();
                    }

                    canMove = false;

                    nutRotateSound.Stop();
                    rotator.StopRotating();

                    outlineBadAnim.Play();
                    GetComponentInParent<SoundsManagerForTemplate>().PlaySound_Stack_Clickable_FailedMatch();
                }
            }
        }

        private void MakeNutFall()
        {
            Destroy(GetComponent<PhysicalButton>());

            nutFallSound.Play();

            transform.parent = null;

            canMove = false;
            nutRotateSound.Stop();
            rotator.StopRotating();

            Rigidbody rb = GetComponent<Rigidbody>();


            rb.isKinematic = false;
            rb.AddForce(transform.up * 250f);
            rb.AddForce(transform.right * Random.Range(-45, 45));
            rb.AddForce(transform.forward * Random.Range(-45, 45));
            rb.AddTorque(
                new Vector3(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360))
                * Random.Range(0.5f, 1.5f));

            GetComponent<Collider>().isTrigger = false;
            Destroy(gameObject, 3f);
        }

    }
}