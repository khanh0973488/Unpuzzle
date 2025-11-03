using HyperPuzzleEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

namespace HyperPuzzleEngine
{
    public class OnTriggerEnterDoEvent : MonoBehaviour
{
    public string colliderNameContains;
    public UnityEvent onTriggerEnterEvent;
    public UnityEvent onTriggerExitEvent;

    public static List<GameObject> colliderNuts = new List<GameObject>();

    private void OnTriggerExit(Collider other)
    {
        if (other.name.ToLower().Contains(colliderNameContains.ToLower()))
        {
            if (!colliderNuts.Contains(other.gameObject) && GetComponentInParent<ShowcaseParent>().IsInGameMode())
            {
                if (other.gameObject.GetComponentInChildren<NutMoveOnBolt>() == null &&
                    other.gameObject.GetComponent<BoltMover>() != null)
                {
                    GetComponentInParent<SoundsManagerForTemplate>().PlaySound_Slices_Filled();

                    Debug.Log("WIN_");
                    colliderNuts.Add(other.gameObject);
                    onTriggerExitEvent.Invoke();
                    Destroy(other.gameObject, 2f);
                }
            }
        }
    }
}
}