using System;
using NUnit.Framework.Constraints;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private Transform _checkpointSpawnPoint;
    [SerializeField] private bool hasBeenHit = false;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (_checkpointSpawnPoint == null)
        {
            _checkpointSpawnPoint = transform;
        }
    }
    

    private void OnTriggerEnter(Collider other)
    {
        if (hasBeenHit == false && other.CompareTag("Player"))
        {
            hasBeenHit = true;
        }
    }
}
