using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

[RequireComponent(typeof(Selector), typeof(Raycaster))]
public class MoveObject : MonoBehaviour
{
    //private Hand.AttachmentFlags attachmentFlags = Hand.defaultAttachmentFlags & (~Hand.AttachmentFlags.SnapOnAttach) & (~Hand.AttachmentFlags.DetachOthers) & (~Hand.AttachmentFlags.VelocityMovement);
    private Selector selector;
    private Raycaster raycaster;

    // Start is called before the first frame update
    void Start()
    {
        selector = GetComponent<Selector>();
        raycaster = GetComponent<Raycaster>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        LockSelection();
        if (selector.selectionLocked)
        {
            DoMoveObject();
            //DoRotateObject();
        }
        else
        {
            DoReleaseObject();
        }
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

    private void DoReleaseObject()
    {
        if (selector.lastSelected != null)
        {
            selector.lastSelected.GetComponent<Rigidbody>().useGravity = true;
            selector.lastSelected.parent = null;
        }    
    }

    private void DoMoveObject()
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

        if (selector.lastSelected != null)
        {
            selector.lastSelected.GetComponent<Rigidbody>().useGravity = false;
            selector.lastSelected.parent = raycaster.rightAttachmentPoint.transform;
        }
        
    }

    private void LockSelection()
    {
        if (SteamVR_Actions._default.GrabPinch.GetStateDown(selector.rightHand.handType))
        {
            selector.selectionLocked = true;
        }

        if (SteamVR_Actions._default.GrabPinch.GetStateUp(selector.rightHand.handType))
        {
            selector.selectionLocked = false;
        }
    }
}
