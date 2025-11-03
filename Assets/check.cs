using System.Collections;
using System.Collections.Generic;
using TMPro;

//using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class check : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private TextMeshProUGUI txt;

    private void OnEnable()
    {
        var posCA = Camera.main.transform.position;
        txt.text = posCA.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
