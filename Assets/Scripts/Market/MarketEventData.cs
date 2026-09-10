using UnityEngine;

[CreateAssetMenu(fileName = "NewMarketEvent", menuName = "Game/Market Event")]
public class MarketEventData : ScriptableObject
{
    [Header("Event")]
    public string eventTitle;

    [TextArea(3, 8)]
    public string eventDescription;

    [Header("Movement Probabilities")]
    [Range(0, 100)] public int strongUpProbability;
    [Range(0, 100)] public int upProbability;
    [Range(0, 100)] public int stableProbability;
    [Range(0, 100)] public int downProbability;
    [Range(0, 100)] public int strongDownProbability;

    public int TotalProbability => strongUpProbability + upProbability + stableProbability + downProbability + strongDownProbability;

    private void OnValidate()
    {
        if (TotalProbability != 100)
        {
            Debug.LogWarning($"L'événement \"{name}\" possède un total de probabilités de {TotalProbability}% au lieu de 100%.");
        }
    }
}