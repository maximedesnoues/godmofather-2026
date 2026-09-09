using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEditor.Experimental.GraphView.GraphView;

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

    [Header("Game Data")]
    [SerializeField] private GameData gameData = new GameData();

    public GameData Data => gameData;
    public bool IsGameOver { get; private set; }

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

        gameData.currentDay++;

        // TO DO :  si Data.soldUICount > 0 alors proba event 20% partout 

        if (gameData.currentDay > gameData.totalDays)
        {
            EndGame();
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

