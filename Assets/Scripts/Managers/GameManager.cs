using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using static UnityEditor.Experimental.GraphView.GraphView;

public class GameManager : MonoBehaviour
{
    [SerializeField] private bool _isSellingMode;
    [SerializeField] private List<CanvasGroup> _sellingModeCanvasGroup;
    private int _sellableUILayer;
    [SerializeField] private Texture2D _sellCursorTexture;
    [SerializeField] private Texture2D _CursorTexture;
    [SerializeField] private BuyableData _buyableData;
    private GameObject _currentSellableObject;

    public static GameManager Instance;
    private void Start()
    {
        Instance = this;
        _sellableUILayer = LayerMask.NameToLayer("SellableUI");
        Cursor.SetCursor(_CursorTexture, Vector2.zero, CursorMode.Auto);
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
                    _currentSellableObject.SetActive(false);

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
