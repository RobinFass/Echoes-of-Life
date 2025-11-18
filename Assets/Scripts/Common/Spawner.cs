using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private GameObject[] prefab;
    [SerializeField] private SpriteRenderer ground;
    [SerializeField] private int amount;
    [SerializeField] private LayerMask spawnMask;


    private Transform spawnedParent;

    private void Start()
    {
        var folder = GameObject.Find("SpawnedObject");
        if (!folder) folder = new GameObject("SpawnedObject");
        spawnedParent = folder.transform;
        for (var i = 0; i < amount; i++)
        {
            var toSpawn = prefab[Random.Range(0, prefab.Length)];
            var xSize = toSpawn.GetComponent<SpriteRenderer>().bounds.size.x * 2;
            var ySize = toSpawn.GetComponent<SpriteRenderer>().bounds.size.y * 2;
            var prefabRadius = Mathf.Max(xSize * 2, ySize * 2);
        
            Vector2 startPos;
            var tries = 0;
            do
            {
                startPos = GetRandomPosition(ground.bounds);
                tries++;
            } while (Physics2D.OverlapCircle(startPos, prefabRadius, spawnMask) && tries < 50);

            Instantiate(toSpawn, startPos, Quaternion.identity, spawnedParent);
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