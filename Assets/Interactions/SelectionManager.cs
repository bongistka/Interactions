using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class SelectionManager : MonoBehaviour
{
    // todo handedness/controller active or whatever lets us choose which controler to draw ray from
    public GameObject rightAttachmentPoint;
    public Hand rightHand;

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

    // Start is called before the first frame update
    void Start()
    {
        handLastPosition = rightAttachmentPoint.transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (selectionLocked)
        {
            MoveObjectY();
        }

        Ray ray = new Ray(rightAttachmentPoint.transform.position, rightAttachmentPoint.transform.forward);
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
                selectionInteractable.OnHandHoverEnd(rightHand);
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
                selectionInteractable.OnHandHoverBegin(rightHand);
                lastSelected = selection;
            }
        }

        if (selectionLocked && !sideMovementLocked)
        {
            MoveObjectToRay(hit);
        }
        handLastPosition = rightAttachmentPoint.transform.position;
    }

    private void MoveObjectY()
    {
        // moves along Y axis if vertical hand motion detected
        Vector3 currentPosition = rightAttachmentPoint.transform.position;
        Vector3 difference = currentPosition - handLastPosition;

        Rigidbody rb = lastSelected.gameObject.GetComponent<Rigidbody>();
        rb.isKinematic = true;

        if ((Mathf.Abs(difference.x) < Mathf.Abs(difference.y)) && (Mathf.Abs(difference.z) < Mathf.Abs(difference.y)))
        {
            sideMovementLocked = true;
            
            Vector3 objectPosition = lastSelected.gameObject.transform.position;
            rb.MovePosition(new Vector3(objectPosition.x, objectPosition.y + difference.y * yMovementSpeed, objectPosition.z));
            //Debug.Log( "rb moved up");
        } else
        {
            rb.isKinematic = false;
            sideMovementLocked = false;
        }
    }

    private void MoveObjectToRay(RaycastHit hit)
    {
        try
        {
            if (hit.point != null && hit.transform.tag == "Floor")
            {
                Rigidbody rb = lastSelected.gameObject.GetComponent<Rigidbody>();
                rb.MovePosition(Vector3.Lerp(lastSelected.transform.position, new Vector3(hit.point.x, lastSelected.transform.position.y, hit.point.z), Time.deltaTime));

            }
        } catch (NullReferenceException e)
        {
            Debug.Log(e);
            handLastPosition = rightAttachmentPoint.transform.position;
        }

    }

    private void RotateSelected()
    {
        if (SteamVR_Actions._default.SnapTurnLeft.GetStateDown(rightHand.handType))
        {
            lastSelected.Rotate(0, rotationDegree, 0);
        }

        if (SteamVR_Actions._default.SnapTurnRight.GetStateDown(rightHand.handType))
        {
            lastSelected.Rotate(0, -rotationDegree, 0);
        }
    }

    private void GrabSelection()
    {
        if (SteamVR_Actions._default.GrabPinch.GetStateDown(rightHand.handType))
        {
            selectionLocked = true;
        }

        if (SteamVR_Actions._default.GrabPinch.GetStateUp(rightHand.handType))
        {
            selectionLocked = false;
        }
    }
}
