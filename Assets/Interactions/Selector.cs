using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

[RequireComponent(typeof(Raycaster))]
public class Selector : MonoBehaviour
{
    private Raycaster raycaster;
    [HideInInspector]
    public RaycastHit hit;
    public float selectionDistance = 100.0f;
    [Range(0.0f, 1.0f)]
    public float selectionPrecision; // selection error threshold, the higher the more precise but harder to select
    private float errorPercentage;
    [HideInInspector]
    public Interactable selectionInteractable;
    [HideInInspector]
    public Transform lastHighlighted;
    [HideInInspector]
    public Transform lastSelected;
    [HideInInspector]
    public bool selectionLocked;
    [HideInInspector]
    public Hand rightHand, leftHand;

    // Start is called before the first frame update
    void Start()
    {
        raycaster = GetComponent<Raycaster>();
        rightHand = GetComponent<Raycaster>().rightAttachmentPoint.GetComponentInParent<Hand>();
        leftHand = GetComponent<Raycaster>().leftAttachmentPoint.GetComponentInParent<Hand>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        LockSelection();
        if (lastHighlighted != null)
        {
            ShouldDeselect();
        }
        if (Physics.Raycast(raycaster.ray, out hit, selectionDistance) && lastHighlighted == null && !selectionLocked) // only select once and not while carrying something
        {
            ShouldSelect();
        }
    }

    private void ShouldSelect()
    {
        Transform selection = hit.transform;
        selectionInteractable = selection.GetComponent<Interactable>();
        if (selectionInteractable != null) // takes care of not selecting anything that is not interactable and highlights at the same time
        {
            selectionInteractable.OnHandHoverBegin(rightHand);
            lastHighlighted = selection;
        }
    }

    private void ShouldDeselect()
    {
        // prevents accidental deselection
        Vector3 vector1 = raycaster.ray.direction;
        Vector3 vector2 = lastHighlighted.position - raycaster.ray.origin;

        errorPercentage = Vector3.Dot(vector1.normalized, vector2.normalized);

        if (errorPercentage < selectionPrecision && !selectionLocked)
        {
            selectionInteractable.OnHandHoverEnd(rightHand);
            lastHighlighted = null;
        }
    }

    private void LockSelection()
    {
        if (SteamVR_Actions._default.GrabGrip.GetStateDown(rightHand.handType) && lastHighlighted != null)
        {
            lastSelected = lastHighlighted;
            lastSelected.gameObject.layer = 2;
            selectionLocked = true;
        }

        if (SteamVR_Actions._default.GrabGrip.GetStateUp(rightHand.handType) && lastSelected != null)
        {
            lastSelected.gameObject.layer = 0;
            selectionLocked = false;
        }
    }
}
