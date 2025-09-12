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
    
    void Start()
    {
        lagTypes = (LagType[])Enum.GetValues(typeof(LagType));
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
            yield return new WaitForSeconds(positionRecordTime);
        }
    }

    IEnumerator LagSpikeRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(lagIntervalMin, lagIntervalMax));
            StartCoroutine(LagSpike());
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
            if(usedLagTypes.Count == 0)
                payload.type = lagTypes[UnityEngine.Random.Range(0, lagTypes.Length)];
            else
            {
                var max = usedLagTypes.Count - 1;
                var x = UnityEngine.Random.Range(0, max);
                payload.type = usedLagTypes[x];
            }
                
            
            payload.duration = UnityEngine.Random.Range(lagFreezeTimeMin, lagFreezeTimeMax);
            Debug.Log("[LagManager] Environmental Lag Spike! Type: " + payload.type + ", Duration: " + payload.duration.ToString("F2"));

            

            GameObject[] laggableObjects = GameObject.FindGameObjectsWithTag("Laggable");

            foreach (GameObject obj in laggableObjects)
            {
                _event.Invoke(payload);
                // obj.SendMessage("OnLag", payload, SendMessageOptions.DontRequireReceiver);
            }
        }
    }
}
