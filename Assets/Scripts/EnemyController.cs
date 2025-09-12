using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyController : MonoBehaviour, ILaggable
{
    public Transform player;
    public float fireRate = 1f;
    public float projectileSpeed = 10f;
    public string projectileTag = "Projectile";
    public float jitterAmount = 0.1f;

    private bool canFire = true;
    private List<Vector3> positionHistory = new List<Vector3>();
    private const int POSITION_HISTORY_COUNT = 30;

    void Start()
    {
        StartCoroutine(RecordPositionRoutine());
        LagManager._event.AddListener(OnLag); // if you dont use pooling, you need to unsubscribe as well.
    }

    public void OnLag(LagPayload payload)
    {
        switch (payload.type)
        {
            case LagType.Freeze:
                StartCoroutine(FreezeRoutine(payload.duration));
                break;
            case LagType.Jitter:
                StartCoroutine(JitterRoutine(payload.duration));
                break;
            case LagType.PositionReset:
                if (positionHistory.Count > 0)
                {
                    transform.position = positionHistory[Random.Range(0, positionHistory.Count)];
                }
                break;
        }
    }

    void Update()
    {
        if (canFire && Time.time >= nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + 1f / fireRate;
        }
    }

    private float nextFireTime;

    void Fire()
    {
        if (player == null) return;

        GameObject projectile = ObjectPooler.Instance.SpawnFromPool(projectileTag, transform.position, Quaternion.identity);
        if (projectile != null)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            projectile.GetComponent<Rigidbody>().linearVelocity = direction * projectileSpeed;
        }
    }

    private IEnumerator RecordPositionRoutine()
    {
        while (true)
        {
            if (positionHistory.Count >= POSITION_HISTORY_COUNT)
            {
                positionHistory.RemoveAt(0);
            }
            positionHistory.Add(transform.position);
            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator FreezeRoutine(float duration)
    {
        Debug.Log("Enemy " + gameObject.GetInstanceID() + " Freeze STARTED");
        canFire = false;
        yield return new WaitForSeconds(duration);
        canFire = true;
        Debug.Log("Enemy " + gameObject.GetInstanceID() + " Freeze ENDED");
    }

    private IEnumerator JitterRoutine(float duration)
    {
        Vector3 originalPosition = transform.position;
        float timer = 0f;

        while (timer < duration)
        {
            float x = Random.Range(-1f, 1f) * jitterAmount;
            float z = Random.Range(-1f, 1f) * jitterAmount;
            transform.position = originalPosition + new Vector3(x, 0, z);
            timer += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPosition;
    }
}
