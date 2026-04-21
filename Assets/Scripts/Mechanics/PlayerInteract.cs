using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    public float range = 4f;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, range))
            {
                Tree tree = hit.collider.GetComponent<Tree>();

                if (tree != null)
                {
                    tree.Zap();
                    return;
                }

                Rock rock = hit.collider.GetComponent<Rock>();

                if (rock != null)
                {
                    rock.Zap();
                }
            }
        }
    }
}