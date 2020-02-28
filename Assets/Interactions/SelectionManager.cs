using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class SelectionManager : MonoBehaviour
{
    //todo handedness
    public GameObject rightAttachmentPoint;
    public Hand rightHand;

    [Range(0.0f, 1.0f)]
    public float selectionPercentage; // selection error threshold, the higher the more precise but harder to select
    private float lookPercentage; 

    [SerializeField] private Transform lastSelected;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = new Ray(rightAttachmentPoint.transform.position, rightAttachmentPoint.transform.forward);
        RaycastHit hit;

        if (lastSelected != null)
        {
            // prevents accidental deselection
            Vector3 vector1 = ray.direction;
            Vector3 vector2 = lastSelected.position - ray.origin;

            lookPercentage = Vector3.Dot(vector1.normalized, vector2.normalized);

            if (lookPercentage < selectionPercentage)
            {
                Interactable selectionInteractable = lastSelected.GetComponent<Interactable>();
                selectionInteractable.OnHandHoverEnd(rightHand);
                lastSelected = null;
            }
        }

        if (Physics.Raycast(ray, out hit) && lastSelected == null) // only select once
        {
            Transform selection = hit.transform;
            Interactable selectionInteractable = selection.GetComponent<Interactable>();
            if (selectionInteractable != null) // takes care of not selecting anything that is not interactable and highlights at the same time
            {
                selectionInteractable.OnHandHoverBegin(rightHand);
                lastSelected = selection;
            }
        }
        
    }

}
