using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class Elevator : MonoBehaviour
{
    public GameObject origin;
    public GameObject elevatorMesh;
    public GameObject elevatorTarget;
    private bool wasLifted;
    [HideInInspector] public Vector3 difference;
    [HideInInspector] public bool isUp;
    public float timeToGoUp = 500.0f;

    // Start is called before the first frame update
    void Start()
    {
        difference = new Vector3 (0.0f ,this.gameObject.transform.position.y - elevatorTarget.gameObject.transform.position.y, 0.0f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (!wasLifted)
            {
                Debug.Log("initiating elevator");
                StartCoroutine("MoveElevator");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            wasLifted = false;
        }
    }


    IEnumerator MoveElevator()
    {
        this.gameObject.transform.parent.parent = null;
        elevatorMesh.transform.parent = null;
  
        if (!isUp)
        {
            while(origin.transform.position.y > difference.y)
            {
                origin.transform.position += difference/ timeToGoUp;
                yield return new WaitForEndOfFrame();
            }            
            isUp = true;
        }
        else
        {
            while (origin.transform.position.y < 0)
            {
                origin.transform.position -= difference / timeToGoUp;
                yield return new WaitForEndOfFrame();
            }
            isUp = false;
        }
        
        this.gameObject.transform.parent.parent = origin.transform;
        elevatorMesh.transform.parent = origin.transform;
    }
}
