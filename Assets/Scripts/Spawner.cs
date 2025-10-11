using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private SpriteRenderer ground;
    [SerializeField] private int coinAmount;

    private Transform spawnedParent;

    private void Start()
    {
        var folder = GameObject.Find("SpawnedObject");
        if (folder == null)
        {
            folder = new GameObject("SpawnedObject");
        }
        spawnedParent = folder.transform;

        for (var i = 0; i < coinAmount; i++)
        {
            var startPos = GetRandomPosition(ground.bounds);
            Instantiate(prefab, startPos, Quaternion.identity, spawnedParent);
        }
    }

    private Vector2 GetRandomPosition(Bounds bounds)
    {
        var extraPadding = 2;
        var objectPadding = prefab.GetComponent<SpriteRenderer>().bounds.extents.x * extraPadding;
        return new Vector2(
            Random.Range(bounds.min.x + objectPadding, bounds.max.x - objectPadding),
            Random.Range(bounds.min.y + objectPadding, bounds.max.y - objectPadding)
        );
    }
}