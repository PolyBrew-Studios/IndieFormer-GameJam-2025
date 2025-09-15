using System;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private Transform _checkpointSpawnPoint;
    [SerializeField] private Vector3 _checkpointSpawnPointPosition;
    [SerializeField] public Quaternion _checkpointSpawnPointRotation;
    [SerializeField] public bool hasBeenHit = false;
    
    [SerializeField] private float recordedTime = 0f;
    
    [SerializeField] public VehicleController _vehicleController;
    [SerializeField] public PlayerForceFieldAdjuster playerForceFieldAdjuster;
    
    [SerializeField] private List<GameObject> _hidableObjects;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (_checkpointSpawnPoint == null)
        {
            _checkpointSpawnPointPosition = new Vector3(transform.position.x,transform.position.y,transform.position.z);
        }
        else
        {
            var cPos = new Vector3(_checkpointSpawnPoint.position.x, _checkpointSpawnPoint.position.y, _checkpointSpawnPoint.position.z);
            _checkpointSpawnPointPosition = cPos;
        }
        
        // Use a valid identity rotation by default
        // _checkpointSpawnPointRotation = Quaternion.identity;
    }

    public void RespawnFromCheckpoint()
    {
        Debug.Log("Respawning from checkpoint");
        var controller = _vehicleController.transform;
        controller.position = _checkpointSpawnPointPosition;
        
        
        _vehicleController.deathZoneBelow = false;
        controller.rotation = _checkpointSpawnPointRotation;
        
        TreeRespawner.respawnTree = true;
        // Vector3 euler = controller.eulerAngles;
        // Quaternion uprightRotation = Quaternion.Euler(0f, euler.y, 0f);
        // controller.rotation = uprightRotation;
        //
        //
        //
        // if (_manualSpawnEulerOffset != Vector3.zero)
        // {
        //     rotationToApply = rotationToApply * Quaternion.Euler(_manualSpawnEulerOffset.x, _manualSpawnEulerOffset.y, _manualSpawnEulerOffset.z);
        // }
        //
        // controller.rotation = rotationToApply;
        //
        GameManager.levelManager.ResetTime(recordedTime);
        playerForceFieldAdjuster.ResetPlayer();
        
    }
    
  

    private void OnTriggerEnter(Collider other)
    {
        if (hasBeenHit == false && other.CompareTag("Player"))
        {
            hasBeenHit = true;
            GameManager.SetSpawn(this);
            recordedTime = GameManager.levelManager.GetTime();
            _checkpointSpawnPointRotation = other.transform.rotation;
            
 
            
            foreach (var obj in _hidableObjects)
            {
                obj.SetActive(false);
            }
        }
    }
}
