using UnityEngine;

public class BoulderStage : MonoBehaviour
{
    [SerializeField] GameObject _boulders;

    GameObject _lastInstance;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && _lastInstance==null)
        {
            _lastInstance = Instantiate(_boulders);
        }
    }
    private void Start()
    {
        Checkpoint.CheckpointRespawn += () => 
        {
            Destroy(_lastInstance);
        };
    }
}
