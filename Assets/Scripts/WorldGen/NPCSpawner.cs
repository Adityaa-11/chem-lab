using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    public GameObject npcPrefab;
    public Vector2 areaSize = new Vector2(40, 40);
    public LayerMask groundMask;
    public LayerMask waterMask;
    public int maxAttempts = 40;
    public float minDistanceFromPlayer = 10f;

    void Start()
    {
        if (npcPrefab == null)
        {
            Debug.LogWarning("NPCSpawner has no prefab assigned.");
            return;
        }

        Vector3 playerPos = Vector3.zero;
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) playerPos = p.transform.position;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Vector3 origin = new Vector3(
                Random.Range(-areaSize.x, areaSize.x),
                200f,
                Random.Range(-areaSize.y, areaSize.y)
            );

            Ray ray = new Ray(origin, Vector3.down);
            RaycastHit[] hits = Physics.RaycastAll(ray, 500f);
            if (hits.Length == 0) continue;

            RaycastHit? groundHit = null;
            RaycastHit? waterHit = null;

            foreach (var hit in hits)
            {
                if (((1 << hit.collider.gameObject.layer) & groundMask) != 0)
                    groundHit = hit;
                if (((1 << hit.collider.gameObject.layer) & waterMask) != 0)
                    waterHit = hit;
            }

            if (!groundHit.HasValue) continue;
            if (waterHit.HasValue && waterHit.Value.distance < groundHit.Value.distance) continue;

            Vector3 pos = groundHit.Value.point;
            if ((pos - playerPos).sqrMagnitude < minDistanceFromPlayer * minDistanceFromPlayer)
                continue;

            Instantiate(npcPrefab, pos, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
            return;
        }

        Debug.LogWarning($"NPCSpawner: gave up after {maxAttempts} attempts — check groundMask.");
    }
}
