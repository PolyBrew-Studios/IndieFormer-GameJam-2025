using UnityEngine;

public class test : MonoBehaviour
{
    [SerializeField] private Transform _bikePivot;
    [SerializeField] private float _bikeFallRotation_L;
    [SerializeField] private float _bikeFallRotation_R;
    
    private VehicleController _vehicleController;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _vehicleController = GetComponent<VehicleController>();
        
    }

    // Update is called once per frame
    void Update()
    {
        
        if (Input.GetKey(KeyCode.E))
        {
            // // _vehicleController.  
            //     
            //     Quaternion spinRotation = Quaternion.AngleAxis(
            //         // _angle,
            //         Vector3.right
            //     );
            //
            // gameObject.transform.localRotation = steeringRotation * spinRotation;
        }
    }
}
