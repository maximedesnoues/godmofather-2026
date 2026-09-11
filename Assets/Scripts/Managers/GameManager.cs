using System.Collections.Generic;
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

    [Header("Game Data")]
    [SerializeField] private GameData gameData = new GameData();

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
            _moneySlider.value = gameData.currentMoney;
        }

        SaveOriginalEventProbabilities();
    }

    public void ToggleSellingMode()
    {
        _isSellingMode = !_isSellingMode;
        foreach (var canvasGroup in _sellingModeCanvasGroup)
        {
            canvasGroup.interactable = !_isSellingMode;
        }
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
                    _buyableData.data.Find(x => x.name == _currentSellableObject.name).isBuyable = true;
                    AddMoney(_buyableData.data.Find(x => x.name == _currentSellableObject.name).value);
                    _currentSellableObject.SetActive(false);
                    Data.soldUICount++;
                    Debug.Log("Clicked on: " + _currentSellableObject.name);
                }
            }
            else
            {
                Debug.Log("It's NOT over UI elements");
                Cursor.SetCursor(_CursorTexture, Vector2.zero, CursorMode.Auto);
            }
        }
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
        _moneySlider.value = gameData.currentMoney;
    }

    public void NextDay()
    {
        if (IsGameOver) return;

        gameData.currentDay++;

        if (gameData.currentDay == 12 && mosquitoPopup != null)
        {
            mosquitoPopup.OpenPopup();
        }

        if (gameData.currentDay > gameData.totalDays)
        {
            EndGame();
            return;
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

