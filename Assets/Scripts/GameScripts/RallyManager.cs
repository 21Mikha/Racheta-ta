using System;
using System.Collections.Generic;
using UnityEngine;

#region Supporting Enums and Classes

// The types of events that can occur during a rally.
public enum RallyEventType
{
    None,
    // When the ball is hit by a player (could be a serve or a return).
    PlayerHit,
    // When the ball bounces on the court.
    Bounce,
    // When the ball hits the net.
    NetHit,
    // When the ball goes out of bounds.
    OutOfBounds
}

// A simplified scoring event—adapt this to your scoring system.
public enum ScoreEvent
{
    None,
    Player1Point,
    Player2Point
}

// A data structure to log what happens during a rally.
public class RallyEvent
{
    public RallyEventType EventType;
    public float Timestamp;
    // For bounce events, we log the zone (e.g., "ZoneA" or "ZoneB").
    public string Zone;
    // For player hit events, we log which player performed the hit.
    public Player Player;

    public RallyEvent(RallyEventType eventType, float timestamp, string zone = "", Player player = null)
    {
        EventType = eventType;
        Timestamp = timestamp;
        Zone = zone;
        Player = player;
    }
}

// Optional: a simple state machine for the rally.
public enum RallyState
{
    WaitingForRally,
    RallyInProgress,
    RallyEnded
}

#endregion

public class RallyManager : MonoBehaviour
{
    public static RallyManager Instance { get; private set; }

    // A list to log events as they occur.
    private List<RallyEvent> rallyEvents = new List<RallyEvent>();

    // State variables.
    public RallyState CurrentRallyState { get; private set; } = RallyState.WaitingForRally;
    public Player LastHitter { get; private set; }
    // Bounce counters for each court zone.
    public int BounceCountZoneA { get; private set; }
    public int BounceCountZoneB { get; private set; }

    // Event for when a rally evaluation completes.
    public event Action<ScoreEvent> OnRallyEvaluated;

    // Used to time out a rally if no new events occur.
    private float rallyStartTime;
    // Stores the most recent zone information (from collision events).
    private string lastZone = "";

