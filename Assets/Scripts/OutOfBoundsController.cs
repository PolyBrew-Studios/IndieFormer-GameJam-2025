using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class OutOfBoundsController : MonoBehaviour
{
    [SerializeField] ActiveCarPrefabSelector _ultimatePlayer;
     [SerializeField] private int WhenToResetUpwards = 100;
     [SerializeField] private int WhenToResetDownwards = -30;
     [SerializeField] float startingHeightOffset = 5;

    private Vector3 startPosition;
    public PlayerForceFieldAdjuster playerForceFieldAdjuster;
    private Quaternion startRotation;

    // Use this for initialization
    void Start()
    {
        var cPos = _ultimatePlayer.LatestController.transform.position;
        startPosition = new Vector3(cPos.x,cPos.y,cPos.z);
        startPosition.y += startingHeightOffset;

        // Use a valid identity rotation by default
        startRotation = Quaternion.identity;
    }

    // Update is called once per frame
    void Update()
    {
        if ((_ultimatePlayer.LatestController.transform.position.y < WhenToResetDownwards) || (_ultimatePlayer.LatestController.transform.position.y > WhenToResetUpwards))
        {
            var controller = _ultimatePlayer.LatestController.transform;
            controller.position = startPosition;

            // Reset rotation to be upright (align up with world Y) while preserving current yaw
            Vector3 euler = controller.eulerAngles;
            Quaternion uprightRotation = Quaternion.Euler(0f, euler.y, 0f);
            controller.rotation = uprightRotation;

            playerForceFieldAdjuster.ResetPlayer();
        }
    }
}

