using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperPuzzleEngine
{
    public class FollowMousePosition : MonoBehaviour
{
    public float speed = 1000f;

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, Input.mousePosition, speed * Time.deltaTime);

        if (Input.GetMouseButtonDown(0))
            GetComponent<Animation>().Play();
    }
}
}