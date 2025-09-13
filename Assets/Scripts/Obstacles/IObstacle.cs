using UnityEngine;

public interface IObstacle
{
    // Called when something touches this obstacle (eg. the player/car)
    void OnTouch(GameObject toucher);
}