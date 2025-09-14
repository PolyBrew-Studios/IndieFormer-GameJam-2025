using System;
using System.Collections.Generic;
using NUnit.Framework.Constraints;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private Transform _checkpointSpawnPoint;
    [SerializeField] private Vector3 _checkpointSpawnPointPosition;
    [SerializeField] private Quaternion _checkpointSpawnPointRotation;
    [SerializeField] private bool hasBeenHit = false;

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
        _checkpointSpawnPointRotation = Quaternion.identity;
    }

    public void RespawnFromCheckpoint()
    {
        var controller = _vehicleController.transform;
        controller.position = _checkpointSpawnPointPosition;

        // Reset rotation to be upright (align up with world Y) while preserving current yaw
        Vector3 euler = controller.eulerAngles;
        Quaternion uprightRotation = Quaternion.Euler(0f, euler.y, 0f);
        controller.rotation = uprightRotation;

        playerForceFieldAdjuster.ResetPlayer();
        
    }
    

    private void OnTriggerEnter(Collider other)
    {
        if (hasBeenHit == false && other.CompareTag("Player"))
        {
            hasBeenHit = true;
            GameManager.SetSpawn(this);
                
            foreach (var obj in _hidableObjects)
            {
                obj.SetActive(false);
            }
        }
    }
}
