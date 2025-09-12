
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyController : MonoBehaviour, ILaggable
{
    [Header("References")]
    public Transform player;

    [Header("Combat")]
    public float fireRate = 1f;
    public float projectileSpeed = 10f;
    public string projectileTag = "Projectile";
    public float jitterAmount = 0.1f;

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float followRadius = 5f;
    public float curveAmount = 4f;

    [Header("Idle Orbit when Player Still")]
    public bool enableIdleOrbit = true;
    [Tooltip("Player speed below this is treated as 'still' for enemy idle orbiting")]
    public float stillnessSpeedThreshold = 0.05f;
    [Tooltip("Angle range in degrees for each idle orbit segment around the player")] 
    public Vector2 idleOrbitAngleRange = new Vector2(20f, 60f);
    [Tooltip("Multiplier for enemy speed during idle orbit")] 
    public float idleMoveSpeedMultiplier = 1f;
    [Tooltip("Multiplier for curve amount during idle orbit")] 
    public float idleCurveAmountMultiplier = 0.6f;

    private bool canFire = true;
    private bool canMove = true;
    private bool isMoving = false;
    private List<Vector3> positionHistory = new List<Vector3>();
    private const int POSITION_HISTORY_COUNT = 30;
    private static readonly WaitForSeconds s_RecordInterval = new WaitForSeconds(0.1f);

    private Rigidbody playerRb;
    private Vector3 lastPlayerPos;

    private void OnEnable()
    {
        LagManager._event.AddListener(OnLag);
    }

    private void OnDisable()
    {
        LagManager._event.RemoveListener(OnLag);
    }

    void Start()
    {
        StartCoroutine(RecordPositionRoutine());
        if (player != null)
        {
            playerRb = player.GetComponent<Rigidbody>();
            lastPlayerPos = player.position;
        }
    }

    public void OnLag(LagPayload payload)
    {
        switch (payload.type)
        {
            case LagType.Freeze:
                StartCoroutine(FreezeRoutine(payload.duration));
                break;
            case LagType.Jitter:
                StartCoroutine(JitterRoutine(payload.duration));
                break;
            case LagType.PositionReset:
                if (positionHistory.Count > 0)
                {
                    transform.position = positionHistory[Random.Range(0, positionHistory.Count)];
                }
                break;
        }
    }

    void Update()
    {
        if (player == null) return;

        if (canFire && !isMoving && Time.time >= nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + 1f / fireRate;
        }

        if (isMoving || !canMove)
        {
            return; 
        }

        // Measure player speed to detect stillness
        float playerSpeed = 0f;
        if (playerRb != null)
        {
            playerSpeed = playerRb.linearVelocity.magnitude;
        }
        else
        {
            Vector3 currentPos = player.position;
            playerSpeed = (currentPos - lastPlayerPos).magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
            lastPlayerPos = currentPos;
        }

        // Use squared distance to avoid sqrt
        float sqrDistanceToPlayer = (transform.position - player.position).sqrMagnitude;
        if (sqrDistanceToPlayer > followRadius * followRadius)
        {
            StartCoroutine(MoveAlongCurve());
        }
        else if (enableIdleOrbit && playerSpeed <= stillnessSpeedThreshold)
        {
            StartCoroutine(MoveAlongCurveIdle());
        }
    }

    private float nextFireTime;

    void Fire()
    {
        if (player == null) return;

        GameObject projectile = ObjectPooler.Instance.SpawnFromPool(projectileTag, transform.position, Quaternion.identity);
        if (projectile != null)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            if (projectile.TryGetComponent<Rigidbody>(out var body))
            {
                body.linearVelocity = direction * projectileSpeed;
            }
        }
    }

    private IEnumerator MoveAlongCurve()
    {
        isMoving = true;

        Vector3 startPos = transform.position;
        Vector3 targetPos = player.position + (transform.position - player.position).normalized * followRadius;

        // Inverse curve: closer to the player => smaller curve; farther away => wider swing
        float startDistanceToPlayer = Vector3.Distance(startPos, player.position);
        float overshoot = Mathf.Max(0f, startDistanceToPlayer - followRadius);
        float normalized = Mathf.Clamp01(overshoot / followRadius); // 0 when at radius, 1 when 2x radius away
        float dynamicCurve = Mathf.Lerp(0f, curveAmount, normalized);

        Vector3 controlPoint = startPos + (targetPos - startPos) / 2 + Vector3.Cross((targetPos - startPos).normalized, Vector3.up) * dynamicCurve * Random.Range(-1f, 1f);

        float journeyTime = Vector3.Distance(startPos, targetPos) / moveSpeed;
        float startTime = Time.time;
        float t = 0f;

        while (t < 1f)
        {
            while (!canMove)
            {
                yield return null;
            }

            if (player == null)
            {
                isMoving = false;
                yield break;
            }

            t = (Time.time - startTime) / journeyTime;
            transform.position = GetBezierPoint(startPos, controlPoint, targetPos, t);
            yield return null;
        }

        isMoving = false;
    }

    private IEnumerator MoveAlongCurveIdle()
    {
        isMoving = true;

        Vector3 startPos = transform.position;
        if (player == null)
        {
            isMoving = false;
            yield break;
        }

        // Determine a target position on the follow circle around the player by rotating our radial vector
        Vector3 radial = startPos - player.position;
        if (radial.sqrMagnitude < 0.0001f)
        {
            radial = Vector3.forward;
        }
        radial = radial.normalized;

        float angle = Random.Range(idleOrbitAngleRange.x, idleOrbitAngleRange.y) * (Random.value < 0.5f ? -1f : 1f);
        Vector3 rotated = Quaternion.AngleAxis(angle, Vector3.up) * radial;
        Vector3 targetPos = player.position + rotated * followRadius;

        // Curve a bit less during idle orbit to keep motion smooth
        float startDistanceToPlayer = Vector3.Distance(startPos, player.position);
        float overshoot = Mathf.Max(0f, startDistanceToPlayer - followRadius);
        float normalized = Mathf.Clamp01(overshoot / followRadius);
        float dynamicCurve = Mathf.Lerp(0f, curveAmount, normalized) * Mathf.Max(0f, idleCurveAmountMultiplier);

        Vector3 controlPoint = startPos + (targetPos - startPos) / 2 +
                               Vector3.Cross((targetPos - startPos).normalized, Vector3.up) * dynamicCurve * Random.Range(-1f, 1f);

        float journeyTime = Vector3.Distance(startPos, targetPos) / (moveSpeed * Mathf.Max(0.01f, idleMoveSpeedMultiplier));
        float startTime = Time.time;
        float t = 0f;

        while (t < 1f)
        {
            while (!canMove)
            {
                yield return null;
            }

            if (player == null)
            {
                isMoving = false;
                yield break;
            }

            t = (Time.time - startTime) / journeyTime;
            transform.position = GetBezierPoint(startPos, controlPoint, targetPos, t);
            yield return null;
        }

        isMoving = false;
    }

    private Vector3 GetBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        t = Mathf.Clamp01(t);
        float oneMinusT = 1f - t;
        return (oneMinusT * oneMinusT * p0) + (2f * oneMinusT * t * p1) + (t * t * p2);
    }

    private IEnumerator RecordPositionRoutine()
    {
        while (true)
        {
            if (positionHistory.Count >= POSITION_HISTORY_COUNT)
            {
                positionHistory.RemoveAt(0);
            }
            positionHistory.Add(transform.position);
            yield return s_RecordInterval;
        }
    }

    private IEnumerator FreezeRoutine(float duration)
    {
        if (Debug.isDebugBuild)
        {
            Debug.Log($"Enemy {gameObject.GetInstanceID()} Freeze STARTED");
        }
        canFire = false;
        canMove = false;
        yield return new WaitForSeconds(duration);
        canFire = true;
        canMove = true;
        if (Debug.isDebugBuild)
        {
            Debug.Log($"Enemy {gameObject.GetInstanceID()} Freeze ENDED");
        }
    }

    private IEnumerator JitterRoutine(float duration)
    {
        canMove = false;
        Vector3 originalPosition = transform.position;
        float timer = 0f;

        while (timer < duration)
        {
            float x = Random.Range(-1f, 1f) * jitterAmount;
            float z = Random.Range(-1f, 1f) * jitterAmount;
            transform.position = originalPosition + new Vector3(x, 0, z);
            timer += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPosition;
        canMove = true;
    }
}
