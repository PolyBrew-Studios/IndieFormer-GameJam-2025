using UnityEngine;

public class GameManager : MonoBehaviour
{
    public bool hasFallenOver = false; // game over
    public static Checkpoint latestCheckpoint; 
    public VehicleController playerVehicle;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var chkpnt = new GameObject("StartingCheckpoint");
        Checkpoint chpnt = chkpnt.AddComponent<Checkpoint>();
        chpnt._vehicleController = playerVehicle;
        chpnt.playerForceFieldAdjuster = FindAnyObjectByType<PlayerForceFieldAdjuster>();
        // create checkpoint onstart of the level
        latestCheckpoint = Instantiate(chpnt, playerVehicle.transform.position, Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void SetSpawn(Checkpoint checkpoint)
    {
       latestCheckpoint = checkpoint;
    }
    
    public static void Respawn()
    {
        Respawn(latestCheckpoint);
    }
    
    public static void Respawn(Checkpoint checkpoint)
    {
        checkpoint.RespawnFromCheckpoint();
        
    }
}
