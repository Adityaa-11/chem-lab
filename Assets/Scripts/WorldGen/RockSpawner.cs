using UnityEngine;

public class RockSpawner : MonoBehaviour
{
    public GameObject[] rockPrefabs;
    public int rockCount = 200;
    public Vector2 areaSize = new Vector2(100, 100);

    public LayerMask groundMask;
    public LayerMask waterMask;

    public Vector2 scaleRange = new Vector2(0.6f, 1.8f);

    void Start()
    {
        for (int i = 0; i < rockCount; i++)
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
                GameObject prefab = rockPrefabs[Random.Range(0, rockPrefabs.Length)];
                GameObject rock = Instantiate(
                    prefab,
                    groundHit.Value.point,
                    Quaternion.Euler(0f, Random.Range(0f, 360f), 0f)
                );
                float s = Random.Range(scaleRange.x, scaleRange.y);
                rock.transform.localScale = Vector3.one * s;
            }
        }
    }
}
