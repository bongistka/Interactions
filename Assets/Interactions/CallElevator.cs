using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

[RequireComponent(typeof(Interactable))]
public class CallElevator : MonoBehaviour
{
    public bool isUpper;
    public Elevator elevator;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void HandHoverUpdate(Hand hand)
    {
        if (SteamVR_Actions._default.GrabPinch.GetStateDown(hand.handType))
        {
            Debug.Log("Button " + this.gameObject.name + " was pressed");
            if(elevator.isUp != isUpper)
            {
                Debug.Log("Calling elevator up " + isUpper);
                StartCoroutine("DoCallElevator");
            }
        }
    }

    IEnumerator DoCallElevator()
    {
        if (isUpper)
        {
            Debug.Log("Elevator movement started");
            while (elevator.elevatorMesh.transform.position.y > elevator.difference.y)
            {
                elevator.elevatorMesh.transform.position -= elevator.difference / elevator.timeToGoUp;
                Debug.Log("Elevator moved to " + elevator.elevatorMesh.transform.position);
                yield return new WaitForEndOfFrame();
            }
            elevator.isUp = true;
        }
        else
        {
            while (elevator.elevatorMesh.transform.position.y < 0)
            {
                elevator.elevatorMesh.transform.position += elevator.difference / elevator.timeToGoUp;
                yield return new WaitForEndOfFrame();
            }
            elevator.isUp = false;
        }
    }
}
