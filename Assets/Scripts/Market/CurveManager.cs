using System.Collections.Generic;
using UnityEngine;

public class CurveManager : MonoBehaviour
{
    [Header("Movement Values")]
    [SerializeField] private float strongUpAmount = 12f;
    [SerializeField] private float upAmount = 6f;
    [SerializeField] private float downAmount = 6f;
    [SerializeField] private float strongDownAmount = 12f;

    [Header("UI")]
    [SerializeField] private CurveUI curveUI;

    private readonly List<CurveMovement> movementHistory = new List<CurveMovement>();

    public IReadOnlyList<CurveMovement> MovementHistory => movementHistory;

    private void Start()
    {
        movementHistory.Clear();
        RefreshCurve();
    }

    public void ApplyMovement(CurveMovement movement)
    {
        float value = GameManager.Instance.Data.currentMarketValue;

        switch (movement)
        {
            case CurveMovement.StrongUp:
                value += strongUpAmount;
                break;

            case CurveMovement.Up:
                value += upAmount;
                break;

            case CurveMovement.Stable:
                break;

            case CurveMovement.Down:
                value -= downAmount;
                break;

            case CurveMovement.StrongDown:
                value -= strongDownAmount;
                break;
        }

        GameManager.Instance.Data.currentMarketValue = value;

        movementHistory.Add(movement);

        Debug.Log($"Courbe : {MarketManager.MovementToString(movement)} | Nouvelle valeur : {value}");

        RefreshCurve();
    }

    private void RefreshCurve()
    {
        if (curveUI == null)
        {
            Debug.LogWarning("CurveUI n'est pas assigné dans CurveManager.");
            return;
        }

        curveUI.DrawCurve(movementHistory);
    }
}