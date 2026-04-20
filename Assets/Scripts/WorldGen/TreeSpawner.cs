using UnityEngine;

public class TreeSpawner : MonoBehaviour
{
    public GameObject[] treePrefabs;
    public int treeCount = 200;
    public Vector2 areaSize = new Vector2(100, 100);

    public LayerMask groundMask;
    public LayerMask waterMask;

    void Start()
    {
        for (int i = 0; i < treeCount; i++)
        {
            Vector3 pos = new Vector3(
                Random.Range(-areaSize.x, areaSize.x),
                200f,
                Random.Range(-areaSize.y, areaSize.y)
            );

            Ray ray = new Ray(pos, Vector3.down);
            RaycastHit[] hits = Physics.RaycastAll(ray, 500f);

            if (hits.Length == 0)
                continue;

            RaycastHit? groundHit = null;
            RaycastHit? waterHit = null;

            foreach (var hit in hits)
            {
                if (((1 << hit.collider.gameObject.layer) & groundMask) != 0)
                    groundHit = hit;

                if (((1 << hit.collider.gameObject.layer) & waterMask) != 0)
                    waterHit = hit;
            }
            if (waterHit.HasValue && groundHit.HasValue)
            {
                if (waterHit.Value.distance < groundHit.Value.distance)
                    continue;
            }
            if (groundHit.HasValue)
            {
                GameObject prefab = treePrefabs[Random.Range(0, treePrefabs.Length)];
                Instantiate(prefab, groundHit.Value.point, Quaternion.identity);
            }
        }
    }
}