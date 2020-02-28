using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class SelectionManager : MonoBehaviour
{
    //todo handedness/controller active or whatever lets us choose which controler to draw ray from
    public GameObject rightAttachmentPoint;
    public Hand rightHand;

    [Range(0.0f, 1.0f)]
    public float selectionPrecision; // selection error threshold, the higher the more precise but harder to select
    private float errorPercentage;
    private bool selectionLocked;
    private Interactable selectionInteractable;

    [SerializeField] private Transform lastSelected;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = new Ray(rightAttachmentPoint.transform.position, rightAttachmentPoint.transform.forward);
        RaycastHit hit;

        if (lastSelected != null)
        {
            // locks selection

            if (SteamVR_Actions._default.GrabPinch.GetStateDown(rightHand.handType))
            {
                selectionLocked = true;
            }

            if (SteamVR_Actions._default.GrabPinch.GetStateUp(rightHand.handType))
            {
                selectionLocked = false;
            }

            // prevents accidental deselection
            Vector3 vector1 = ray.direction;
            Vector3 vector2 = lastSelected.position - ray.origin;

            errorPercentage = Vector3.Dot(vector1.normalized, vector2.normalized);

            if (errorPercentage < selectionPrecision && !selectionLocked)
            {
                selectionInteractable.OnHandHoverEnd(rightHand);
                lastSelected = null;
            }
        }

        if (Physics.Raycast(ray, out hit) && lastSelected == null && !selectionLocked) // only select once and not while carrying something
        {
            Transform selection = hit.transform;
            selectionInteractable = selection.GetComponent<Interactable>();
            if (selectionInteractable != null) // takes care of not selecting anything that is not interactable and highlights at the same time
            {
                selectionInteractable.OnHandHoverBegin(rightHand);
                lastSelected = selection;
            }
        }
        
    }

}
