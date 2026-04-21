using UnityEngine;

public class TreeSpawner : MonoBehaviour
{
    public GameObject[] treePrefabs;
    public GameObject[] mushroomPrefabs;
    public GameObject burntTreePrefab;
    public int treeCount = 200;
    public int burntTreeCount = 100;
    public Vector2 areaSize = new Vector2(100, 100);

    public LayerMask groundMask;
    public LayerMask waterMask;

    void Start()
    {
        for (int i = 0; i < treeCount; i++)
        {
            GameObject prefab = treePrefabs[Random.Range(0, treePrefabs.Length)];
            Vector3 basePos = new Vector3(
                Random.Range(-areaSize.x, areaSize.x),
                200f,
                Random.Range(-areaSize.y, areaSize.y)
            );
            Create(prefab, Quaternion.identity, basePos);
            if (Random.Range(1, 101) > 90)
            {
                Vector3 mushroomPos = basePos + new Vector3(
                    Random.Range(-2f, 2f),   // small horizontal offset
                    0f,
                    Random.Range(-2f, 2f)
                );
                Create(
                    mushroomPrefabs[Random.Range(0, mushroomPrefabs.Length)],
                    Quaternion.identity,
                    mushroomPos,
                    0.1f // small lift above ground
                );
            }
        }
        for (int i = 0; i < burntTreeCount; i++)
        {
            Vector3 pos = new Vector3(
                Random.Range(-areaSize.x, areaSize.x),
                200f,
                Random.Range(-areaSize.y, areaSize.y)
            );
            Quaternion rot = Quaternion.Euler(
                0f,
                Random.Range(0f, 360f),
                90f
            );
            Create(burntTreePrefab, rot, pos, 0.25f);
        }
    }
    void Create(GameObject thisObj, Quaternion thisQuat, Vector3 pos, float offset = 0)
    {

        Ray ray = new Ray(pos, Vector3.down);
        RaycastHit[] hits = Physics.RaycastAll(ray, 500f);

        if (hits.Length == 0)
            return;

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
                return;
        }
        if (groundHit.HasValue)
        {
            Instantiate(thisObj, groundHit.Value.point + Vector3.up * offset, thisQuat);
        }
    }
}