using System;
using UnityEngine;

public class PlayerForceFieldAdjuster : MonoBehaviour
{
    [Header("Bike Lean")]
    [SerializeField] private Transform _bikePivot;
    [SerializeField] private float _bikeFallRotation_L = 15f;
    [SerializeField] private float _bikeFallRotation_R = -15f;
    [SerializeField] private float idleDelaySeconds = 2f;
    [SerializeField] private float lerpDurationSeconds = 2f;

    [SerializeField] private Collider raycastBoxCollider;
    [SerializeField] private float raycastDetectLeftDistance = 2f;
    [SerializeField] private float raycastDetectRightDistance = 2f;
    
    [Header("Vehicle Param Ranges")]    
    [SerializeField] private float maxSteeringAngle = 40;
    [SerializeField] private float maxSpringStiffness = 30000;
    [SerializeField] private float maxDamperStiffness = 3000;
    
    [SerializeField] private float minSteeringAngle = 20;
    [SerializeField] private float minSpringStiffness = 120;
    [SerializeField] private float minDamperStiffness = 120;
    
    
    [SerializeField] private GameObject playerStatic;
    [SerializeField] private GameObject playerRagdoll;
    
    // Runtime values
    private float _currentSteeringAngle;
    private float _currentSpringStiffness;
    private float _currentDamperStiffness;

    private VehicleController _vehicleController;
    [SerializeField]  private OurInput _currentInputSystem;
    private Rigidbody _rigidbody;

    // Idle tracking
    private float _timeSinceLastSteerInput = 0f;
    private int _lastSteerDir = 0; // -1 left, 1 right, 0 none yet
    private float _idleBlend = 0f; // 0 = original, 1 = fully fallen

    private bool isFalling = false;
    private bool isFallenOff = false;
    
    // Rotation bases
    private Quaternion _baseLocalRotation;

    private void Awake()
    {
        _vehicleController = GetComponent<VehicleController>();
        _rigidbody = GetComponent<Rigidbody>();
        
        // read defaults as max values from controller
        _currentSteeringAngle = maxSteeringAngle = _vehicleController.maxSteeringAngle;
        _currentSpringStiffness = maxSpringStiffness = _vehicleController.springStiffness;
        _currentDamperStiffness = maxDamperStiffness = _vehicleController.damperStiffness;

        // Remember starting rotation
        _bikePivot = _bikePivot == null ? transform : _bikePivot;
        _baseLocalRotation = _bikePivot.localRotation;
    }

    private void FixedUpdate()
    {
        if (_currentInputSystem == null)
            return;

        KeyCode left = _currentInputSystem.LeftSteering;
        KeyCode right = _currentInputSystem.RightSteering;

        // Detect steering input
        int steerDir = 0;
        bool leftPressed = Input.GetKey(left);
        bool rightPressed = Input.GetKey(right);
        if (leftPressed && !rightPressed) steerDir = -1;
        else if (rightPressed && !leftPressed) steerDir = 1;
        else steerDir = 0;

        if (steerDir != 0)
        {
            _lastSteerDir = steerDir;
            _timeSinceLastSteerInput = 0f;
        }
        else
        {
            _timeSinceLastSteerInput += Time.fixedDeltaTime;
        }

        // Determine target idle blend
        float targetBlend = (_timeSinceLastSteerInput >= idleDelaySeconds) ? 1f : 0f;
        float blendSpeed = (lerpDurationSeconds <= 0.0001f) ? 1f : (Time.fixedDeltaTime / lerpDurationSeconds);
        _idleBlend = Mathf.MoveTowards(_idleBlend, targetBlend, blendSpeed);

        // Lerp vehicle parameters between max and min based on _idleBlend
        _currentSteeringAngle = Mathf.Lerp(maxSteeringAngle, minSteeringAngle, _idleBlend);
        _currentSpringStiffness = Mathf.Lerp(maxSpringStiffness, minSpringStiffness, _idleBlend);
        _currentDamperStiffness = Mathf.Lerp(maxDamperStiffness, minDamperStiffness, _idleBlend);

        _vehicleController.maxSteeringAngle = _currentSteeringAngle;
         _vehicleController.springStiffness = _currentSpringStiffness;
         _vehicleController.damperStiffness = _currentDamperStiffness;

        // Compute target lean rotation
        float targetLean = 0f;
        if (_idleBlend > 0f)
        {
            // Choose direction based on last steer, default to left if unknown
            if (_lastSteerDir < 0)
                targetLean = _bikeFallRotation_L;
            else if (_lastSteerDir > 0)
                targetLean = _bikeFallRotation_R;
            else
                targetLean = _bikeFallRotation_L; // default
        }

        isFalling = targetLean != 0f;

        // if starting to fall, check if we already flipped to the ground by shooting a raycast to the falling side of the bike
        if (isFalling)
        {
            // steer right
            if (_lastSteerDir == 0  || _lastSteerDir < 0)
            {

                var leftAnchor = _vehicleController.tireAnchors[1];
                
                Debug.DrawLine(leftAnchor.position, leftAnchor.position + Vector3.right * 20f, Color.lightYellow);
                Physics.Raycast(leftAnchor.position, Vector3.right, out RaycastHit hit, raycastDetectLeftDistance);
                {
                    if (hit.collider != null)
                    {
                        Debug.Log("Fallen over L");
                        isFallenOff = true;
                    }
                                            
                }
            }
            else if (_lastSteerDir > 0)
            {
                var rightAnchor = _vehicleController.tireAnchors[2];
                
                Debug.DrawLine(rightAnchor.position, rightAnchor.position + Vector3.right * 20f, Color.lightYellow);
                Physics.Raycast(rightAnchor.position, Vector3.left, out RaycastHit hit, raycastDetectLeftDistance);
                {
                    if (hit.collider != null)
                    {
                        Debug.Log("Fallen over R");                    
                        isFallenOff = true;
                        
                    }
                }
            }
            
        }
        

        if (isFallenOff != true)
        {
            Quaternion targetRotation = _baseLocalRotation * Quaternion.AngleAxis(targetLean, Vector3.forward);
            gameObject.transform.localRotation = gameObject.transform.localRotation * targetRotation;    
        }
        else
        {
            playerStatic?.SetActive(false);
            playerRagdoll?.SetActive(true);
            
            _vehicleController.enabled = false;
        }
        
    }
}
