using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerForceFieldAdjuster : MonoBehaviour
{
    [Header("Bike Lean")]
    [SerializeField] private Transform _bikePivot;
    [SerializeField] private float _bikeFallRotation_L = 15f; // forces bike to lean left
    [SerializeField] private float _bikeFallRotation_R = -15f; // forces bike to lean right
    [SerializeField] private float idleDelaySeconds = 2f; // time the player has before he needs to make an input
    [SerializeField] private float lerpDurationSeconds = 2f; 

    [SerializeField] private float raycastDetectLeftDistance = 2f;
    [SerializeField] private float raycastDetectRightDistance = 2f;
    
    [Header("Vehicle Param Ranges")]    
    [SerializeField] private float maxSteeringAngle = 40;
    [SerializeField] private float maxSpringStiffness = 30000;
    [SerializeField] private float maxDamperStiffness = 3000;
    
    [SerializeField] private float minSteeringAngle = 20;
    [SerializeField] private float minSpringStiffness = 120;
    [SerializeField] private float minDamperStiffness = 120;
    
    [Header("PlayerModel")]    
    [SerializeField] private GameObject playerStatic;
    [SerializeField] private GameObject playerRagdollPrefab;
    private GameObject _prevRagdoll;
    
    [Header("InputSystem")]    
    [SerializeField]  private OurInput _currentInputSystem;
    
    // Runtime values
    private float _currentSteeringAngle;
    private float _currentSpringStiffness;
    private float _currentDamperStiffness;

    private VehicleController _vehicleController;
    private Rigidbody _rigidbody;

    // Idle tracking
    private float _timeSinceLastSteerInput = 0f;
    private int _lastSteerDir = 0; // -1 left, 1 right, 0 none yet
    private float _idleBlend = 0f; // 0 = original, 1 = fully fallen

    public bool isFalling { get; set; }= false;
    public bool isFallenOff { get; set; } = false;
    
    // Public API to trigger fallen state externally (e.g., from obstacles)
    public void EnableFallenOff()
    {
        isFallenOff = true;
    }
    
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
        
        // if(isFallenOff)
        //     return;

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

        
        // rotationAngleCheck
        Transform rotSource = (_rigidbody != null) ? _rigidbody.transform : transform;

        // degrees between body up and world up
        float uprightAngle = Vector3.Angle(rotSource.up, Vector3.up);
        // at 90 degrees, we're fallen off the bike'
        if (uprightAngle >= 90f || _vehicleController.deathZoneBelow)
        {
            isFalling = true;
            isFallenOff = true;
            Debug.Log($"Fallen over (uprightAngle={uprightAngle:0.0})");
        }

        isFalling = targetLean != 0f;

        // if starting to fall, check if we already flipped to the ground by shooting a raycast to the falling side of the bike
        if (isFalling)
        {
            // steer right
            if (_lastSteerDir == 0  || _lastSteerDir < 0)
            {
                // raycast check as fallback detection
                var leftAnchor = _vehicleController.tireAnchors[1];
                
                Debug.DrawLine(leftAnchor.position, leftAnchor.position + Vector3.right * 20f, Color.lightYellow);
                Physics.Raycast(leftAnchor.position, Vector3.right, out RaycastHit hit, raycastDetectLeftDistance);
                {
                    if (hit.collider != null)
                    {
                        // Debug.Log("Fallen over L");
                        isFallenOff = true;
                    }
                                            
                }
                

            }
            else if (_lastSteerDir > 0)
            {
                // raycast check as fallback detection
                var rightAnchor = _vehicleController.tireAnchors[2];
                
                Debug.DrawLine(rightAnchor.position, rightAnchor.position + Vector3.right * 20f, Color.lightYellow);
                Physics.Raycast(rightAnchor.position, Vector3.left, out RaycastHit hit, raycastDetectLeftDistance);
                {
                    if (hit.collider != null)
                    {
                        // Debug.Log("Fallen over R");                    
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
            if (playerStatic != null)
            {
                playerStatic?.SetActive(false);
                // Spawn ragdoll only once (on first frame after fallen-off)
                if (playerStatic != null && _prevRagdoll == null)
                {
                    var newRagdoll = Instantiate(playerRagdollPrefab, playerStatic.transform.position, playerStatic.transform.rotation);
                    newRagdoll.SetActive(true);
                    _prevRagdoll = newRagdoll; // remember the instance so we don't spawn again
                }

                _vehicleController.enabled = false;
            }

            _vehicleController.enabled = false;

        }
        
   
    }
    
    public void ResetPlayer()
    {
        isFalling = false;
        isFallenOff = false;
        if (_bikePivot != null)
            _bikePivot.localRotation = _baseLocalRotation;
        else
            transform.localRotation = _baseLocalRotation;
        playerStatic?.SetActive(true);
        // Removed: playerRagdollPrefab?.SetActive(false); // don't toggle prefab

        // Cleanup spawned ragdoll instance and clear the "spawned" state
        if (_prevRagdoll != null)
        {
            Destroy(_prevRagdoll);
            _prevRagdoll = null;
        }
        
        _vehicleController.GetRigidBody().linearVelocity = Vector3.zero;
        _vehicleController.GetRigidBody().angularVelocity = Vector3.zero;
        
        _currentSteeringAngle = maxSteeringAngle;
        _currentSpringStiffness = maxSpringStiffness;
        _currentDamperStiffness = maxDamperStiffness;

        _vehicleController._currentLocalVelocity = Vector3.zero;
        _vehicleController._velocityRatio = 0f;
        _vehicleController._currentSteeringAngle = 0f;
        
        _timeSinceLastSteerInput = -2f; // grace period
        _lastSteerDir = 0;
        
        // Wait one frame before re-enabling the controller
        StartCoroutine(EnableControllerNextFrame());

    }
    private IEnumerator EnableControllerNextFrame()
    {
        yield return null; // waits for one frame
        if (_vehicleController != null)
        {
            isFalling = false;
            isFallenOff = false;
            
            // disable again to prevent physics from fighting with the scripted motion
            _vehicleController.GetRigidBody().linearVelocity = Vector3.zero;
            _vehicleController.GetRigidBody().angularVelocity = Vector3.zero;
            
            _currentSteeringAngle = maxSteeringAngle;
            _currentSpringStiffness = maxSpringStiffness;
            _currentDamperStiffness = maxDamperStiffness;

            _vehicleController._currentLocalVelocity = Vector3.zero;
            _vehicleController._velocityRatio = 0f;
            _vehicleController._currentSteeringAngle = 0f;
        
            _timeSinceLastSteerInput = -2f; // grace period
            _lastSteerDir = 0;
            
            
            
            _vehicleController.enabled = true;
        }
    }


}
