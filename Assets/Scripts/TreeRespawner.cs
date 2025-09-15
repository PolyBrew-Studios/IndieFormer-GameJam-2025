using UnityEngine;

public class TreeRespawner : MonoBehaviour
{
    public GameObject treePrefab;
    public static bool respawnTree = false;
    public GameObject treeToBeReplaced;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (respawnTree)
        {
            respawnTree = false;
            Destroy(treeToBeReplaced);
            var g = Instantiate(treePrefab, transform.position, transform.rotation);
            treeToBeReplaced = g;
        }
    }
}
