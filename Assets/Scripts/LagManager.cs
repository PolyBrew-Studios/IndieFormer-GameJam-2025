using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LagManager : MonoBehaviour
{
    public float lagIntervalMin = 2f;
    public float lagIntervalMax = 5f;

    void Start()
    {
        StartCoroutine(LagSpikeRoutine());
    }

    IEnumerator LagSpikeRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(lagIntervalMin, lagIntervalMax));
            StartCoroutine(LagSpike());
        }
    }

    IEnumerator LagSpike()
    {
        GameObject[] laggableObjects = GameObject.FindGameObjectsWithTag("Laggable");
        Debug.Log("Lag Spike! Found " + laggableObjects.Length + " laggable objects.");
        List<ILaggable> laggables = new List<ILaggable>();

        foreach (GameObject obj in laggableObjects)
        {
            ILaggable laggable = obj.GetComponent<ILaggable>();
            if (laggable != null)
            {
                laggables.Add(laggable);
                laggable.OnLagStart();
            }
        }

        yield return new WaitForSeconds(Random.Range(0.5f, 3.0f));

        foreach (ILaggable laggable in laggables)
        {
            laggable.OnLagEnd();
        }
    }
}
