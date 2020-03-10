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

    public float rayDistance = 100;
    [Range(0.0f, 1.0f)]
    public float selectionPrecision = 0.8f; // selection error threshold, the higher the more precise but harder to select

    [SerializeField] private Transform lastHighlighted;
    [SerializeField] private Transform lastSelected;

    private Interactable selectionInteractable;
    private bool selectionLocked;

    private Vector3 lastHit;

    [Range(1.0f, 5.0f)]
    public float movementSpeed = 2.0f;

    public int waitTillMove = 90;
    private int waitUp = 0;
    private int waitDown = 0;

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
        Deselect(ray);
        LockSelection();
        if (selectionLocked)
        {
            MoveObjectToRay(ray);
        }
    }

    private void Deselect(Ray ray)
    {
        if (lastHighlighted != null)
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
            lastSelected = null;
            selectionLocked = false;
        }
    }

    private void MoveObjectToRay(Ray ray)
    {
        Physics.Raycast(ray, out RaycastHit hit, rayDistance);
        Rigidbody rb = lastSelected.gameObject.GetComponent<Rigidbody>();
        if (hit.point != null && hit.transform.CompareTag("Floor"))
        {
            lastHit = hit.point;
            rb.MovePosition(Vector3.Lerp(lastSelected.transform.position, lastHit, Time.deltaTime * movementSpeed));
        } else
        {
            rb.MovePosition(Vector3.Lerp(lastSelected.transform.position, lastHit, Time.deltaTime * movementSpeed));
        }

    }
}
