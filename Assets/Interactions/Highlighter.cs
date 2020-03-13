using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

[RequireComponent(typeof(TeleportPoint))]
public class Highlighter : MonoBehaviour
{
    private TeleportPoint thisTeleportPoint;
    [Tooltip("If one directional, this is ignored")]
    public TeleportPoint otherTeleportPoint;
    public bool oneDirectional;
    private bool playerNearby;

    // Start is called before the first frame update
    void Start()
    {
        thisTeleportPoint = GetComponent<TeleportPoint>();
    }

    // Update is called once per frame
    void Update()
    {
        if (playerNearby)
        {
            thisTeleportPoint.Highlight(true);
            if (!oneDirectional)
            {
                otherTeleportPoint.Highlight(true);
            }
        } 
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerNearby = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerNearby = false;
            thisTeleportPoint.Highlight(false);
            if (!oneDirectional)
            {
                otherTeleportPoint.Highlight(false);
            }
        }
    }
}
