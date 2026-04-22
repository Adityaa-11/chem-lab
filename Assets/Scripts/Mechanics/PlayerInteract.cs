using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    public float range = 4f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (DialogUI.IsOpen)
            return;
        if (!Input.GetMouseButtonDown(0))
            return;

        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                interactable.Interact();
                return;
            }

            Chemist chemist = hit.collider.GetComponentInParent<Chemist>();
            if (chemist != null)
            {
                chemist.Talk();
            }
        }
    }
}

public interface IInteractable
{
    void Interact();
}