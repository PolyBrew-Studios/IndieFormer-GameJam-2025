using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveCarPrefabSelector : MonoBehaviour
{
    [SerializeField] List<VehicleController> _vehicleLevels;
    [SerializeField] List<_CameraSettings> _cameraSettings;

    // [SerializeField] private PlayerInput playerInputRef;
    [SerializeField] private CameraController cameraControllerRef;



    private void Awake()
    {
    }

    [field:SerializeField] public VehicleController LatestController { get; set; }
    public Vector3 LatestVehicleStartingLocation { get; set; }
    void Start()
    {
        // GameManager.Instance.PlayerLeveledUp += OnPlayerLeveledUp;

        // playerInputRef.SetControlledVehicle(LatestController);

    }
    // void OnPlayerLeveledUp()
    // {
    //     VehicleController newActiveController = _vehicleLevels[0];
    //     LatestVehicleStartingLocation = LatestController.transform.position;
    //
    //     newActiveController.gameObject.transform.SetPositionAndRotation(LatestController.gameObject.transform.position, LatestController.gameObject.transform.rotation);
    //     newActiveController.gameObject.SetActive(true);
    //     newActiveController.TransferRigidBodyParameters(LatestController.GetRigidBody());
    //
    //     LatestController.gameObject.SetActive(false);
    //     LatestController = newActiveController;
    //
    //     playerInputRef.SetControlledVehicle(LatestController);
    //     cameraControllerRef.SetTarget(LatestController.transform, _cameraSettings[0]);
    //
    //     // GameManager.Instance.SetActiveVehicle(LatestController); // was used to set speedometer, maybe necessary for gear shifts?
    //
    // }

    // Update is called once per frame
    void Update()
    {
        
    }
}
