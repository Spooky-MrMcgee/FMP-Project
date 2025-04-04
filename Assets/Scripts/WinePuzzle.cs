 using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WinePuzzle : MonoBehaviour
{
    [Header("Objects and Checks")]
    [SerializeField] bool wineFridgeChecked;
    [SerializeField] bool keyFound;
    [SerializeField] public bool doorIsOpen;
    [SerializeField] public bool acidUsed;
    [SerializeField] bool bottleObtained;
    [SerializeField] public PuzzleInteractable wineFridge;
    [SerializeField] PuzzleInteractable trapDoor;
    [SerializeField] PuzzleInteractable updatedTrapDoor;
    [SerializeField] InteractableItem key;
    [SerializeField] InteractableItem medkit;
    [SerializeField] InteractableItem bottle;
    [SerializeField] GameObject wallHole;
    [SerializeField] GameObject trapDoorPerspective;
    [SerializeField] GameObject wineFridgeDoor;
    [SerializeField] GameObject useAcid;
    bool clickingUp, clickingDown;
    bool increaseTemp;

    [Header("Text Updates")]
    [SerializeField] List<string> updatedWineFridgeCheck = new List<string>();
    //[SerializeField] List<string> updatedDoorCheck = new List<string>();
    [SerializeField] List<string> updatedWineFridgeOpen = new List<string>();
    [SerializeField] List<string> updatedWineFridgeOpenCheck = new List<string>();
    [SerializeField] List<string> updatedWineFridgeAcid = new List<string>();

    [Header("Puzzle Updates")]
    [SerializeField] TextMeshProUGUI temperature;
    [SerializeField] Canvas temperatureCanvas;
    [SerializeField] float currentTemp;
    [SerializeField] float correctTemp;
    // Update is called once per frame

    private void Start()
    {
        useAcid.GetComponent<UseAcid>().usedAcid = false;
    }

    void Update()
    {
        // This script handles all of the necessary components for the wine puzzle, communicating with all relevant puzzle interactable objects to 
        // update the puzzle based on player progression.

        if (wineFridge.interacted || PlayerManager.Instance.firstPerson != null)
        {
            temperatureCanvas.enabled = true;
            temperature.text = currentTemp.ToString() + "°C";
        }
        else
            temperatureCanvas.enabled = false;

        if (wineFridge.interacted && !wineFridgeChecked)
        {
            wineFridgeChecked = true;
        }

        if (wineFridgeChecked && wallHole.activeSelf && keyFound == false)
        {
            if (PlayerManager.Instance.SearchInventory(key))
            {
                keyFound = true;
            }
        }

        if (wineFridgeChecked && !doorIsOpen)
        {
            wallHole.SetActive(true);
            wineFridge.interactText = updatedWineFridgeCheck;
        }

        if (keyFound)
        {
            trapDoor.interactText = updatedTrapDoor.interactText;
            trapDoor.cameraPerspective = updatedTrapDoor.cameraPerspective;
            trapDoor.causeTransition = updatedTrapDoor.causeTransition;
            trapDoor.fpTransition = updatedTrapDoor.fpTransition;
        }

        if (useAcid.GetComponent<UseAcid>().usedAcid && !bottleObtained)
        {
            if (PlayerInteraction.Instance.nearestInteractable == wineFridge && doorIsOpen)
            {
                acidUsed = true;
                PlayerInventory.Instance.DisplayInventory();
            }
        }

        if (acidUsed && !bottleObtained)
        {
            bottleObtained = true;
            wineFridge.interactText = updatedWineFridgeAcid;
            wineFridge.interactable = bottle;
            wineFridge.collectable = true;
            wineFridge.Interact();
        }
        if (clickingUp && increaseTemp)
            StartCoroutine(ShiftTemp(1));
        else if (clickingDown && increaseTemp)
            StartCoroutine(ShiftTemp(-1));

        if (!clickingUp && !clickingDown)
            StopCoroutine("ShiftTemp");

        if (currentTemp == correctTemp && doorIsOpen == false)
        {
            StopCoroutine("ShiftTemp");
            wineFridgeDoor.transform.localRotation = new Quaternion(wineFridgeDoor.transform.localRotation.x, wineFridgeDoor.transform.localRotation.y, 90, wineFridgeDoor.transform.localRotation.w);
            doorIsOpen = true;
            PlayerInteraction.Instance.itemBeingInteracted = wineFridge.gameObject;
            wineFridge.fpTransition = null;
            PlayerManager.Instance.firstPerson = null;
            wineFridge.interactText = updatedWineFridgeOpen;
            wineFridge.Interact();
        }

        if (doorIsOpen)
        {
            currentTemp = correctTemp;
            wineFridge.interactText = updatedWineFridgeOpenCheck;
        }
    }
   
    // The following scripts indicate whether the player is or isn't clicking on the wine puzzle buttons and shifts the temperature accordingly, using a coroutine to slow down the quickness of it.
    public void StopClicking()
    {
        clickingUp = false;
        clickingDown = false;
    }

    public void ShiftUp()
    {
        clickingUp = true;
        StartCoroutine(ShiftTemp(1));
    }

    public void ShiftDown()
    {
        clickingDown = true;
        StartCoroutine(ShiftTemp(-1));
    }

    IEnumerator ShiftTemp(float tempIncrease)
    {
        increaseTemp = false;
        yield return new WaitForSeconds(0.2f);
        currentTemp += tempIncrease;
        increaseTemp = true;
    }
}
