using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public bool hasFallenOver = false; // game over
    public static Checkpoint latestCheckpoint; 
    public VehicleController playerVehicle;

    public GameObject PauseMenu;
    
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
        if (Input.GetKeyUp(KeyCode.Escape) || Input.GetKeyUp(KeyCode.Tab))
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        PauseMenu.SetActive(!PauseMenu.activeSelf);
        if (PauseMenu.activeSelf)
        {
            Time.timeScale = 0f;
                
        }
        else
        {
            Time.timeScale = 1f;
        }
    }

    public void QuitGame()
    {
#if UNITY_STANDALONE_OSX
        Application.Quit();
#endif
        
#if UNITY_STANDALONE_WIN
        Application.Quit();
#endif

#if UNITY_WEBGL
    // do nothing
#endif
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


    public void BackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);

    }
    
    
    
}
