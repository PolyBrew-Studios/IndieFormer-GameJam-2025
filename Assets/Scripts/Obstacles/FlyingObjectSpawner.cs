using System.Collections;
using UnityEngine;

public class FlyingObjectSpawner : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject _spawnableObject;

    [Header("Launch Direction/Speed")]
    [Tooltip("Direction to launch in. Typically one axis will be 1 (e.g., (1,0,0) for +X). If 'Use Local Direction' is enabled, this is interpreted in the spawner's local space.")]
    [SerializeField] private Vector3 _spawnDirection = Vector3.forward;
    [SerializeField] private float _launchSpeed = 10f;
    [SerializeField] private bool _useLocalDirection = true;

    [Header("Speed Randomization")]
    [Tooltip("Enable random variation around the base launch speed.")]
    [SerializeField] private bool _enableRandomLaunchSpeed = false;
    [Tooltip("Random variation range (± units/second) applied around the base launch speed when enabled.")]
    [SerializeField] private float _randomLaunchSpeedDelta = 0f;

    [Header("Spawn Rotation")]
    [Tooltip("If enabled, uses the specified rotation for the spawned object instead of the spawner's rotation.")]
    [SerializeField] private bool _overrideSpawnRotation = false;
    [Tooltip("Euler angles used for the spawn rotation. Interpreted in world space if 'World Space' is enabled; otherwise relative to the spawner's rotation.")]
    [SerializeField] private Vector3 _spawnRotationEuler = Vector3.zero;
    [Tooltip("If enabled, the spawn rotation Euler is in world space. If disabled, it's relative to this spawner (local space).")]
    [SerializeField] private bool _spawnRotationInWorldSpace = true;

    [Header("Rotation (optional)")]
    [SerializeField] private bool _enableRotation = false;
    [Tooltip("Angular velocity in degrees per second around each axis. Applied if Rotation is enabled and a Rigidbody exists.")]
    [SerializeField] private Vector3 _angularVelocityDegPerSec = Vector3.zero;

    [Header("Lifecycle")]
    [SerializeField] private bool _spawnOnStart = true;
    [Tooltip("Auto-destroy the spawned object after this many seconds.")]
    [SerializeField] private float _lifetimeSeconds = 5f;
    [Tooltip("If the prefab has no Rigidbody, add one so velocity/rotation can be applied.")]
    [SerializeField] private bool _addRigidbodyIfMissing = true;

    [Header("Spawning Over Time")]
    [SerializeField] private bool _enableTimedSpawning = false;
    [Tooltip("How many objects to spawn over a span of 10 seconds (evenly). Set to 0 to disable.")]
    [SerializeField] private int _objectsPer10Seconds = 0;

    [Header("Timed Batch Options")]
    [Tooltip("How many objects to spawn at once for each timed tick. 1 = single spawn per tick.")]
    [SerializeField] private int _spawnBatchCount = 1;

    private Coroutine _spawningRoutine;

    private void Start()
    {
        if (_spawnOnStart)
        {
            Spawn();
        }

        TryStartTimedSpawning();
    }

    private void OnEnable()
    {
        TryStartTimedSpawning();
    }

    private void OnDisable()
    {
        if (_spawningRoutine != null)
        {
            StopCoroutine(_spawningRoutine);
            _spawningRoutine = null;
        }
    }

    private void OnValidate()
    {
        if (_objectsPer10Seconds < 0) _objectsPer10Seconds = 0;
        if (_lifetimeSeconds < 0f) _lifetimeSeconds = 0f;
        if (_launchSpeed < 0f) _launchSpeed = 0f;
        if (_randomLaunchSpeedDelta < 0f) _randomLaunchSpeedDelta = 0f;
        if (_spawnBatchCount < 1) _spawnBatchCount = 1;
    }

    private void TryStartTimedSpawning()
    {
        if (!_enableTimedSpawning || _objectsPer10Seconds <= 0)
        {
            return;
        }

        if (_spawningRoutine == null && isActiveAndEnabled)
        {
            _spawningRoutine = StartCoroutine(SpawnLoop());
        }
    }

    private IEnumerator SpawnLoop()
    {
        while (_enableTimedSpawning && _objectsPer10Seconds > 0)
        {
            // Compute interval based on current setting. Recalculate each loop to reflect runtime changes.
            float interval = 10f / Mathf.Max(1, _objectsPer10Seconds);

            // Wait first to avoid an immediate timed spawn on startup. This prevents double-spawning with _spawnOnStart.
            yield return new WaitForSeconds(interval);

            int batch = Mathf.Max(1, _spawnBatchCount);
            for (int i = 0; i < batch; i++)
            {
                Spawn();
            }
        }

        _spawningRoutine = null;
    }

    public GameObject Spawn()
    {
        if (_spawnableObject == null)
        {
            Debug.LogWarning($"{nameof(FlyingObjectSpawner)}: No spawnable object assigned.");
            return null;
        }

        // Determine spawn rotation
        Quaternion spawnRot = transform.rotation;
        if (_overrideSpawnRotation)
        {
            Quaternion eulerRot = Quaternion.Euler(_spawnRotationEuler);
            spawnRot = _spawnRotationInWorldSpace ? eulerRot : transform.rotation * eulerRot;
        }

        // Instantiate at this spawner's position/rotation (or overridden rotation)
        GameObject instance = Instantiate(_spawnableObject, transform.position, spawnRot);

        // Determine launch direction
        Vector3 dir = _spawnDirection;
        if (dir.sqrMagnitude <= Mathf.Epsilon)
        {
            dir = Vector3.forward; // default safe direction
        }
        dir = dir.normalized;
        if (_useLocalDirection)
        {
            dir = transform.TransformDirection(dir);
        }

        // Ensure Rigidbody exists if we want to apply velocity/rotation
        Rigidbody rb = instance.GetComponent<Rigidbody>();
        if (rb == null && _addRigidbodyIfMissing)
        {
            rb = instance.AddComponent<Rigidbody>();
        }

        if (rb != null)
        {
            // Apply linear velocity
            float speed = Mathf.Max(0f, _launchSpeed);
            if (_enableRandomLaunchSpeed && _randomLaunchSpeedDelta > 0f)
            {
                float min = Mathf.Max(0f, speed - _randomLaunchSpeedDelta);
                float max = speed + _randomLaunchSpeedDelta;
                speed = Random.Range(min, max);
            }
            rb.linearVelocity = dir * speed;

            // Optional angular velocity
            if (_enableRotation && _angularVelocityDegPerSec != Vector3.zero)
            {
                // Convert degrees/sec to radians/sec as Unity stores angularVelocity in rad/sec
                Vector3 angularRad = _angularVelocityDegPerSec * Mathf.Deg2Rad;
                // Interpret angular in world space; if you want local, convert via TransformDirection
                rb.angularVelocity = angularRad;
            }
        }

        // Auto destroy after lifetime
        if (_lifetimeSeconds > 0f)
        {
            Destroy(instance, _lifetimeSeconds);
        }

        return instance;
    }
}
