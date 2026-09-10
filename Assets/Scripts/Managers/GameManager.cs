using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private bool _isSellingMode;
    [SerializeField] private List<CanvasGroup> _sellingModeCanvasGroup;
    private int _sellableUILayer;
    [SerializeField] private Texture2D _sellCursorTexture;
    [SerializeField] private Texture2D _CursorTexture;
    [SerializeField] private BuyableData _buyableData;
    private GameObject _currentSellableObject;

    [SerializeField] private Slider _moneySlider;

    [Header("Market Events")]
    [SerializeField] private List<MarketEventData> _marketEvents;

    [Header("Game Data")]
    [SerializeField] private GameData gameData = new GameData();

    [Header("Mail")]
    [SerializeField] private GameObject mailNotificationIcon;
    [SerializeField] private Texture2D mailNotificationIconMom;
    [SerializeField] private Texture2D mailNotificationIconBoss;
    [SerializeField] private Texture2D mailNotificationIconStory;
    [SerializeField] private GameObject mailZone;
    [SerializeField] private GameObject mailTitle;
    [SerializeField] private GameObject mailText;
    [SerializeField] private GameObject oneButton;
    [SerializeField] private GameObject twoButton;

    [Header("Confirmation Pop Up")]
    [SerializeField] private GameObject confirmationPopUp;
    [SerializeField] private GameObject itemToSell;

    public GameData Data => gameData;
    public bool IsGameOver { get; private set; }

    private readonly Dictionary<MarketEventData, EventProbabilities> _originalEventProbabilities = new Dictionary<MarketEventData, EventProbabilities>();

    private struct EventProbabilities
    {
        public int strongUp;
        public int up;
        public int stable;
        public int down;
        public int strongDown;

        public EventProbabilities(MarketEventData marketEvent)
        {
            strongUp = marketEvent.strongUpProbability;
            up = marketEvent.upProbability;
            stable = marketEvent.stableProbability;
            down = marketEvent.downProbability;
            strongDown = marketEvent.strongDownProbability;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    private void Start()
    {
        _sellableUILayer = LayerMask.NameToLayer("SellableUI");
        Cursor.SetCursor(_CursorTexture, Vector2.zero, CursorMode.Auto);
        _moneySlider.value = gameData.currentMoney;
        SaveOriginalEventProbabilities();
    }

    public void ToggleSellingMode()
    {
        _isSellingMode = !_isSellingMode;
        foreach (var canvasGroup in _sellingModeCanvasGroup)
        {
            canvasGroup.interactable = !_isSellingMode;
        }
        Cursor.SetCursor(_isSellingMode ? _sellCursorTexture : _CursorTexture, Vector2.zero, CursorMode.Auto);
    }
    private void Update()
    {
        if (_isSellingMode)
        {
            if (IsPointerOverUIElement(GetEventSystemRaycastResults()))
            {
                Debug.Log("It's over UI elements");
                Cursor.SetCursor(_sellCursorTexture, Vector2.zero, CursorMode.Auto);

                if (Mouse.current.leftButton.wasPressedThisFrame)
                { 
                    if( (_buyableData.data.Find(x => x.name == _currentSellableObject.name).needConfirmation))
                        {
                        Debug.Log("Need confirmation for: " + _currentSellableObject.name);
                        confirmationPopUp.SetActive(true);
                        itemToSell = _currentSellableObject;
                        ToggleSellingMode();
                    }
                    else
                    {
                        Debug.Log("No confirmation needed for: " + _currentSellableObject.name);
                        _buyableData.data.Find(x => x.name == _currentSellableObject.name).isBuyable = true;
                        AddMoney(_buyableData.data.Find(x => x.name == _currentSellableObject.name).value);
                        _currentSellableObject.SetActive(false);
                        Data.soldUICount++;
                        Debug.Log("Clicked on: " + _currentSellableObject.name);
                    }
                }
            }
            else
            {
                Debug.Log("It's NOT over UI elements");
                Cursor.SetCursor(_CursorTexture, Vector2.zero, CursorMode.Auto);
            }
        }
    }
    public void ConfirmSell()
    {
        if (itemToSell != null)
        {
            _buyableData.data.Find(x => x.name == itemToSell.name).isBuyable = true;
            AddMoney(_buyableData.data.Find(x => x.name == itemToSell.name).value);
            itemToSell.SetActive(false);
            Data.soldUICount++;
            Debug.Log("Confirmed sell for: " + itemToSell.name);
            itemToSell = null;
        }
        ToggleSellingMode();
        confirmationPopUp.SetActive(false);
    }
    public void AddMoney(int amount)
    {
        if (amount <= 0) return;
        gameData.currentMoney += amount;
        _moneySlider.value = gameData.currentMoney;
    }

    public void RemoveMoney(int amount)
    {
        if (amount <= 0) return;
        gameData.currentMoney = Mathf.Max(0, gameData.currentMoney - amount);
    }

    public void NextDay()
    {
        if (IsGameOver) return;

        gameData.wasMailOpened = false;
        gameData.currentDay++;

        if (gameData.currentDay == gameData.nextMailDay)
        {
            if (gameData.nextMail.mailType == MailData.MailType.Mom)
                mailNotificationIcon.GetComponent<RawImage>().texture = mailNotificationIconMom;
            else if (gameData.nextMail.mailType == MailData.MailType.Boss)
                mailNotificationIcon.GetComponent<RawImage>().texture = mailNotificationIconBoss;
            else if (gameData.nextMail.mailType == MailData.MailType.Story)
                mailNotificationIcon.GetComponent<RawImage>().texture = mailNotificationIconStory;
        }
        bool soldUIEffectActive = gameData.soldUICount > 0;

        if (gameData.soldUICount > 0)
            gameData.soldUICount--;

        if (soldUIEffectActive)
            SetEqualEventProbabilities();
        else
            RestoreOriginalEventProbabilities();

        if (gameData.currentDay > gameData.totalDays)
        {
            EndGame();
        }
    }

    private void SaveOriginalEventProbabilities()
    {
        _originalEventProbabilities.Clear();

        foreach (MarketEventData marketEvent in _marketEvents)
        {
            if (marketEvent == null) continue;

            _originalEventProbabilities[marketEvent] = new EventProbabilities(marketEvent);
        }
    }

    private void SetEqualEventProbabilities()
    {
        foreach (MarketEventData marketEvent in _marketEvents)
        {
            if (marketEvent == null) continue;

            marketEvent.strongUpProbability = 20;
            marketEvent.upProbability = 20;
            marketEvent.stableProbability = 20;
            marketEvent.downProbability = 20;
            marketEvent.strongDownProbability = 20;
        }
    }

    private void RestoreOriginalEventProbabilities()
    {
        foreach (KeyValuePair<MarketEventData, EventProbabilities> entry in _originalEventProbabilities)
        {
            MarketEventData marketEvent = entry.Key;
            EventProbabilities probabilities = entry.Value;

            if (marketEvent == null) continue;

            marketEvent.strongUpProbability = probabilities.strongUp;
            marketEvent.upProbability = probabilities.up;
            marketEvent.stableProbability = probabilities.stable;
            marketEvent.downProbability = probabilities.down;
            marketEvent.strongDownProbability = probabilities.strongDown;
        }
    }

    private void EndGame()
    {
        IsGameOver = true;
        Debug.Log("===== FIN DE PARTIE =====");

        if (gameData.QuotaReached)
            Debug.Log("Victoire : quota atteint !");
        else
            Debug.Log("Défaite : quota non atteint.");
    }

    public void OpenMail()
    {
        if (gameData.currentDay != gameData.nextMailDay || gameData.wasMailOpened) return;

        gameData.wasMailOpened = true;

        mailZone.SetActive(true);
        mailTitle.GetComponent<TextMeshProUGUI>().text = gameData.nextMail.title;
        mailText.GetComponent<TextMeshProUGUI>().text = gameData.nextMail.content;
        if (gameData.nextMail.mailType == MailData.MailType.Story)
        {
            oneButton.SetActive(false);
            twoButton.SetActive(true);
            twoButton.GetComponentsInChildren<TextMeshProUGUI>()[1].text = gameData.nextMail.leftButtonText;
            twoButton.GetComponentsInChildren<TextMeshProUGUI>()[2].text = gameData.nextMail.rightButtonText;
        }
        else
        {
            oneButton.SetActive(true);
            twoButton.SetActive(false);
            oneButton.GetComponent<TextMeshProUGUI>().text = gameData.nextMail.leftButtonText;
        }
        gameData.nextMailDay = gameData.nextMail.nextMailDay;
    }

    public void CloseMailGood()
    {
        mailZone.SetActive(false);
        gameData.nextMail = gameData.nextMail.nextMailGood;
        Debug.Log("Good");
    }
    public void CloseMailBad()
    {
        mailZone.SetActive(false);
        gameData.nextMail = gameData.nextMail.nextMailBad;
        Debug.Log("Bad");
    }


    private bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults)
    {
        for (int index = 0; index < eventSystemRaysastResults.Count; index++)
        {
            RaycastResult curRaysastResult = eventSystemRaysastResults[index];
            if (curRaysastResult.gameObject.layer == _sellableUILayer)
            {
                _currentSellableObject = curRaysastResult.gameObject;
                return true;
            }
        }
        return false;
    }
    static List<RaycastResult> GetEventSystemRaycastResults()
    {
        Vector2 pointerPosition = Mouse.current.position.ReadValue();
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = pointerPosition;
        List<RaycastResult> raysastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raysastResults);
        return raysastResults;
    }
}

