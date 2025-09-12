using System;
using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour, ILaggable
{
    public float lifetime = 5f;

    private Rigidbody rb;
    private Vector3 savedVelocity;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        LagManager._event.AddListener(OnLag); 
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
            StartCoroutine(FreezeRoutine(payload.duration));
        }
    }

    private IEnumerator FreezeRoutine(float duration)
    {
        savedVelocity = rb.linearVelocity;
        Debug.Log("Projectile " + gameObject.GetInstanceID() + " FREEZE START. Velocity saved: " + savedVelocity);
        rb.linearVelocity = Vector3.zero;

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = savedVelocity;
        Debug.Log("Projectile " + gameObject.GetInstanceID() + " FREEZE END. Velocity restored: " + savedVelocity);
    }
}
