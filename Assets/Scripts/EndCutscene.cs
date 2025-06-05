using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System;

public class EndCutscene : MonoBehaviour
{
    [SerializeField] GameObject motherTextbox, sonTextbox;
    [SerializeField] TextMeshProUGUI motherText, sonText;
    PlayerInputs PlayerActions;
    bool startDialogue = false;
    bool continueText;
    [SerializeField] GameObject endObject;
    int lineCount;
    [SerializeField] AudioSource talkingSFX;

    [SerializeField] List<String> dialogue = new List<String>();

    void Awake()
    {
        PlayerActions = new PlayerInputs();
        PlayerActions.Cutscene.Continue.performed += AdvanceDialogue;
        PlayerActions.Enable();
        StartCoroutine(CutsceneDelay());
    }

    IEnumerator CutsceneDelay()
    {
        yield return new WaitForSeconds(11f);
        StartDialogue();
    }

    public void StartDialogue()
    {
        // Similar dialogue handler to the regular scripts, just flavoured a little differently.
        sonTextbox.SetActive(true);
        startDialogue = true;
        sonText.text = dialogue[0];
        lineCount++;
        talkingSFX.Play();
    }

    void AdvanceDialogue(InputAction.CallbackContext ctx)
    {
        if (!startDialogue)
            return;

        if (lineCount < dialogue.Count)
        {
            talkingSFX.Play();
            if (dialogue[lineCount].StartsWith("m"))
            {
                string line = dialogue[lineCount].Substring(1);
                motherTextbox.SetActive(true);
                sonTextbox.SetActive(false);
                motherText.text = line;
            }
            else if (dialogue[lineCount].StartsWith("s"))
            {
                string line = dialogue[lineCount].Substring(1);
                motherTextbox.SetActive(false);
                sonTextbox.SetActive(true);
                sonText.text = line;
            }
            StartCoroutine(TextDelay());
        }
        else
        {
            FinishEnd();
        }
    }

    void FinishEnd()
    {
        endObject.SetActive(true);
    }

    IEnumerator TextDelay()
    {
        continueText = false;
        yield return new WaitForSeconds(0.1f);
        lineCount++;
        continueText = true;
    }
}
