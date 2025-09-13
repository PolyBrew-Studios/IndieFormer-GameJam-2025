using System;
using UnityEngine;

public class FallingTreeObstacle : Obstacle
{
    [Header("Setup")]
    [SerializeField] private Transform _pivot;
    private Rigidbody _rb;

    [Header("Fall Axis/Angle")]
    [SerializeField] private Vector3 _targetRotationPosition = Vector3.forward; // local axis to rotate around
    [SerializeField] private float maxFallAngle = 90f;                          // positive fall angle (degrees)

    [Header("Speeds")]
    [SerializeField] private float fallSpeedDegPerSec = 30f;                    // main fall speed
    [SerializeField] private float preLeanSpeedDegPerSec = 60f;                 // speed for the small opposite lean

    [Header("Pre‑lean (opposite direction)")]
    [SerializeField] private float preLeanAngleDegrees = 5f;                    // how much to go opposite before falling
    [SerializeField] private float preLeanHoldSeconds = 0.05f;                  // optional little pause at the opposite extreme

    private enum State { Idle, PreLean, Falling }
    private State _state = State.Idle;

    private Quaternion _basePivotLocalRotation; 
    private Quaternion _preLeanLocalRotation;
    private Quaternion _fallTargetLocalRotation;
    private float _preLeanHoldTimer = 0f;

    private bool hasBeenFallenOver = false;

    private void Awake()
    {
        _rb = GetComponentInChildren<Rigidbody>();
        _basePivotLocalRotation = _pivot != null ? _pivot.localRotation : Quaternion.identity;

        Vector3 axis = _targetRotationPosition.sqrMagnitude > 0f ? _targetRotationPosition.normalized : Vector3.forward;
        _preLeanLocalRotation = _basePivotLocalRotation * Quaternion.AngleAxis(-Mathf.Abs(preLeanAngleDegrees), axis);
        _fallTargetLocalRotation = _basePivotLocalRotation * Quaternion.AngleAxis(Mathf.Abs(maxFallAngle), axis);
    }

    private void FixedUpdate()
    {
        if (_pivot == null || _state == State.Idle)
            return;

        // Keep physics kinematic during controlled animation
        if (_rb != null)
            _rb.isKinematic = true;

        if (_state == State.PreLean)
        {
            float step = Mathf.Max(0f, preLeanSpeedDegPerSec) * Time.fixedDeltaTime;
            _pivot.localRotation = Quaternion.RotateTowards(_pivot.localRotation, _preLeanLocalRotation, step);

            float remaining = Quaternion.Angle(_pivot.localRotation, _preLeanLocalRotation);
            if (remaining <= 0.1f)
            {
                // reached pre-lean
                _pivot.localRotation = _preLeanLocalRotation;
                if (preLeanHoldSeconds > 0f)
                {
                    _preLeanHoldTimer += Time.fixedDeltaTime;
                    if (_preLeanHoldTimer >= preLeanHoldSeconds)
                    {
                        _preLeanHoldTimer = 0f;
                        _state = State.Falling;
                    }
                }
                else
                {
                    _state = State.Falling;
                }
            }
        }
        else if (_state == State.Falling)
        {
            float step = Mathf.Max(0f, fallSpeedDegPerSec) * Time.fixedDeltaTime;
            _pivot.localRotation = Quaternion.RotateTowards(_pivot.localRotation, _fallTargetLocalRotation, step);

            float remaining = Quaternion.Angle(_pivot.localRotation, _fallTargetLocalRotation);
            if (remaining <= 0.1f)
            {
                _pivot.localRotation = _fallTargetLocalRotation; // snap
                _state = State.Idle;

                // re-enable physics if present
                if (_rb != null)
                {
                    _rb.isKinematic = true;
                    _rb.linearVelocity = Vector3.zero;
                    _rb.angularVelocity = Vector3.zero;
                    hasBeenFallenOver = true;
                }
            }
        }
    }

    protected  void OnTriggerEnter(Collider other)
    {
        if(hasBeenFallenOver)
            return;
        
        if (other.CompareTag("Player"))
        {
            // Start at base rotation, then do a small opposite lean before falling
            if (_pivot != null)
            {
                _pivot.localRotation = _basePivotLocalRotation;
            }
            _state = State.PreLean;

            // disable physics during animation so tree doesn't fight the scripted motion
            if (_rb != null)
            {
                _rb.isKinematic = true;
            }
        }
    }
}
