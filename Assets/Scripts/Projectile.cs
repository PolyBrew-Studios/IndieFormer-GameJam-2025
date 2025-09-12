using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour, ILaggable
{
    public float lifetime = 5f;

    private Rigidbody rb;
    private Vector3 savedVelocity;
    private bool isLagging = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        StartCoroutine(DeactivateRoutine());
    }

    IEnumerator DeactivateRoutine()
    {
        yield return new WaitForSeconds(lifetime);
        gameObject.SetActive(false);
    }

    public void OnLagStart()
    {
        if (isLagging) return;
        isLagging = true;
        savedVelocity = rb.linearVelocity;
        rb.isKinematic = true; // Also set to kinematic to stop physics interactions
    }

    public void OnLagEnd()
    {
        if (!isLagging) return;
        isLagging = false;
        rb.isKinematic = false;
        rb.linearVelocity = savedVelocity;
    }
}
