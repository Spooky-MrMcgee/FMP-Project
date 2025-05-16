using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPTransition : MonoBehaviour, IInteractable
{
    public GameObject cameraPerspective;
    public bool isOrthographic;
    public float orthographicPerspective;
    public bool flashlight;
    public bool transition;
    public void Interact()
    {
        StartCoroutine(EnterFirstPersonTransition());
    }

    public IEnumerator EnterFirstPersonTransition()
    {
        if (transition)
        {
            UIHandler.Instance.transition = true;
            yield return new WaitForSeconds(2f);
        }
        PlayerInteraction.Instance.itemBeingInteracted = this.gameObject;
        PlayerManager.Instance.firstPerson = this;
        yield return null;
    }

    public IEnumerator ExitFirstPersonTransition()
    {
        if (transition)
        {
            UIHandler.Instance.transition = true;
            yield return new WaitForSeconds(2f);
        }
        PlayerInteraction.Instance.itemBeingInteracted = null;
        PlayerManager.Instance.firstPerson = null;
        PlayerManager.Instance.firstPersonFinished = true;
        yield return null;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            if (Physics.Raycast(PlayerManager.Instance.flashlight.transform.position, PlayerManager.Instance.flashlight.transform.forward, out hit, 30f))
            {
                if (hit.transform.GetComponent<InteractableScript>())
                {

                    InteractableScript interactable = hit.transform.GetComponent<InteractableScript>();
                    if (!PlayerInteraction.Instance.currentlyInteracting)
                    {
                        PlayerInteraction.Instance.itemBeingInteracted = hit.transform.gameObject;
                        PlayerInteraction.Instance.currentlyInteracting = true;
                    }
                    if (PlayerInteraction.Instance.currentlyInteracting && PlayerInteraction.Instance.itemBeingInteracted == hit.transform.gameObject)
                    {
                        interactable.Interact();
                    }
                }
            }
        }
    }
}
