using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private SpriteRenderer ground;
    [SerializeField] private int amount;
    [SerializeField] private LayerMask spawnMask;


    private Transform spawnedParent;

    private void Start()
    {
        var folder = GameObject.Find("SpawnedObject");
        if (folder == null)
        {
            folder = new GameObject("SpawnedObject");
        }
        spawnedParent = folder.transform;
        
        var xSize = prefab.GetComponent<SpriteRenderer>().bounds.size.x*2;
        var ySize = prefab.GetComponent<SpriteRenderer>().bounds.size.y*2;
        var prefabRadius = Mathf.Max(xSize*2, ySize*2);
        
        for (var i = 0; i < amount; i++)
        {
            Vector2 startPos;
            var tries = 0;
            do
            {
                startPos = GetRandomPosition(ground.bounds);
                tries++;
            }
            while (Physics2D.OverlapCircle(startPos, prefabRadius, spawnMask) && tries < 50);

            Instantiate(prefab, startPos, Quaternion.identity, spawnedParent);
        }
    }

    private Vector2 GetRandomPosition(Bounds bounds)
    {
        var wallThicknes = 2;
        return new Vector2(
            Random.Range(bounds.min.x + wallThicknes, bounds.max.x - wallThicknes),
            Random.Range(bounds.min.y + wallThicknes, bounds.max.y - wallThicknes)
        );
    }
}