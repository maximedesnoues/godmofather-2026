using UnityEngine;
using UnityEngine.InputSystem;

public class InputController: MonoBehaviour
{
    [SerializeField] private InputActionReference _inputActionSellMode;
    void OnEnable()
    {
        if (_inputActionSellMode == null || _inputActionSellMode.action == null) return;

        _inputActionSellMode.action.started += OnSellMode;
    }
    private void OnDisable()
    {
        if (_inputActionSellMode == null || _inputActionSellMode.action == null) return;

        _inputActionSellMode.action.started -= OnSellMode;
    }
    private void OnSellMode(InputAction.CallbackContext context)
    {
        GameManager.Instance.ToggleSellingMode();
    }
}

