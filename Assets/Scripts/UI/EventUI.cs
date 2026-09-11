using TMPro;
using UnityEngine;

public class EventUI : MonoBehaviour
{
    [Header("Application")]
    [SerializeField] private GameObject applicationPanel;

    [Header("Event Preview")]
    [SerializeField] private TMP_Text eventPreviewTitle;

    [Header("Event Window")]
    [SerializeField] private GameObject eventWindow;
    [SerializeField] private TMP_Text eventTitleText;
    [SerializeField] private TMP_Text eventDescriptionText;
    [SerializeField] private TMP_Text hintText;

    private MarketEventData currentEvent;
    private bool hasHint;
    private CurveMovement hintMovement;
    private string currentHintText;

    private void Start()
    {
        applicationPanel.SetActive(false);
    }

    public void SetEvent(MarketEventData marketEvent, bool newHasHint, CurveMovement newHintMovement)
    {
        currentEvent = marketEvent;
        hasHint = newHasHint;
        hintMovement = newHintMovement;

        if (currentEvent == null) return;

        if (eventPreviewTitle != null)
            eventPreviewTitle.text = currentEvent.eventTitle;

        if (eventTitleText != null)
            eventTitleText.text = currentEvent.eventTitle;

        if (eventDescriptionText != null)
            eventDescriptionText.text = currentEvent.eventDescription;

        RefreshHint();
    }

    private void RefreshHint()
    {
        if (hintText == null || currentEvent == null) return;

        hintText.gameObject.SetActive(true);

        if (!hasHint)
        {
            hintText.text = "";
            return;
        }

        hintText.text = currentEvent.hintText;
    }

    public void OpenEventWindow()
    {
        if (applicationPanel == null || eventWindow == null || currentEvent == null)
            return;

        applicationPanel.SetActive(true);
        eventWindow.SetActive(true);
    }

    public void CloseEventWindow()
    {
        if (eventWindow != null)
            eventWindow.SetActive(false);

        if (applicationPanel != null)
            applicationPanel.SetActive(false);
    }

    public void ToggleEventWindow()
    {
        if (applicationPanel == null || eventWindow == null || currentEvent == null)
            return;

        if (applicationPanel.activeSelf)
            CloseEventWindow();
        else
            OpenEventWindow();
    }
}