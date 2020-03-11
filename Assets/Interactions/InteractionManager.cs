using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class InteractionManager : MonoBehaviour
{
    public enum Handedness
    {
        right,
        left,
    };
    public Handedness handedness;

    private GameObject handAttachmentPoint;
    private Hand hand;

    public float rayDistance = 20;

    [Tooltip("Selection error threshold, the higher the more precise but harder to select.")]
    [Range(0.0f, 1.0f)]
    public float selectionPrecision = 0.8f;
    [Tooltip("Speed of horizontal lerp.")]
    [Range(1.0f, 5.0f)]
    public float movementSpeed = 2.0f;
    public float rotationDegree = 0.5f;

    [SerializeField] private Transform lastHighlighted;
    [SerializeField] private Transform lastSelected;

    private Interactable selectionInteractable;
    private bool selectionLocked;

    private Vector3 lastHit;

    // Start is called before the first frame update
    void Start()
    {
        switch (handedness)
        {
            case Handedness.right:
                handAttachmentPoint = GameObject.FindGameObjectWithTag("ControllerRight").transform.parent.GetChild(1).gameObject;
                break;
            case Handedness.left:
                handAttachmentPoint = GameObject.FindGameObjectWithTag("ControllerLeft").transform.parent.GetChild(1).gameObject;
                break;
        }
        hand = handAttachmentPoint.GetComponentInParent<Hand>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Ray ray = new Ray(handAttachmentPoint.transform.position, handAttachmentPoint.transform.forward);
        Select(ray);
        if (lastHighlighted != null)
        {
            Deselect(ray);
            LockSelection();
        }
        
        if (selectionLocked)
        {
            MoveObjectToRay(ray);
            DeleteObject();
            RotateObject();
        }
    }

    private void RotateObject()
    {
        if (SteamVR_Input.GetState("SnapTurnLeft", hand.handType))
        {
            lastSelected.Rotate(0, rotationDegree, 0);
        }
        if (SteamVR_Input.GetState("SnapTurnRight", hand.handType))
        {
            lastSelected.Rotate(0, -rotationDegree, 0);
        }
    }

    private void DeleteObject()
    {
        if (SteamVR_Actions._default.GrabGrip.GetStateDown(hand.otherHand.handType))
        {
            GameObject.Destroy(lastSelected.gameObject);
            lastSelected = null;
        }
    }

    private void Deselect(Ray ray)
    {
        // prevents accidental deselection
        Vector3 vector1 = ray.direction;
        Vector3 vector2 = lastHighlighted.position - ray.origin;
        float errorPercentage;
        errorPercentage = Vector3.Dot(vector1.normalized, vector2.normalized);

        if (errorPercentage < selectionPrecision && !selectionLocked)
        {
            selectionInteractable.OnHandHoverEnd(hand);
            lastHighlighted = null;
        }
    }

    private void Select(Ray ray)
    {
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, rayDistance) && lastHighlighted == null && !selectionLocked) // only select once and not while carrying something
        {
            Transform selection = hit.transform;
            selectionInteractable = selection.GetComponent<Interactable>();
            if (selectionInteractable != null) // takes care of not selecting anything that is not interactable and highlights at the same time
            {
                selectionInteractable.OnHandHoverBegin(hand);
                lastHighlighted = selection;
            }
        }
    }

    private void LockSelection()
    {
        if (SteamVR_Actions._default.GrabPinch.GetStateDown(hand.handType))
        {
            lastSelected = lastHighlighted;
            lastSelected.gameObject.layer = 2;
            selectionLocked = true;
        }

        if (SteamVR_Actions._default.GrabPinch.GetStateUp(hand.handType))
        {
            lastSelected.gameObject.layer = 0;
            lastSelected.parent = null;
            lastSelected.gameObject.GetComponent<Rigidbody>().isKinematic = false;
            lastSelected = null;
            selectionLocked = false;
        }
    }

    private void MoveObjectToRay(Ray ray)
    {
        Rigidbody rb = lastSelected.gameObject.GetComponent<Rigidbody>();

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, 1 << LayerMask.NameToLayer("Surface"))) //if raycast is hitting the surface
        {
            lastHit = hit.point;
            MoveHorizontal(rb);

        } else //if we don't hit the floor
        {
            MoveVertical(rb);
        }

    }

    private void MoveVertical(Rigidbody rb) // gameobject on the stick
    {
        rb.isKinematic = true;
        lastSelected.parent = handAttachmentPoint.transform;
    }

    private void MoveHorizontal(Rigidbody rb) // <----->
    {
        rb.isKinematic = false;
        lastSelected.parent = null;
        rb.MovePosition(Vector3.Lerp(lastSelected.transform.position, lastHit, Time.deltaTime * movementSpeed));
    }
}
