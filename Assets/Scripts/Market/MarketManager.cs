using UnityEngine;
using UnityEngine.InputSystem;

public class MarketManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CurveManager curveManager;
    [SerializeField] private MarketUI marketUI;
    [SerializeField] private EventUI eventUI;

    [Header("Available Events")]
    [SerializeField] private MarketEventData[] availableEvents;

    [Header("Hint Settings")]
    [Range(0f, 1f)][SerializeField] private float hintChance = 0.50f;
    [Range(0f, 1f)][SerializeField] private float wrongHintChance = 0.10f;

    public MarketEventData CurrentEvent { get; private set; }
    public CurveMovement HiddenMovement { get; private set; }
    public CurveMovement HintMovement { get; private set; }

    public bool HasHint { get; private set; }
    public bool HintIsWrong { get; private set; }
    public bool PredictionValidated { get; private set; }

    private void Start()
    {
        StartDay();
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame && eventUI != null)
            eventUI.ToggleEventWindow();

        if (Keyboard.current.eKey.wasPressedThisFrame)
            EndDay();
    }

    public void StartDay()
    {
        PredictionValidated = false;

        GameManager.Instance.Data.hasPrediction = false;
        GameManager.Instance.Data.currentBet = 0;

        if (marketUI != null)
            marketUI.ApplyCurrentBetInput();

        GenerateRandomEvent();

        if (CurrentEvent == null) return;

        HiddenMovement = RollMovement(CurrentEvent);
        GenerateHint();

        Debug.Log($"===== JOUR {GameManager.Instance.Data.currentDay} =====");
        Debug.Log($"Événement : {CurrentEvent.eventTitle}");
        Debug.Log($"[DEBUG] Résultat secret : {MovementToString(HiddenMovement)}");

        if (HasHint)
        {
            string hintState = HintIsWrong ? "FAUX" : "VRAI";
            Debug.Log($"Indice : {MovementToString(HintMovement)} [DEBUG : {hintState}]");
        }
        else
        {
            Debug.Log("Aucun indice aujourd'hui.");
        }

        if (eventUI != null)
            eventUI.SetEvent(CurrentEvent, HasHint, HintMovement);

        if (marketUI != null)
            marketUI.RefreshAll();
    }

    private void GenerateRandomEvent()
    {
        if (availableEvents == null || availableEvents.Length == 0)
        {
            Debug.LogError("Aucun MarketEventData n'est configuré dans MarketManager.");
            CurrentEvent = null;
            return;
        }

        int randomIndex = Random.Range(0, availableEvents.Length);
        CurrentEvent = availableEvents[randomIndex];
    }

    private CurveMovement RollMovement(MarketEventData marketEvent)
    {
        if (marketEvent.TotalProbability != 100)
        {
            Debug.LogError($"Les probabilités de {marketEvent.name} doivent totaliser 100%.");
            return CurveMovement.Stable;
        }

        int roll = Random.Range(0, 100);
        int cumulative = marketEvent.strongUpProbability;

        if (roll < cumulative) return CurveMovement.StrongUp;

        cumulative += marketEvent.upProbability;
        if (roll < cumulative) return CurveMovement.Up;

        cumulative += marketEvent.stableProbability;
        if (roll < cumulative) return CurveMovement.Stable;

        cumulative += marketEvent.downProbability;
        if (roll < cumulative) return CurveMovement.Down;

        return CurveMovement.StrongDown;
    }

    private void GenerateHint()
    {
        HasHint = Random.value < hintChance;
        HintIsWrong = false;

        if (!HasHint) return;

        HintIsWrong = Random.value < wrongHintChance;

        if (HintIsWrong)
            HintMovement = GenerateWrongMovement(HiddenMovement);
        else
            HintMovement = HiddenMovement;
    }

    private CurveMovement GenerateWrongMovement(CurveMovement realMovement)
    {
        CurveMovement wrongMovement;

        do
        {
            wrongMovement = (CurveMovement)Random.Range(0, 5);
        }
        while (wrongMovement == realMovement);

        return wrongMovement;
    }

    public void SelectPrediction(CurveMovement prediction)
    {
        if (!GameManager.Instance.Data.CanPredict || PredictionValidated)
            return;

        GameManager.Instance.Data.playerPrediction = prediction;
        GameManager.Instance.Data.hasPrediction = true;

        Debug.Log($"Pronostic sélectionné : {MovementToString(prediction)}");

        if (marketUI != null) marketUI.RefreshAll();
    }

    public void SetBet(int amount)
    {
        if (!GameManager.Instance.Data.CanPredict || PredictionValidated) return;

        amount = Mathf.Clamp(amount, 0, GameManager.Instance.Data.currentMoney);
        GameManager.Instance.Data.currentBet = amount;
    }

    public void ValidatePrediction()
    {
        GameData data = GameManager.Instance.Data;

        if (!data.CanPredict)
        {
            Debug.Log("Les pronostics commencent au jour 2.");
            return;
        }

        if (PredictionValidated)
        {
            Debug.Log("Le pronostic de cette journée est déjà validé.");
            return;
        }

        if (!data.hasPrediction)
        {
            Debug.Log("Choisis d'abord --, -, =, + ou ++.");
            return;
        }

        if (data.currentBet <= 0)
        {
            Debug.Log("La mise doit être supérieure à 0.");
            return;
        }

        if (data.currentBet > data.currentMoney)
        {
            Debug.Log("Tu ne possèdes pas assez d'argent.");
            return;
        }

        PredictionValidated = true;

        Debug.Log($"PRONOSTIC VALIDÉ : {MovementToString(data.playerPrediction)} | Mise : {data.currentBet}");

        if (marketUI != null) marketUI.RefreshAll();
    }

    public void EndDay()
    {
        if (GameManager.Instance.IsGameOver)
            return;

        if (!PredictionValidated)
        {
            Debug.Log("Impossible de terminer la journée : sélectionne un pronostic, entre une mise puis clique sur VALIDER.");
            return;
        }

        Debug.Log($"===== FIN DU JOUR {GameManager.Instance.Data.currentDay} =====");

        ResolveCurrentDay();
        GameManager.Instance.NextDay();

        if (!GameManager.Instance.IsGameOver)
            StartDay();
    }

    private void ResolveCurrentDay()
    {
        ResolveBet();

        curveManager.ApplyMovement(HiddenMovement);

        Debug.Log($"Résultat réel : {MovementToString(HiddenMovement)}");

        if (marketUI != null)
            marketUI.RefreshAll();
    }

    private void ResolveBet()
    {
        GameData data = GameManager.Instance.Data;

        int bet = data.currentBet;
        CurveMovement prediction = data.playerPrediction;
        CurveMovement result = HiddenMovement;

        if (prediction == result)
        {
            int gain = Mathf.RoundToInt(bet * 2f);
            GameManager.Instance.AddMoney(gain);
            Debug.Log($"Pronostic exact : +{gain}");
            return;
        }

        MarketZone predictionZone = GetZone(prediction);
        MarketZone resultZone = GetZone(result);

        if (predictionZone == resultZone && resultZone != MarketZone.Neutral)
        {
            int gain = Mathf.RoundToInt(bet * 1.5f);
            GameManager.Instance.AddMoney(gain);
            Debug.Log($"Bonne zone : +{gain}");
            return;
        }

        if (resultZone == MarketZone.Neutral)
        {
            Debug.Log("Le résultat était neutre : aucun gain ni aucune perte.");
            return;
        }

        GameManager.Instance.RemoveMoney(bet);
        Debug.Log($"Mauvaise zone : -{bet}");
    }

    private MarketZone GetZone(CurveMovement movement)
    {
        switch (movement)
        {
            case CurveMovement.StrongUp:
            case CurveMovement.Up:
                return MarketZone.Positive;

            case CurveMovement.Stable:
                return MarketZone.Neutral;

            case CurveMovement.Down:
            case CurveMovement.StrongDown:
                return MarketZone.Negative;

            default:
                return MarketZone.Neutral;
        }
    }

    public static string MovementToString(CurveMovement movement)
    {
        switch (movement)
        {
            case CurveMovement.StrongUp:
                return "++";

            case CurveMovement.Up:
                return "+";

            case CurveMovement.Stable:
                return "=";

            case CurveMovement.Down:
                return "-";

            case CurveMovement.StrongDown:
                return "--";

            default:
                return "?";
        }
    }
}