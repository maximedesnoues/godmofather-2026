using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

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

    public void AddMoney(int amount)
    {
        if (amount <= 0) return;
        gameData.currentMoney += amount;
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

        if (gameData.currentDay > gameData.totalDays) EndGame();
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
}