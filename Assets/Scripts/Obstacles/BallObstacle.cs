using UnityEngine;

public class BallObstacle : ObstacleBase
{
    [Header("Jump Settings")]
    [SerializeField] private float jumpIntervalSeconds = 3f;
    [SerializeField] private float initialDelaySeconds = 0f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private bool useImpulse = true;
    [SerializeField] private bool randomizeInitialDelay = false;

    [Header("Touch Settings")] 
    [SerializeField] private float touchExtraUpwardForce = 2.5f;
    [SerializeField] private float touchHorizontalPush = 0f; // 0 to disable

    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        float delay = initialDelaySeconds;
        if (randomizeInitialDelay && jumpIntervalSeconds > 0f)
        {
            delay = Random.Range(0f, jumpIntervalSeconds);
        }

        if (jumpIntervalSeconds > 0f)
        {
            CancelInvoke(nameof(Jump));
            InvokeRepeating(nameof(Jump), delay, jumpIntervalSeconds);
        }
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(Jump));
    }

    private void Jump()
    {
        if (_rb == null)
            return;

        var mode = useImpulse ? ForceMode.Impulse : ForceMode.Force;
        _rb.AddForce(Vector3.up * jumpForce, mode);
    }


}