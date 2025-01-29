using UnityEngine;

public class AIController : Player
{
    private enum AIState { Idle, MoveToPosition, PrepareForShot, HitBall, RecoverPosition }
    private AIState currentState = AIState.Idle;

    [Header("AI Settings")]
    [SerializeField] private float reactionTime = 0.15f;
    [SerializeField] private float shotVariationAngle = 10f;
    [SerializeField] private float shotDecisionRadius = 1.5f;
    [SerializeField] private float recoveryThreshold = 1f;
    [SerializeField] private float netApproachDistance = 5f;
    [SerializeField] private float lobVerticalThreshold = -20f;

    private Rigidbody rb;
    private Vector3 targetPosition;
    private bool isBallApproaching;
    private float decisionTimer;
    private Vector3 neutralPosition;
    private Vector3 netPosition = new Vector3(0, 0, 0);
    [SerializeField] private Transform opponent;
    private Vector3 lastOpponentPosition;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody>();
        neutralPosition = new Vector3(0, 0, transform.position.z);
        SetBounds(-40, 40, 0,100);
    }

    private void OnEnable() => ball.OnSuccessfulShot += OnBallHit;
    private void OnDisable() => ball.OnSuccessfulShot -= OnBallHit;


    private void Update()
    {
        decisionTimer -= Time.deltaTime;

        // Check for new ball approaching regardless of current state
        if (isBallApproaching && decisionTimer <= 0)
        {
            if (currentState != AIState.MoveToPosition && currentState != AIState.PrepareForShot)
            {
                ChangeState(AIState.MoveToPosition);
            }
        }

        switch (currentState)
        {
            case AIState.MoveToPosition:
                MoveToTargetPosition();
                if (WithinReachOfTarget())
                    ChangeState(AIState.PrepareForShot);
                break;

            case AIState.PrepareForShot when ball.canPerformShot:
                ExecuteShot();
                break;

            case AIState.RecoverPosition:
                MoveToNeutralPosition();

                // Check both recovery completion and new ball
                if (Vector3.Distance(transform.position, neutralPosition) < recoveryThreshold)
                {
                    ChangeState(AIState.Idle);
                }
                break;
        }
    }

    private void OnBallHit(Vector3 landingPosition, Player player)
    {
        if (player != this)
        {
            decisionTimer = reactionTime;
            targetPosition = CalculateOptimalMovePosition(landingPosition);
            isBallApproaching = true;
            lastOpponentPosition = opponent.position;

            // Immediately interrupt recovery if ball is coming
            if (currentState == AIState.RecoverPosition)
            {
                ChangeState(AIState.MoveToPosition);
            }
        }
    }

    private Vector3 CalculateOptimalMovePosition(Vector3 landingPosition)
    {
        Vector3 adjustedPosition = landingPosition;
        adjustedPosition.x = Mathf.Clamp(adjustedPosition.x, minX + 2f, maxX - 2f);
        adjustedPosition.z = Mathf.Clamp(adjustedPosition.z, minZ + 2f, maxZ - 2f);
        return adjustedPosition + CalculateAnticipationOffset();
    }

    private Vector3 CalculateAnticipationOffset()
    {
        Vector3 ballToOpponent = (lastOpponentPosition - ball.transform.position).normalized;
        return ballToOpponent * Random.Range(0.5f, 1.5f);
    }

    private void MoveToTargetPosition()
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        currentDirection = new Vector2(direction.x, direction.z);
        isMoving = true;

        if (Vector3.Distance(transform.position, targetPosition) < 0.5f)
        {
            isMoving = false;
        }
    }

    private void MoveToNeutralPosition()
    {
        targetPosition = neutralPosition;
        MoveToTargetPosition();
    }

    private bool WithinReachOfTarget()
    {
        Vector2 aiPos = new Vector2(transform.position.x, transform.position.z);
        Vector2 targetPos = new Vector2(targetPosition.x, targetPosition.z);
        return Vector2.Distance(aiPos, targetPos) < shotDecisionRadius;
    }

    private void ExecuteShot()
    {
        Vector3 shotDirection = CalculateOptimalShotDirection();

        if (ShouldUseLob())
        {
            ball.LobShot(shotDirection, Random.Range(0.8f, 1.2f), this);
        }
        else if (ShouldUseTopspin())
        {
            ball.TopspinShot(shotDirection, Random.Range(0.5f, 1.5f), this);
        }
        else
        {
            ball.FlatShot(shotDirection, this);
        }

        ChangeState(AIState.RecoverPosition);
    }

    private Vector3 CalculateOptimalShotDirection()
    {
        Vector3 targetPosition = GetOpponentWeakSide();
        Vector3 baseDirection = (targetPosition - transform.position).normalized;
        return AddShotVariation(baseDirection);
    }

    private Vector3 GetOpponentWeakSide()
    {
        Vector3 opponentPosition = opponent.position;
        float courtWidth = maxX - minX;

        // Target the side opposite to where opponent is moving
        Vector3 movementDirection = opponent.GetComponent<Rigidbody>().velocity.normalized;
        float lateralBias = Vector3.Dot(movementDirection, Vector3.right);

        return new Vector3(
            lateralBias > 0 ? minX + courtWidth * 0.25f : maxX - courtWidth * 0.25f,
            0,
            opponentPosition.z + Random.Range(-3f, 3f)
        );
    }

    private Vector3 AddShotVariation(Vector3 baseDirection)
    {
        float randomAngle = Random.Range(-shotVariationAngle, shotVariationAngle);
        return Quaternion.Euler(0, randomAngle, 0) * baseDirection;
    }

    private bool ShouldUseLob()
    {
        return opponent.position.z > netPosition.z + 2f &&
               transform.position.z < lobVerticalThreshold &&
               stamina.CurrentStamina > 50f;
    }

    private bool ShouldUseTopspin()
    {
        float netDistance = Vector3.Distance(transform.position, netPosition);
        return netDistance < netApproachDistance &&
               stamina.CurrentStamina > 30f &&
               Random.value > 0.3f;
    }

    private void ChangeState(AIState newState)
    {
        if (currentState == newState) return;

        currentState = newState;
        switch (newState)
        {
            case AIState.RecoverPosition:
                targetPosition = neutralPosition;
                break;
        }
    }
private void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 18;
        style.normal.textColor = Color.white;
        GUI.Label(new Rect(10, 10, 300, 30), $"AI State: {currentState}", style);
        GUI.Label(new Rect(10, 40, 300, 30), $"Ball Approaching: {isBallApproaching}", style);
        GUI.Label(new Rect(10, 70, 300, 30), $"Target Position: {targetPosition}", style);
        GUI.Label(new Rect(10, 100, 300, 30), $"Current Position: {transform.position}", style);
        GUI.Label(new Rect(10, 130, 300, 30), $"Opponent Position: {(opponent != null ? opponent.transform.position.ToString() : "N/A")}", style);
        GUI.Label(new Rect(10, 160, 300, 30), $"AI Velocity: {rb.velocity}", style);
    }
}
