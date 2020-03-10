using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

[RequireComponent(typeof(Selector), typeof(Raycaster))]
public class MoveObject : MonoBehaviour
{
    [Range(0.0f, 1.0f)]
    public float yMovementThreshold = 0.2f;
    
    private Selector selector;
    private Raycaster raycaster;
    private Vector3 handLastPosition;
    private bool sideMovementLocked;

    // Start is called before the first frame update
    void Start()
    {
        selector = GetComponent<Selector>();
        raycaster = GetComponent<Raycaster>();
        handLastPosition = raycaster.rightAttachmentPoint.transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (selector.selectionLocked)
        {
            if (!sideMovementLocked)
            {
                MoveObjectToRay(selector.hit);
            }
            MoveObjectY();
            //RotateObject();
        }
        else
        {
            ReleaseObject();
        }
        handLastPosition = raycaster.rightAttachmentPoint.transform.position;
    }

    private void RotateObject()
    {
        if (SteamVR_Actions._default.SnapTurnLeft.GetStateDown(selector.rightHand.handType))
        {
            selector.lastSelected.Rotate(new Vector3(0, 1, 0), Space.Self);
        }

        if (SteamVR_Actions._default.SnapTurnRight.GetStateDown(selector.rightHand.handType))
        {
            selector.lastSelected.Rotate(new Vector3(0, -1, 0), Space.Self);
        }
    }

    private void ReleaseObject()
    {
        if (selector.lastSelected != null)
        {
            //Debug.Log("Object " + selector.lastSelected + " was released");
            selector.lastSelected.GetComponent<Rigidbody>().useGravity = true;
            selector.lastSelected.GetComponent<Rigidbody>().velocity = Vector3.zero;
            selector.lastSelected.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            selector.lastSelected.parent = null;
            selector.lastSelected = null;
        }    
    }

    private void MoveObjectY()
    {
        // moves along Y axis if vertical hand motion detected
        Vector3 currentPosition = raycaster.rightAttachmentPoint.transform.position;
        Vector3 difference = currentPosition - handLastPosition;

        if ((Mathf.Abs(difference.x) < Mathf.Abs(difference.y)) && (Mathf.Abs(difference.z) < Mathf.Abs(difference.y)) && (Mathf.Abs(difference.y) > yMovementThreshold))
        {
            sideMovementLocked = true;

            if (selector.lastSelected != null)
            {
                selector.lastSelected.GetComponent<Rigidbody>().useGravity = false;
                selector.lastSelected.parent = raycaster.rightAttachmentPoint.transform;
                selector.lastSelected.GetComponent<Rigidbody>().velocity = Vector3.zero;
                selector.lastSelected.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            }

        }
        else
        {
            sideMovementLocked = false;
        }

    }

    private void MoveObjectToRay(RaycastHit hit)
    {
        try
        {
            if (hit.point != null && hit.transform.CompareTag("Floor"))
            {
                Rigidbody rb = selector.lastSelected.gameObject.GetComponent<Rigidbody>();
                rb.MovePosition(Vector3.Lerp(selector.lastSelected.transform.position, new Vector3(hit.point.x, selector.lastSelected.transform.position.y, hit.point.z), Time.deltaTime));

            }
        }
        catch (NullReferenceException e)
        {
            //Debug.Log(e);
            handLastPosition = raycaster.rightAttachmentPoint.transform.position;
        }

    }
}
