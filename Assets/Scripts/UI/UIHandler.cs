using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class UIHandler : MonoBehaviour
{

    public static UIHandler Instance;
    [SerializeField] Sprite[] spriteSheet;
    [SerializeField] RawImage rectangleReticle;
    [SerializeField] Image targetReticle;
    [SerializeField] TextMeshProUGUI UIText;
    [SerializeField] Canvas UICanvas;
    GameObject currentTarget;
    bool lockedOn;
    bool textFinished;
    bool textDisplayed;
    bool continueText;
    [SerializeField] int lineCount;
    [SerializeField] List<string> currentText;
    public event Action FinishInteracted;
    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        PlayerInteraction.Instance.InteractableInRange += DisplayItemUI;
        PlayerInteraction.Instance.TextInteractable += DisplayText;
        PlayerCombat.Instance.AimingAtEnemy += DisplayTargetReticle;
    }

    private void Update()
    {
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

        if (textDisplayed && !textFinished && Input.GetKeyDown(KeyCode.Space))
        {
            DisplayText(currentText);
        }

    }

    private void OnDisable()
    {
        PlayerInteraction.Instance.InteractableInRange -= DisplayItemUI;
        PlayerCombat.Instance.AimingAtEnemy -= DisplayTargetReticle;
    }

    void DisplayItemUI(Vector3 itemPosition)
    {
        Debug.Log("Item UI is being displayed here.");
    }

    void DisplayText(List<string> text)
    {
        if (!textDisplayed)
        {
            currentText.AddRange(text);
            UIText.enabled = true;
            UIText.text = currentText[lineCount];
            StartCoroutine(TextDelay());
        }

        if (textDisplayed && continueText)
        {
            if (lineCount < currentText.Count)
            {
                UIText.text = currentText[lineCount];
                StartCoroutine(TextDelay());
            }
            else
            {
                Debug.Log("Text should be finished");
                textFinished = true;
                HideText();
            }
        }
    }

    IEnumerator TextDelay()
    {
        continueText = false;
        yield return new WaitForSeconds(0.1f);
        lineCount++;
        continueText = true;
        textDisplayed = true;
    }

    IEnumerator InteractDelay()
    {
        yield return new WaitForSeconds(0.1f);
        textFinished = true;
    }

    void HideText()
    {
        Debug.Log("Done!");
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
        Cursor.visible = false;
        rectangleReticle.enabled = true;
        rectangleReticle.transform.position = new Vector2(Input.mousePosition.x + 30, Input.mousePosition.y - 30);
    }    

    void HideReticle()
    {
        Cursor.visible = true;
        rectangleReticle.enabled = false;
        HideTargetReticle();
    }

    void HideTargetReticle()
    {
        targetReticle.enabled = false;
    }

    void DisplayTargetReticle(GameObject reticleTarget)
    {
        RectTransform canvasRect = UICanvas.GetComponent<RectTransform>();
        Vector2 viewportPosition = Camera.main.WorldToViewportPoint(reticleTarget.transform.position);
        Vector2 ObjectToScreenPos = new Vector2((viewportPosition.x * canvasRect.sizeDelta.x) - (canvasRect.sizeDelta.x * 0.5f), (viewportPosition.y * canvasRect.sizeDelta.y) - (canvasRect.sizeDelta.y * 0.5f));
        targetReticle.enabled = true;
        targetReticle.rectTransform.anchoredPosition = ObjectToScreenPos;
        ReticleFocus(targetReticle);
    }

    void ReticleFocus(Image reticle)
    {
        if (PlayerCombat.Instance.focusTime <= 0)
            PlayerCombat.Instance.focusTime = 0;
        reticle.sprite = spriteSheet[(int)Math.Round(PlayerCombat.Instance.focusTime)];
    }
}
