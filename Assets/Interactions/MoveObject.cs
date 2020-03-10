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

    //private Hand.AttachmentFlags attachmentFlags = Hand.defaultAttachmentFlags & (~Hand.AttachmentFlags.SnapOnAttach) & (~Hand.AttachmentFlags.DetachOthers) & (~Hand.AttachmentFlags.VelocityMovement);
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
        LockSelection();
        if (selector.selectionLocked)
        {
            
            MoveObjectY();

            if (!sideMovementLocked)
            {
                MoveObjectToRay(selector.hit);
            }
            //DoRotateObject();
        }
        else
        {
            ReleaseObject();
        }
        handLastPosition = raycaster.rightAttachmentPoint.transform.position;
    }

    private void DoRotateObject()
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
            selector.lastSelected.GetComponent<Rigidbody>().useGravity = true;
            selector.lastSelected.parent = null;
        }    
    }

    private void MoveObjectY()
    {
        /*
        Transform transformOffsett = new GameObject().transform;
        transformOffsett.position = raycaster.rightAttachmentPoint.transform.position - selector.lastSelected.transform.position;
        transformOffsett.rotation = Quaternion.Inverse(raycaster.rightAttachmentPoint.transform.localRotation * selector.lastSelected.transform.localRotation);
        
        GrabTypes startingGrabType = selector.rightHand.GetGrabStarting();
        selector.rightHand.AttachObject(selector.lastSelected.gameObject, startingGrabType, attachmentFlags);
        
        raycaster.rightAttachmentPoint.AddComponent<Rigidbody>();
        raycaster.rightAttachmentPoint.AddComponent<FixedJoint>();
        raycaster.rightAttachmentPoint.GetComponent<FixedJoint>().connectedBody = selector.lastSelected.GetComponent<Rigidbody>();
        */

        

        // moves along Y axis if vertical hand motion detected
        Vector3 currentPosition = raycaster.rightAttachmentPoint.transform.position;
        Vector3 difference = currentPosition - handLastPosition;

        //Rigidbody rb = selector.lastSelected.gameObject.GetComponent<Rigidbody>();
        //rb.isKinematic = true;

        if ((Mathf.Abs(difference.x) < Mathf.Abs(difference.y)) && (Mathf.Abs(difference.z) < Mathf.Abs(difference.y)) && (Mathf.Abs(difference.y) > yMovementThreshold))
        {
            sideMovementLocked = true;

            //Vector3 objectPosition = lastSelected.gameObject.transform.position;
            //rb.MovePosition(new Vector3(objectPosition.x, objectPosition.y + difference.y * yMovementSpeed, objectPosition.z));
            //Debug.Log( "rb moved up");
            if (selector.lastSelected != null)
            {
                selector.lastSelected.GetComponent<Rigidbody>().useGravity = false;
                selector.lastSelected.parent = raycaster.rightAttachmentPoint.transform;
            }

        }
        else
        {
            //rb.isKinematic = false;
            sideMovementLocked = false;
            selector.lastSelected.GetComponent<Rigidbody>().useGravity = true;
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
            Debug.Log(e);
            handLastPosition = raycaster.rightAttachmentPoint.transform.position;
        }

    }

    private void LockSelection()
    {
        if (SteamVR_Actions._default.GrabPinch.GetStateDown(selector.rightHand.handType))
        {
            selector.lastSelected.gameObject.layer = 2;
            selector.selectionLocked = true;
        }

        if (SteamVR_Actions._default.GrabPinch.GetStateUp(selector.rightHand.handType))
        {
            selector.lastSelected.gameObject.layer = 0;
            selector.selectionLocked = false;           
        }
    }
}
