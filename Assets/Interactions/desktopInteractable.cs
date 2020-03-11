using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;
using Valve.VR;

[RequireComponent(typeof(Interactable))]
public class DesktopInteractable : MonoBehaviour
{
    private float rotationDegree;
    private float scalingFactor;

    // Start is called before the first frame update
    void Awake()
    {
        rotationDegree = GameObject.FindObjectOfType<InteractionManager>().rotationDegree;
        scalingFactor = GameObject.FindObjectOfType<InteractionManager>().scalingFactor;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void HandHoverUpdate(Hand hand)
    {
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            GameObject.Destroy(this.gameObject);
        }
        if (Input.GetKey(KeyCode.R))
        {
            this.gameObject.transform.Rotate(0, rotationDegree, 0);
        }
        if (Input.GetKey(KeyCode.U))
        {
            this.gameObject.transform.localScale *= scalingFactor;
        }
        if (Input.GetKey(KeyCode.I))
        {
            this.gameObject.transform.localScale *= 2 - scalingFactor;
        }
    }
}
