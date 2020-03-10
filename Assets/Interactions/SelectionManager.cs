using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class SelectionManager : MonoBehaviour
{
    public int changeMovementTime = 50;

    // todo handedness/controller active or whatever lets us choose which controler to draw ray from
    public GameObject handAttachmentPoint;
    public Hand hand;

    [Range(0.0f, 1.0f)]
    public float selectionPrecision; // selection error threshold, the higher the more precise but harder to select
    [Range(1.0f, 10.0f)]
    public float yMovementSpeed; // multiplicator of Y movement
    [Range(0.0f, 5.0f)]
    public float rotationDegree; 
    public float rayDistance; 
    private float errorPercentage;
    private bool selectionLocked;
    private bool sideMovementLocked;
    private Interactable selectionInteractable;
    private Vector3 handLastPosition;

    [SerializeField] private Transform lastSelected;
    private Vector3 lastHit;
    private int framesToLift = 0;

    // Start is called before the first frame update
    void Start()
    {
        handLastPosition = handAttachmentPoint.transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (selectionLocked)
        {
            //MoveObjectY();
        }

        Ray ray = new Ray(handAttachmentPoint.transform.position, handAttachmentPoint.transform.forward);
        RaycastHit hit;

        if (lastSelected != null)
        {
            GrabSelection();
            RotateSelected();

            // prevents accidental deselection
            Vector3 vector1 = ray.direction;
            Vector3 vector2 = lastSelected.position - ray.origin;

            errorPercentage = Vector3.Dot(vector1.normalized, vector2.normalized);

            if (errorPercentage < selectionPrecision && !selectionLocked)
            {
                selectionInteractable.OnHandHoverEnd(hand);
                Rigidbody rb = lastSelected.gameObject.GetComponent<Rigidbody>();
                rb.isKinematic = false;
                lastSelected = null;
            }
        }

        if (Physics.Raycast(ray, out hit, rayDistance) && lastSelected == null && !selectionLocked) // only select once and not while carrying something
        {
            Transform selection = hit.transform;
            selectionInteractable = selection.GetComponent<Interactable>();
            if (selectionInteractable != null) // takes care of not selecting anything that is not interactable and highlights at the same time
            {
                selectionInteractable.OnHandHoverBegin(hand);
                lastSelected = selection;
            }
        }

        if (selectionLocked && !sideMovementLocked)
        {
            MoveObjectToRay(hit);
        }
        handLastPosition = handAttachmentPoint.transform.position;
    }

    private void MoveObjectY()
    {
        // moves along Y axis if vertical hand motion detected


        Rigidbody rb = lastSelected.gameObject.GetComponent<Rigidbody>();
        rb.isKinematic = true;

        sideMovementLocked = true;
        //rb.isKinematic = true;
        //rb.useGravity = false;

        lastSelected.parent = handAttachmentPoint.transform;
    }

    private void MoveObjectToRay(RaycastHit hit)
    {
        try
        {
            Rigidbody rb = lastSelected.gameObject.GetComponent<Rigidbody>();
            if (hit.point != null && hit.transform.CompareTag("Floor"))
            {
                //sideMovementLocked = false;
                rb.isKinematic = false;
                lastSelected.parent = null;
                lastHit = hit.point;
                rb.MovePosition(Vector3.Lerp(lastSelected.transform.position, lastHit, Time.deltaTime));
                framesToLift = 0;
            }
            else
            {
                if (framesToLift < changeMovementTime)
                {
                    framesToLift += 1;
                    rb.MovePosition(Vector3.Lerp(lastSelected.transform.position, lastHit, Time.deltaTime));
                } else
                {
                    MoveObjectY();
                    
                }
                
                //MoveObjectY();

            }
        } catch (NullReferenceException e)
        {
            
            //Debug.Log(e);
        }
        handLastPosition = handAttachmentPoint.transform.position;

    }

    private void RotateSelected()
    {
        if (SteamVR_Actions._default.SnapTurnLeft.GetStateDown(hand.handType))
        {
            lastSelected.Rotate(0, rotationDegree, 0);
        }

        if (SteamVR_Actions._default.SnapTurnRight.GetStateDown(hand.handType))
        {
            lastSelected.Rotate(0, -rotationDegree, 0);
        }
    }

    private void GrabSelection()
    {
        if (SteamVR_Actions._default.GrabPinch.GetStateDown(hand.handType))
        {
            
            lastSelected.gameObject.layer = 2;
            selectionLocked = true;
        }

        if (SteamVR_Actions._default.GrabPinch.GetStateUp(hand.handType))
        {
            lastSelected.gameObject.layer = 0;
            lastSelected.parent = null;
            lastSelected.gameObject.GetComponent<Rigidbody>().isKinematic = false;
            //lastSelected.gameObject.GetComponent<Rigidbody>().useGravity = true;
            selectionLocked = false;
            sideMovementLocked = false;
        }
    }
}
