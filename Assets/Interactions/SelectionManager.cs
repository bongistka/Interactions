using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class SelectionManager : MonoBehaviour
{
    public GameObject rightAttachmentPoint;
    public Hand rightHand;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = new Ray(rightAttachmentPoint.transform.position, rightAttachmentPoint.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            Transform selection = hit.transform;
            Interactable selectionInteractable = selection.GetComponent<Interactable>();
            if (selectionInteractable != null)
            {
                selectionInteractable.OnHandHoverBegin(rightHand);
            }
        }
    }
}
