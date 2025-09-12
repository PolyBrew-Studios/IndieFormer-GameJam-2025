using UnityEngine;

public class test : MonoBehaviour
{
    [SerializeField] private Transform _bikePivot;
    [SerializeField] private float _bikeFallRotation_L;
    [SerializeField] private float _bikeFallRotation_R;
    
    private VehicleController _vehicleController;
    private Rigidbody _rigidbody;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _vehicleController = GetComponent<VehicleController>();
        
    }

    // Update is called once per frame
    void Update()
    {
        
        // TODO: Move into PlayerFailingScript or something
        // Slowly lerp the bike to the ground if no input was given for a few seconds
        // Meaning slowly lerping the values below.
        // if player moves in the meantime lerp back to the original values
        
        
        if (Input.GetKey(KeyCode.E))
        {
                Quaternion spinRotation = Quaternion.AngleAxis(
                    _bikeFallRotation_L,
                    Vector3.forward
                );
                _vehicleController.maxSteeringAngle = 120;
                _vehicleController.springStiffness = 5000;
                _vehicleController.damperStiffness = 5000;
            gameObject.transform.localRotation = gameObject.transform.localRotation * spinRotation;
        }
    }
}
