using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MarketUI : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private MarketManager marketManager;

    [Header("Money")]
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private TMP_InputField betInputField;

    [Header("Prediction Buttons")]
    [SerializeField] private Button strongDownButton;
    [SerializeField] private Button downButton;
    [SerializeField] private Button stableButton;
    [SerializeField] private Button upButton;
    [SerializeField] private Button strongUpButton;
    [SerializeField] private Button validateButton;

    private void Start()
    {
        RefreshAll();
    }

    private void Update()
    {
        RefreshPredictionControls();
    }

    public void RefreshAll()
    {
        if (GameManager.Instance == null || marketManager == null) return;

        RefreshMoney();
        RefreshPredictionControls();
    }

    private void RefreshMoney()
    {
        if (moneyText == null) return;

        string formattedMoney = GameManager.Instance.Data.currentMoney.ToString("N0").Replace(",", " ");
        moneyText.text = $"$ {formattedMoney}";
    }

    private void RefreshPredictionControls()
    {
        if (GameManager.Instance == null || marketManager == null) return;

        bool canPredict = GameManager.Instance.Data.currentDay >= 2 && !marketManager.PredictionValidated;

        if (strongDownButton != null) strongDownButton.interactable = canPredict;
        if (downButton != null) downButton.interactable = canPredict;
        if (stableButton != null) stableButton.interactable = canPredict;
        if (upButton != null) upButton.interactable = canPredict;
        if (strongUpButton != null) strongUpButton.interactable = canPredict;
        if (betInputField != null) betInputField.interactable = canPredict;
        if (validateButton != null) validateButton.interactable = canPredict;
    }

    public void SelectStrongDown()
    {
        marketManager.SelectPrediction(CurveMovement.StrongDown);
    }

    public void SelectDown()
    {
        marketManager.SelectPrediction(CurveMovement.Down);
    }

    public void SelectStable()
    {
        marketManager.SelectPrediction(CurveMovement.Stable);
    }

    public void SelectUp()
    {
        marketManager.SelectPrediction(CurveMovement.Up);
    }

    public void SelectStrongUp()
    {
        marketManager.SelectPrediction(CurveMovement.StrongUp);
    }

    public void OnBetChanged()
    {
        if (betInputField == null) return;

        if (int.TryParse(betInputField.text, out int amount))
            marketManager.SetBet(amount);
        else
            marketManager.SetBet(0);
    }

    public void ValidatePrediction()
    {
        marketManager.ValidatePrediction();
    }
}