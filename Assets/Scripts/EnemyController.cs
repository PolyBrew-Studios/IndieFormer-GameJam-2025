using UnityEngine;

public class EnemyController : MonoBehaviour, ILaggable
{
    public Transform player;
    public float fireRate = 1f;
    public float projectileSpeed = 10f;
    public string projectileTag = "Projectile";

    private float nextFireTime;
    private bool isLagging = false;

    void Update()
    {
        if (!isLagging && Time.time >= nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + 1f / fireRate;
        }
    }

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

    public void OnLagStart()
    {
        isLagging = true;
    }

    public void OnLagEnd()
    {
        isLagging = false;
    }
}
