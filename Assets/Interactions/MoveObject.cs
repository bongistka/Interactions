using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

[RequireComponent(typeof(Selector), typeof(Raycaster))]
public class MoveObject : MonoBehaviour
{
    private Selector selector;
    private Raycaster raycaster;
    // Start is called before the first frame update
    void Start()
    {
        selector = GetComponent<Selector>();
        raycaster = GetComponent<Raycaster>();
    }

    // Update is called once per frame
    void FixedUpdate
        ()
    {
        LockSelection();
        if (selector.selectionLocked)
        {
            DoMoveObject();
        }
    }

    private void DoMoveObject()
    {
        raycaster.rightAttachmentPoint.AddComponent<FixedJoint>();
        raycaster.rightAttachmentPoint.GetComponent<FixedJoint>().connectedBody = selector.lastSelected.GetComponent<Rigidbody>();
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