    #region Unity Lifecycle

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
        // Subscribe to ball collision events.
        // (Your Ball class should invoke these events from OnCollisionEnter.)
        BallPhysics.OnGroundHit += HandleBallBounce;
        Ball.OnCollisionZone += HandleCollisionZone;
        Ball.OnBallHitNet += HandleBallNetHit;
        Ball.OnBallOutOfBounds += HandleBallOutOfBounds;
        BallPhysics.OnPlayerHitBall += HandlePlayerHitBall;
    }

    private void OnDestroy()
    {
        // Unsubscribe to avoid memory leaks.
        BallPhysics.OnGroundHit -= HandleBallBounce;
        Ball.OnCollisionZone -= HandleCollisionZone;
        Ball.OnBallHitNet -= HandleBallNetHit;
        Ball.OnBallOutOfBounds -= HandleBallOutOfBounds;
        BallPhysics.OnPlayerHitBall -= HandlePlayerHitBall;
    }

    private void Update()
    {
        // If a rally is in progress but no events occur for 5 seconds, evaluate it.
        if (CurrentRallyState == RallyState.RallyInProgress && Time.time - rallyStartTime > 5f)
        {
            EndRallyAndEvaluate();
        }
    }

    #endregion

    #region Rally Event Logging Methods

    // Reset all rally data for the next point.
    public void ResetRally()
    {
        rallyEvents.Clear();
        BounceCountZoneA = 0;
        BounceCountZoneB = 0;
        LastHitter = null;
        lastZone = "";
        CurrentRallyState = RallyState.WaitingForRally;
        rallyStartTime = Time.time;
    }

    // Utility to add an event to the rally log.
    private void AddRallyEvent(RallyEvent rallyEvent)
    {
        rallyEvents.Add(rallyEvent);
        Debug.Log($"Rally Event Added: {rallyEvent.EventType} at {rallyEvent.Timestamp:F2} Zone: {rallyEvent.Zone}");
    }

    #endregion

    #region Event Handlers

    // Called when a player hits the ball.
    private void HandlePlayerHitBall(Vector3 Pos, Vector3 velocity, Player player)
    {
        if (CurrentRallyState == RallyState.WaitingForRally)
        {
            CurrentRallyState = RallyState.RallyInProgress;
            rallyStartTime = Time.time;
        }
        LastHitter = player;
        AddRallyEvent(new RallyEvent(RallyEventType.PlayerHit, Time.time, "", player));
    }

    // Called when the ball bounces (via collision with the court).
    private void HandleBallBounce(Vector3 Pos, Vector3 velocity)
    {
        // Use the most recent zone information (set in HandleCollisionZone).
        string zone = lastZone;
        AddRallyEvent(new RallyEvent(RallyEventType.Bounce, Time.time, zone));
        if (zone == "ZoneA")
            BounceCountZoneA++;
        else if (zone == "ZoneB")
            BounceCountZoneB++;
    }

    // Called when the ball collides with a zone (e.g., ZoneA or ZoneB).
    private void HandleCollisionZone(string zoneTag)
    {
        lastZone = zoneTag;
        // (Optional: you could log an event here if needed.)
    }

    // Called when the ball hits the net.
    private void HandleBallNetHit()
    {
        AddRallyEvent(new RallyEvent(RallyEventType.NetHit, Time.time));
    }

    // Called when the ball goes out of bounds.
    private void HandleBallOutOfBounds()
    {
        AddRallyEvent(new RallyEvent(RallyEventType.OutOfBounds, Time.time));
        EndRallyAndEvaluate();
    }

    #endregion

    #region Rally Evaluation

    // Ends the rally and triggers evaluation of the point.
    public void EndRallyAndEvaluate()
    {
        if (CurrentRallyState == RallyState.RallyEnded)
            return;

        CurrentRallyState = RallyState.RallyEnded;
        ScoreEvent score = EvaluateRally();
        OnRallyEvaluated?.Invoke(score);
        Debug.Log($"Rally Evaluated: {score}");
        // After evaluation, reset for the next rally.
        ResetRally();
    }

    // Evaluate the rally events and decide who wins the point.
    // (This is where you implement your tennis-specific rules.)
    public ScoreEvent EvaluateRally()
    {
        // Sample logic:
        // 1. If the ball bounced twice in the same court, the last hitter loses the rally.
        // 2. If the ball went out-of-bounds after one bounce, award the point to the opponent.
        // 3. Otherwise, if the rally ended (say, by timeout) without a fault, return no point.
        //
        // (You can make this as complex as needed to handle serves, lets, volley faults, etc.)

        // Determine the rally zone based on the last known zone.
        string rallyZone = lastZone;

        // Check for a double bounce on one side.
        if ((rallyZone == "ZoneA" && BounceCountZoneA >= 2) ||
            (rallyZone == "ZoneB" && BounceCountZoneB >= 2))
        {
            // The last hitter loses the rally (faulted by double bounce).
            return LastHitter is AIController ? ScoreEvent.Player2Point : ScoreEvent.Player1Point;
        }

        // Check for out-of-bounds.
        bool outOfBounds = rallyEvents.Exists(e => e.EventType == RallyEventType.OutOfBounds);
        if (outOfBounds)
        {
            // If the ball went out without bouncing, then it’s a fault.
            bool noBounce = ((rallyZone == "ZoneA" && BounceCountZoneA == 0) ||
                             (rallyZone == "ZoneB" && BounceCountZoneB == 0));

            if (noBounce)
            {
                // Award the point to the opponent of the last hitter.
                return LastHitter is AIController ? ScoreEvent.Player1Point : ScoreEvent.Player2Point;
            }
            else
            {
                // If the ball bounced (even once) and then went out,
                // you can decide based on your rules.
                // Here, for simplicity, we treat it as a fault too.
                return LastHitter is AIController ? ScoreEvent.Player2Point : ScoreEvent.Player1Point;
            }
        }

        // If no fault conditions are met, assume the rally is still in play.
        return ScoreEvent.None;
    }

    #endregion

    #region Debugging Display

    // Display the rally event log on screen for debugging.
    private void OnGUI()
    {
        GUIStyle style = new GUIStyle { fontSize = 14, normal = { textColor = Color.white } };
        string eventsLog = "Rally Events:\n";
        foreach (var e in rallyEvents)
        {
            eventsLog += $"{e.EventType} {(string.IsNullOrEmpty(e.Zone) ? "" : " - " + e.Zone)} at {e.Timestamp:F2}\n";
        }
        GUI.Label(new Rect(500, 10, 300, 300), eventsLog, style);
    }

    #endregion
}
