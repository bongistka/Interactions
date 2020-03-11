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

    public enum DefaultMovement
    {
        horizontal,
        vertical,
    };
    [Tooltip("Preferred type of movement.")]
    public DefaultMovement defaultMovement; 

    private GameObject handAttachmentPoint;
    private Hand hand;

    public float rayDistance = 100;
    public int waitTillMove = 2;

    [Tooltip("Selection error threshold, the higher the more precise but harder to select.")]
    [Range(0.0f, 1.0f)]
    public float selectionPrecision = 0.8f;
    [Tooltip("Speed of horizontal lerp.")]
    [Range(1.0f, 5.0f)]
    public float movementSpeed = 2.0f;

    [SerializeField] private Transform lastHighlighted;
    [SerializeField] private Transform lastSelected;

    private Interactable selectionInteractable;
    private bool selectionLocked;

    private Vector3 lastHit;

    [SerializeField] private int waitUp = 0;
    [SerializeField] private int waitDown = 0;
    public string hittingWhat;

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

        switch (defaultMovement)
        {
            case DefaultMovement.horizontal:
                waitDown = waitTillMove;
                break;
            case DefaultMovement.vertical:
                waitUp = waitTillMove;
                break;
        }
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
            lastSelected.parent = null;
            lastSelected.gameObject.GetComponent<Rigidbody>().isKinematic = false;
            lastSelected = null;
            selectionLocked = false;
            waitUp = 0;
            waitDown = 0;
        }
    }

    private void MoveObjectToRay(Ray ray)
    {
        Rigidbody rb = lastSelected.gameObject.GetComponent<Rigidbody>();

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, 1 << LayerMask.NameToLayer("Floor"))) //if raycast is hitting the floor
        {
            hittingWhat = hit.transform.gameObject.name;
            lastHit = hit.point;
            waitUp = 0;
            if (waitDown < waitTillMove) //if we've been up before we count so to prevent accidental drop
            {
                waitDown += 1;
                MoveVertical(rb); // otherwise we stay in the air
            }
            else //if countdown done we go to the floor (also default if horizontal movement set to default)
            {
                MoveHorizontal(rb);
            }

        } else //if we don't hit the floor
        {
            //hittingWhat = hit.transform.gameObject.name;
            waitDown = 0;
            if (waitUp < waitTillMove) // we count to prewent accidental lift (also default if vertical movement set to default)
            {
                waitUp += 1;
                MoveHorizontal(rb); // otherwise we stay on the floor
            }
            else // if countdown done we lift the object
            {
                MoveVertical(rb);
            }
        }

    }

    private void MoveVertical(Rigidbody rb) // up, down
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
