using System;
using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour, ILaggable
{
    public float lifetime = 5f;

    private Rigidbody rb;
    private Vector3 savedVelocity;
    private Coroutine freezeRoutine;
    private float freezeEndTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        LagManager._event.AddListener(OnLag);
    }

    private void OnDisable()
    {
        LagManager._event.RemoveListener(OnLag);
    }

    // void OnEnable()
    // {
    //     StartCoroutine(DeactivateRoutine());
    // }

    // void OnDisable()
    // {
    //     rb.isKinematic = false;
    // }

    // IEnumerator DeactivateRoutine()
    // {
    //     yield return new WaitForSeconds(lifetime);
    //     gameObject.SetActive(false);
    // }

    public void OnLag(LagPayload payload)
    {
        if (payload.type == LagType.Freeze)
        {
            // Extend existing freeze or start a new one without stacking multiple coroutines
            if (freezeRoutine == null)
            {
                savedVelocity = rb != null ? rb.linearVelocity : Vector3.zero;
                if (rb != null) rb.linearVelocity = Vector3.zero;
                freezeEndTime = Time.time + payload.duration;
                freezeRoutine = StartCoroutine(FreezeRoutine());
            }
            else
            {
                freezeEndTime += payload.duration;
            }
        }
    }

    private IEnumerator FreezeRoutine()
    {
        while (Time.time < freezeEndTime)
        {
            yield return null;
        }

        if (rb != null)
        {
            rb.linearVelocity = savedVelocity;
        }
        freezeRoutine = null;
    }
}
