using System;
using UnityEngine;

[Serializable]
public class GameData
{
    [Header("Money")]
    [Min(0)] public int currentMoney = 1000;
    [Min(0)] public int moneyQuota = 300000;
    [Min(0)] public int soldUICount = 0;

    [Header("Time")]
    [Min(1)] public int currentDay = 1;
    [Min(1)] public int totalDays = 20;

    [Header("Market")]
    [Min(0f)] public float currentMarketValue = 100f;
    public CurveMovement playerPrediction = CurveMovement.Stable;
    [Min(0)] public int currentBet = 0;
    public bool hasPrediction = false;

    public int DaysRemaining => Mathf.Max(0, totalDays - currentDay);
    public int MoneyRemainingToQuota => Mathf.Max(0, moneyQuota - currentMoney);
    public bool QuotaReached => currentMoney >= moneyQuota;
    public bool CanPredict => currentDay >= 1 && currentDay <= totalDays;
}