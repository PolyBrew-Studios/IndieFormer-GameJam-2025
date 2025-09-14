using UnityEngine;

public class BoulderStage : MonoBehaviour
{
    [SerializeField] GameObject _boulders;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            Instantiate(_boulders);
    }
}
