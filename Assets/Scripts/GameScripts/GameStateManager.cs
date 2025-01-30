using UnityEngine;
using System;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    public enum GameState
    {
        Neutral,
        BallInZoneA,
        BallInZoneB,
        HitByPlayer1,
        HitByPlayer2,
        BallOutside,
        BallInNet,
        Player1Score,
        Player2Score
    }

    [SerializeField] private GameState currentState = GameState.Neutral;
    public event Action<GameState> OnStateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Ball.OnTriggerEnterZone += HandleZoneEntry;
        Ball.OnBallHitNet += HandleNetHit;
        Ball.OnBallOutOfBounds += HandleOutOfBounds;
        Ball.OnGroundHit += HandleGroundHit;
        Ball.OnSuccessfulShot += HandleSuccessfulShot;
    }

    public GameState CurrentState
    {
        get => currentState;
        private set
        {
            if (currentState != value)
            {
                currentState = value;
                OnStateChanged?.Invoke(value);
                HandleStateTransitions();
            }
        }
    }

    private void HandleZoneEntry(string zoneName)
    {
        switch (zoneName)
        {
            case "ZoneA" when CurrentState != GameState.BallInZoneA:
                CurrentState = GameState.BallInZoneA;
                break;
            case "ZoneB" when CurrentState != GameState.BallInZoneB:
                CurrentState = GameState.BallInZoneB;
                break;
        }
    }

    private void HandleNetHit() => CurrentState = GameState.BallInNet;

    private void HandleOutOfBounds() => CurrentState = GameState.BallOutside;

    private void HandleGroundHit(string groundArea)
    {
        switch (CurrentState)
        {
            case GameState.BallInZoneA when groundArea == "X1":
                CurrentState = GameState.Player1Score;
                break;
            case GameState.BallInZoneB when groundArea == "Y2":
                CurrentState = GameState.Player2Score;
                break;
        }
    }
    private void HandleSuccessfulShot(Vector3 landingPos, Player player)
    {
        if (currentState == GameState.BallInZoneA || currentState == GameState.BallInZoneB)
        {
            CurrentState = player is AIController ? GameState.HitByPlayer2 : GameState.HitByPlayer1;
        }
    }

    private void HandleStateTransitions()
    {
        switch (CurrentState)
        {
            case GameState.Player1Score:
            case GameState.Player2Score:
                ResetAfterScore();
                break;
            case GameState.BallOutside:
            case GameState.BallInNet:
                HandleFault();
                break;
        }
    }

    private void ResetAfterScore()
    {
        // Reset game elements and return to neutral
        CurrentState = GameState.Neutral;
        Debug.Log("Score processed. Resetting to neutral state.");
    }

    private void HandleFault()
    {
        // Determine fault based on last action
        GameState faultingState = CurrentState switch
        {
            GameState.BallOutside when CurrentState == GameState.HitByPlayer1
                => GameState.Player2Score,
            GameState.BallOutside when CurrentState == GameState.HitByPlayer2
                => GameState.Player1Score,
            GameState.BallInNet when CurrentState == GameState.HitByPlayer1
                => GameState.Player2Score,
            GameState.BallInNet when CurrentState == GameState.HitByPlayer2
                => GameState.Player1Score,
            _ => GameState.Neutral
        };

        CurrentState = faultingState;
    }

    public void RegisterPlayerHit(Player player)
    {
        CurrentState = player is AIController ? GameState.HitByPlayer2 : GameState.HitByPlayer1;
    }

    private void OnDestroy()
    {
        Ball.OnTriggerEnterZone -= HandleZoneEntry;
        Ball.OnBallHitNet -= HandleNetHit;
        Ball.OnBallOutOfBounds -= HandleOutOfBounds;
        Ball.OnGroundHit -= HandleGroundHit;
        Ball.OnSuccessfulShot -= HandleSuccessfulShot;
    }
    private void OnGUI()
    {
        // Define the style for the text
        GUIStyle style = new GUIStyle();
        style.fontSize = 18;
        style.normal.textColor = Color.white;

        // Calculate the position for top-right corner
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        float width = 300f;
        float height = 30f;
        Rect rect = new Rect(screenSize.x - width - 10, 10, width, height);

        // Display the current game state
        GUI.Label(rect, $"Current State: {currentState}", style);
    }
}