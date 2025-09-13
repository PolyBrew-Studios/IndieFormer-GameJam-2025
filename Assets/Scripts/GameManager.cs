using UnityEngine;

public class GameManager : MonoBehaviour
{
    public bool hasFallenOver = false; // game over
    public Transform latestCheckpointPosition; 
    public Transform latestCheckpointRotation;
    public VehicleController playerVehicle;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // latestCheckpointPosition = play
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
