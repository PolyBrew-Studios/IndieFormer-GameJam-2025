using UnityEngine;

public class GameManager : MonoBehaviour
{
    public bool hasFallenOver = false; // game over
    public Vector3 latestCheckpointPosition; 
    public Quaternion latestCheckpointRotation;
    public VehicleController playerVehicle;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        latestCheckpointPosition = playerVehicle.transform.position;
        latestCheckpointRotation = playerVehicle.transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
