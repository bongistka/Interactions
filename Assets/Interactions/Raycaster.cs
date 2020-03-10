using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class Raycaster : MonoBehaviour
{
    public enum Handedness
    {
        right,
        left,
        both
    };
    public Handedness handedness;  // which hand is used for selecting (to prevent unnecessarry raycasting)

    [HideInInspector]
    public Ray ray;
    [HideInInspector]
    public GameObject rightAttachmentPoint;
    [HideInInspector]
    public GameObject leftAttachmentPoint;


    // Start is called before the first frame update
    void Awake()
    {
        rightAttachmentPoint = GameObject.FindGameObjectWithTag("ControllerRight").transform.parent.GetChild(1).gameObject;
        
        leftAttachmentPoint = GameObject.FindGameObjectWithTag("ControllerLeft").transform.parent.GetChild(1).gameObject;
        
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        ray = new Ray(rightAttachmentPoint.transform.position, rightAttachmentPoint.transform.forward);
    }
}
