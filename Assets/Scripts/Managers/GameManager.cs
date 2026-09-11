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

    [SerializeField] private MosquitoPopup mosquitoPopup;

    [Header("Market Events")]
    [SerializeField] private List<MarketEventData> _marketEvents;
    [SerializeField] private GameObject sellHalo;

    [Header("Game Data")]
    [SerializeField] private GameData gameData = new GameData();

    [Header("Mail")]
    [SerializeField] private GameObject mailNotificationIcon;
    [SerializeField] private Sprite mailNotificationIconBase;
    [SerializeField] private Sprite mailNotificationIconMom;
    [SerializeField] private Sprite mailNotificationIconBoss;
    [SerializeField] private Sprite mailNotificationIconStory;
    [SerializeField] private GameObject mailZone;
    [SerializeField] private GameObject mailTitle;
    [SerializeField] private GameObject mailText;
    [SerializeField] private GameObject oneButton;
    [SerializeField] private GameObject twoButton;

    [Header("Confirmation Pop Up")]
    [SerializeField] private GameObject confirmationPopUp;
    [SerializeField] private GameObject itemToSell;

    [Header("Calendar")]
    [SerializeField] private GameObject calendarZone;
    [SerializeField] private List<Sprite> calendarImages;

    [Header("Sons")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip vente;
    [SerializeField] private AudioClip notif;
    [SerializeField] private AudioClip goodProno;
    [SerializeField] private AudioClip badProno;
    [SerializeField] private AudioClip newDay;
    [SerializeField] private AudioClip music;


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

        if (_moneySlider != null)
        {
            _moneySlider.minValue = 0;
            _moneySlider.maxValue = gameData.moneyQuota;
            _moneySlider.value = gameData.moneyQuota - gameData.currentMoney;
        }
        SaveOriginalEventProbabilities();
    }

    public void ToggleSellingMode()
    {
        _isSellingMode = !_isSellingMode;
        sellHalo.SetActive(_isSellingMode);
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
                Cursor.SetCursor(_sellCursorTexture, Vector2.zero, CursorMode.Auto);

                if (Mouse.current.leftButton.wasPressedThisFrame)
                { 
                    if( (_buyableData.data.Find(x => x.name == _currentSellableObject.name).needConfirmation))
                        {
                        confirmationPopUp.SetActive(true);
                        itemToSell = _currentSellableObject;
                        ToggleSellingMode();
                    }
                    else
                    {
                        _buyableData.data.Find(x => x.name == _currentSellableObject.name).isBuyable = true;
                        AddMoney(_buyableData.data.Find(x => x.name == _currentSellableObject.name).value);
                        Destroy(_currentSellableObject);
                        Data.soldUICount++;
                        audioSource.PlayOneShot(vente);
                    }
                }
            }
            else
            {
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
            Destroy(itemToSell);
            Data.soldUICount++;
            itemToSell = null;
        }
        audioSource.PlayOneShot(vente);
        ToggleSellingMode();
        confirmationPopUp.SetActive(false);
    }
    public void AddMoney(int amount)
    {
        if (amount <= 0) return;

        gameData.currentMoney += amount;
        RefreshMoneySlider();
    }

    public void RemoveMoney(int amount)
    {
        if (amount <= 0) return;

        gameData.currentMoney = Mathf.Max(0, gameData.currentMoney - amount);
        RefreshMoneySlider();
    }

    private void RefreshMoneySlider()
    {
        if (_moneySlider == null) return;

        _moneySlider.maxValue = gameData.moneyQuota;
        _moneySlider.value = gameData.moneyQuota - gameData.currentMoney;
    }

    public void NextDay()
    {
        if (IsGameOver) return;

        calendarZone.GetComponent<Image>().sprite = calendarImages[gameData.currentDay - 1];

        gameData.wasMailOpened = false;
        gameData.currentDay++;

        if (gameData.currentDay == 12 && mosquitoPopup != null)
        {
            mosquitoPopup.OpenPopup();
        }

        if (gameData.currentDay == gameData.nextMailDay)
        {
            audioSource.PlayOneShot(notif);
            Debug.Log("Mail received: " + gameData.nextMail.mailType);
            if (gameData.nextMail.mailType == MailData.MailType.Mom)
                mailNotificationIcon.GetComponent<Image>().sprite = mailNotificationIconMom;
            else if (gameData.nextMail.mailType == MailData.MailType.Boss)
                mailNotificationIcon.GetComponent<Image>().sprite = mailNotificationIconBoss;
            else if (gameData.nextMail.mailType == MailData.MailType.Story)
                mailNotificationIcon.GetComponent<Image>().sprite = mailNotificationIconStory;
        }
        bool soldUIEffectActive = gameData.soldUICount > 0;

        if (gameData.soldUICount > 0)
            gameData.soldUICount--;

        if (soldUIEffectActive)
            SetEqualEventProbabilities();
        else
            RestoreOriginalEventProbabilities();
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
        if (gameData.currentDay != gameData.nextMailDay || gameData.wasMailOpened)
        {
            return;
        }

        gameData.wasMailOpened = true;

        mailZone.SetActive(true);
        mailTitle.GetComponent<TextMeshProUGUI>().text = gameData.nextMail.title;
        mailText.GetComponent<TextMeshProUGUI>().text = gameData.nextMail.content;
        if (gameData.nextMail.mailType == MailData.MailType.Story)
        {
            oneButton.SetActive(false);
            twoButton.SetActive(true);
            twoButton.GetComponentsInChildren<TextMeshProUGUI>()[0].text = gameData.nextMail.leftButtonText;
            twoButton.GetComponentsInChildren<TextMeshProUGUI>()[1].text = gameData.nextMail.rightButtonText;
        }
        else
        {
            oneButton.SetActive(true);
            twoButton.SetActive(false);
            oneButton.GetComponentInChildren<TextMeshProUGUI>().text = gameData.nextMail.leftButtonText;
        }
    }

    public void CloseMailGood()
    {
        mailZone.SetActive(false);
        mailNotificationIcon.GetComponent<Image>().sprite = mailNotificationIconBase;
        AddMoney(gameData.nextMail.leftButtonMoney);
        gameData.karma += gameData.nextMail.leftButtonKarma;
        gameData.nextMailDay = gameData.nextMail.nextMailDay;
        gameData.nextMail = gameData.nextMail.nextMailGood;
    }
    public void CloseMailBad()
    {
        mailZone.SetActive(false);
        AddMoney(gameData.nextMail.rightButtonMoney);
        gameData.karma += gameData.nextMail.rightButtonKarma;
        mailNotificationIcon.GetComponent<Image>().sprite = mailNotificationIconBase;
        gameData.nextMailDay = gameData.nextMail.nextMailDay;
        gameData.nextMail = gameData.nextMail.nextMailBad;
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

