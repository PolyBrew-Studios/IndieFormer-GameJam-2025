using System;
using UnityEngine;

public class ColliderNotifier : MonoBehaviour
{
    public Action ColliderHit { get; set; }
    private void OnTriggerEnter(Collider other)
    {
        ColliderHit?.Invoke();
    }
}
