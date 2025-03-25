using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    Camera cam;
    [SerializeField] TextMeshProUGUI menuName, enterButton, dialogueText, endingText;
    [SerializeField] UnityEngine.UI.Image panel;
    [SerializeField] AudioSource talkingSFX;
    [SerializeField] GameObject speechBubble;
    [SerializeField] List<string> dialogue = new List<string>();
    int lineCount = 0;
    bool continueText, dialogueStarted, dialogueFinished;
    PlayerInputs PlayerActions;
    float time;
    Color panelColour, textColour;
    private void Awake()
    {
        PlayerActions = new PlayerInputs();
        PlayerActions.Cutscene.Continue.performed += AdvanceDialogue;
        PlayerActions.Enable();
    }

    private void Update()
    {
        if (dialogueFinished)
        {
            FinishIntro();
        }
    }

    public void StartGame()
    {
        menuName.enabled = false;
        enterButton.gameObject.SetActive(false);
        cam = Camera.main;
        cam.GetComponent<Animation>().Play();
    }

    public void StartDialogue()
    {
        speechBubble.SetActive(true);
        dialogueStarted = true;
        dialogueText.text = dialogue[0];
        lineCount++;
        talkingSFX.Play();
    }

    void AdvanceDialogue(InputAction.CallbackContext ctx)
    {
        if (!dialogueStarted)
            return;

        if (lineCount < dialogue.Count)
        {
            talkingSFX.Play();
            dialogueText.text = dialogue[lineCount];
            StartCoroutine(TextDelay());
        }
        else
        {
            panel.gameObject.SetActive(true);
            dialogueFinished = true;
            StartCoroutine(SceneTransition());
        }
    }

    void FinishIntro()
    {
        time += Time.deltaTime / 2;
        panelColour = panel.color;
        textColour = endingText.color;
        panelColour.a = time;
        textColour.a = time;
        panel.color = panelColour;
        endingText.color = textColour;
    }
    IEnumerator TextDelay()
    {
        continueText = false;
        yield return new WaitForSeconds(0.1f);
        lineCount++;
        continueText = true;
    }

    IEnumerator SceneTransition()
    {
        yield return new WaitForSeconds(5f);
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.buildIndex + 1);
    }
}
