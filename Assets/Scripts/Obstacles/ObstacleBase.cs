using UnityEngine;

/// <summary>
/// Base class for all obstacles. Provides a unified OnTouch entry point and
/// default forwarding from collision/trigger events.
/// Also includes helper to affect the player (e.g., mark them as fallen).
/// </summary>
public abstract class ObstacleBase : MonoBehaviour
{
  
    protected virtual void OnCollisionEnter(Collision collision)
    {
        TriggerPlayerFall(collision.gameObject);
    }



    /// <summary>
    /// Attempts to find a PlayerForceFieldAdjuster on the toucher or its parents
    /// and enables the fallen state.
    /// </summary>
    protected void TriggerPlayerFall(GameObject toucher)
    {
        if (toucher == null) return;
        var adjuster = toucher.GetComponentInParent<PlayerForceFieldAdjuster>();
        if (adjuster != null)
        {
            adjuster.EnableFallenOff();
        }
    }
}
