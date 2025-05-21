using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
public class UIHandler : MonoBehaviour
{

    public static UIHandler Instance;
    [Header("UI Objects")]
    [SerializeField] Sprite[] spriteSheet;
    [SerializeField] Image rectangleReticle;
    [SerializeField] Image targetReticle;
    [SerializeField] TextMeshProUGUI UIText;
    [SerializeField] Canvas UICanvas;
    [SerializeField] Image door;
    [SerializeField] Image transitionPanel;
    [SerializeField] int lineCount;
    [SerializeField] List<string> currentText;
    [SerializeField] Image itemPopUp;
    [SerializeField] Image puzzlePopUp;
    [SerializeField] Sprite[] itemPopUpSprite;

    [Header("UI Checks")]
    public bool transition;
    bool reverseTransition = false;
    float time;
    GameObject currentTarget;
    bool lockedOn;
    bool textFinished;
    public bool textDisplayed { get; private set; }
    bool continueText;
    bool itemUIDisplayed;
    [SerializeField] GameObject currentItem;
    public event Action FinishInteracted;
    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Subscribing to all necessary events
        PlayerInteraction.Instance.InteractableInRange += DisplayItemUI;
        PlayerInteraction.Instance.InteractableNoLongerInRange += HideItemUI;
        PlayerInteraction.Instance.TextInteractable += DisplayText;
        PlayerCombat.Instance.AimingAtEnemy += DisplayTargetReticle;
    }

    private void OnDisable()
    {
        PlayerInteraction.Instance.InteractableInRange -= DisplayItemUI;
        PlayerCombat.Instance.AimingAtEnemy -= DisplayTargetReticle;
    }

    private void Update()
    {
        // Handles all the necessary UI that updates in real time, such as player aiming and text updates.
        if (PlayerCombat.Instance.currentlyAiming)
        {
            DisplayReticle();
            lockedOn = true;
        }
        else
        {
            lockedOn = false;
            targetReticle.sprite = spriteSheet[spriteSheet.Length - 1];
            HideReticle();
        }

        if (!PlayerCombat.Instance.aimingAtEnemy)
            HideTargetReticle();

        if (textDisplayed && !textFinished && PlayerManager.Instance.PlayerActions.Player.PlayerInteract.WasPerformedThisFrame())
            DisplayText(currentText);

        if (transition == true)
            PanelTransition();

        if (PlayerMovement.PlayerMove.nextToDoor)
            door.enabled = true;
        else
            door.enabled = false;

    }


    void DisplayItemUI(GameObject itemObject)
    {
        RectTransform canvasRect = UICanvas.GetComponent<RectTransform>();
        currentItem = itemObject;
        if (itemObject.GetComponent<InteractableScript>() == null)
            itemObject = itemObject.transform.parent.gameObject;
       
        if (itemObject.GetComponent<InteractableScript>().collectable)
        {
            itemPopUp.enabled = true;
            Vector2 popUpPosition = Camera.main.WorldToViewportPoint(currentItem.transform.position);
            Vector2 objectToScreenPos = new Vector2((popUpPosition.x * canvasRect.sizeDelta.x) - (canvasRect.sizeDelta.x * 0.5f), (popUpPosition.y * canvasRect.sizeDelta.y) - (canvasRect.sizeDelta.y * 0.5f));
            itemPopUp.rectTransform.anchoredPosition = objectToScreenPos;
        }
        else if (itemObject.GetComponent<PuzzleInteractable>())
        {
            puzzlePopUp.enabled = true;
            Vector2 popUpPosition = Camera.main.WorldToViewportPoint(currentItem.transform.position);
            Vector2 objectToScreenPos = new Vector2((popUpPosition.x * canvasRect.sizeDelta.x) - (canvasRect.sizeDelta.x * 0.5f), (popUpPosition.y * canvasRect.sizeDelta.y) - (canvasRect.sizeDelta.y * 0.5f));
            puzzlePopUp.rectTransform.anchoredPosition = objectToScreenPos;
        }
        currentItem = itemObject;
    }

    void HideItemUI(GameObject currentItemToHide)
    {
        if (currentItem == currentItemToHide)
        {
            itemPopUp.enabled = false;
            puzzlePopUp.enabled = false;
        }

    }

    void DisplayText(List<string> text)
    {
        // Takes a queue of strings based on the variable text and filters through them as the player proceeds through text dialogue.
        if (!textDisplayed)
        {
            currentText.AddRange(text);
            UIText.enabled = true;
            UIText.text = currentText[lineCount];
            StartCoroutine(TextDelay());
            AudioManager.Instance.PlaySFX("MainCharacterTalking");
        }

        if (textDisplayed && continueText)
        {
            if (lineCount < currentText.Count)
            {
                UIText.text = currentText[lineCount];
                StartCoroutine(TextDelay());
                AudioManager.Instance.PlaySFX("MainCharacterTalking");
            }
            else
            {
                textFinished = true;
                HideText();
            }
        }
    }

    public void PanelTransition()
    {
        // If a specific puzzle or event needs a fadein/fadeout effect then PanelTransition is called.
        Color panelColour = transitionPanel.color;

        if (transitionPanel.color.a < 0)
        {
            time = 0;
            panelColour.a = 0;
            transitionPanel.color = panelColour;
            transition = false;
            reverseTransition = false;
            return;
        }

        if (transitionPanel.color.a > 1)
            reverseTransition = true;

        if (reverseTransition)
            time -= Time.deltaTime / 2;
        else
            time += Time.deltaTime / 2;
        
        panelColour.a = time;
        transitionPanel.color = panelColour;
    }

    IEnumerator TextDelay()
    {
        // TextDelay occurs so the player cannot instantly skip through all dialogue.
        continueText = false;
        yield return new WaitForSeconds(0.1f);
        lineCount++;
        continueText = true;
        textDisplayed = true;
    }

    IEnumerator InteractDelay()
    {
        // InteractDelay does much the same so the player isn't stuck infinitely interacting with an object.
        yield return new WaitForSeconds(0.1f);
        textFinished = true;
    }

    void HideText()
    {
        // HideText simply hides the displayables after an object is finished being interacted with.
        UIText.text = "";
        UIText.enabled = false;
        textDisplayed = false;
        textFinished = false;
        lineCount = 0;
        currentText.Clear();
        FinishInteracted?.Invoke();
    }

    void DisplayReticle()
    {
        // Displays the target reticle when the player is aiming.
        Cursor.visible = false;
        rectangleReticle.enabled = true;
        rectangleReticle.transform.position = new Vector2(Input.mousePosition.x + 30, Input.mousePosition.y - 30);
    }    

    void HideReticle()
    {
        // Hides the reticle on an enemy when the player moves away/stops aiming.
        Cursor.visible = true;
        rectangleReticle.enabled = false;
        HideTargetReticle();
    }

    void HideTargetReticle()
    {
        // Hides the aiming reticle that follows the players mouse.
        targetReticle.enabled = false;
    }

    void DisplayTargetReticle(GameObject reticleTarget)
    {
        // Displays the aiming reticle that follows the players mouse.
        RectTransform canvasRect = UICanvas.GetComponent<RectTransform>();
        Vector2 viewportPosition = Camera.main.WorldToViewportPoint(reticleTarget.transform.position);
        Vector2 objectToScreenPos = new Vector2((viewportPosition.x * canvasRect.sizeDelta.x) - (canvasRect.sizeDelta.x * 0.5f), (viewportPosition.y * canvasRect.sizeDelta.y) - (canvasRect.sizeDelta.y * 0.5f));
        targetReticle.enabled = true;
        targetReticle.rectTransform.anchoredPosition = objectToScreenPos;
        ReticleFocus(targetReticle);
    }

    void ReticleFocus(Image reticle)
    {
        // Cycles through a sprite sheet in order to highlight the player 'locking in' on an enemy.
        if (PlayerCombat.Instance.focusTime <= 0)
            PlayerCombat.Instance.focusTime = 0;
        reticle.sprite = spriteSheet[(int)Math.Round(PlayerCombat.Instance.focusTime)];
    }
}
