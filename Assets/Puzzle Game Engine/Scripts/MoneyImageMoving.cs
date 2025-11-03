using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using HyperPuzzleEngine;

namespace HyperPuzzleEngine
{
    public class MoneyImageMoving : MonoBehaviour
    {
        public float speed = 10;
        public float distance = 100f;
        public Transform moneyTargetTransform;

        Vector3 targetPos;

        private bool canMove = true;

        [HideInInspector] public int totalMoneyAmount;

        private Vector3 startPos = new Vector3(0f, 0f, 0f);
        private Vector3 startLocalScale = new Vector3(0f, 0f, 0f);
        float startSpeed = 0f;

        public void MoveTowardsMoneyIcon()
        {
            targetPos = moneyTargetTransform.position;
            speed *= 3.4f;
        }

        void OnEnable()
        {
            StopAllCoroutines();

            if (startPos == Vector3.zero)
            {
                startLocalScale = transform.localScale;
                startPos = transform.position;
                startSpeed = speed;
            }
            else
            {
                transform.localScale = startLocalScale;
                transform.position = startPos;
                speed = startSpeed;
            }

            GetComponent<Image>().enabled = true;

            canMove = true;

            Invoke("MoveTowardsMoneyIcon", 0.3f + transform.GetSiblingIndex() * 0.08f);

            targetPos = startPos + Vector3.right * Random.Range(-distance, distance) + Vector3.forward * Random.Range(-distance, distance);

            StartCoroutine(Move());

        }

        IEnumerator Move()
        {
            while (canMove)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

                if (Vector3.Distance(transform.position, moneyTargetTransform.position) < 0.05f)
                {
                    canMove = false;
                    transform.position = moneyTargetTransform.position;
                    GetComponent<Animation>().Play();
                }

                yield return new WaitForEndOfFrame();
            }

            yield return new WaitForSeconds(0.5f);

            if (transform.GetSiblingIndex() == transform.parent.childCount - 1)
                transform.parent.gameObject.SetActive(false);
        }
    }
}
