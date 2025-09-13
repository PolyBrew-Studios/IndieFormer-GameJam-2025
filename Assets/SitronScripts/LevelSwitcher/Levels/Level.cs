using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Level", order = 1)]
public class Level : ScriptableObject
{
    [field: SerializeField] public int NextSceneIndex { get; set; }
    [field:SerializeField] public float ThreeStarTreshold { get; set; }
    [field: SerializeField] public float TwoStarTreshold { get; set; }
    [field: SerializeField] public float OneStarTreshold { get; set; }

}
