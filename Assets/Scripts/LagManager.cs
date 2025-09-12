using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Events;

public class LagManager : MonoBehaviour
{
        
    [Header("General Settings")]
    public float lagIntervalMin = 2f;
    public float lagIntervalMax = 5f;
    public float lagFreezeTimeMin = 5f;
    public float lagFreezeTimeMax = 5f;
    
    [SerializeField]
    public List<LagType> usedLagTypes;
    
    [Header("Player Lag Settings")]
    public bool playerIsLaggable = false;
    public PlayerController player;
    public float positionRecordTime = 0.1f;
    public int positionHistoryCount = 50;
    private List<Vector3> recentPositions = new List<Vector3>();

    private LagType[] lagTypes;

    public static UnityEvent<LagPayload> _event = new UnityEvent<LagPayload>();

    private WaitForSeconds recordWait;
    
    void Start()
    {
        lagTypes = (LagType[])Enum.GetValues(typeof(LagType));
        recordWait = new WaitForSeconds(positionRecordTime);
        StartCoroutine(LagSpikeRoutine());
        if (playerIsLaggable)
        {
            StartCoroutine(RecordPosition());
        }
        
    }

    IEnumerator RecordPosition()
    {
        while (true)
        {
            if (player != null && recentPositions.Count < positionHistoryCount)
            {
                recentPositions.Add(player.transform.position);
            }
            else if (player != null)
            {
                recentPositions.RemoveAt(0);
                recentPositions.Add(player.transform.position);
            }
            yield return recordWait;
        }
    }

    IEnumerator LagSpikeRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(lagIntervalMin, lagIntervalMax));
            yield return LagSpike();
        }
    }

    IEnumerator LagSpike()
    {
        if (playerIsLaggable)
        {
            // Player-centric lag
            if (recentPositions.Count > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, recentPositions.Count);
                player.transform.position = recentPositions[randomIndex];
            }
            yield return null;
        }
        else
        {
            // Environmental lag
            LagPayload payload = new LagPayload();
            if (usedLagTypes == null || usedLagTypes.Count == 0)
            {
                payload.type = lagTypes[UnityEngine.Random.Range(0, lagTypes.Length)];
            }
            else
            {
                var x = UnityEngine.Random.Range(0, usedLagTypes.Count); // upper bound exclusive
                payload.type = usedLagTypes[x];
            }
                
            payload.duration = UnityEngine.Random.Range(lagFreezeTimeMin, lagFreezeTimeMax);
            if (Debug.isDebugBuild)
            {
                Debug.Log($"[LagManager] Environmental Lag Spike! Type: {payload.type}, Duration: {payload.duration:F2}");
            }

            // Invoke once: listeners will handle themselves; avoid repeated global invocations and Find costs
            _event.Invoke(payload);
        }
    }
}
