using System;
using UnityEngine;

public class ColliderNotifier : MonoBehaviour
{
    public Action ColliderHit { get; set; }
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
            ColliderHit?.Invoke();
    }
}
