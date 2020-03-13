using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class MovePlayer : MonoBehaviour
{
    public TeleportPoint thisTeleportPoint;
    public TeleportPoint otherTeleportPoint;
    public Teleport teleporting;
    public GameObject origin;
    public bool wasTeleported;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (!wasTeleported)
            {
                //Debug.Log("initiating teleport to " + otherTeleportPoint.title);
                teleporting.InitiateTeleportFade();
                StartCoroutine("MoveOrigin");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            wasTeleported = false;
        }
    }


    IEnumerator MoveOrigin()
     {
        yield return new WaitForSeconds(teleporting.teleportFadeTime);
        Vector3 difference = thisTeleportPoint.gameObject.transform.position - otherTeleportPoint.gameObject.transform.position;
        origin.transform.position += difference;
        otherTeleportPoint.gameObject.GetComponentInChildren<MovePlayer>().wasTeleported = true;
     }
}
