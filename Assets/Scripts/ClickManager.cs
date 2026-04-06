using UnityEngine;
using UnityEngine.InputSystem;

public class ClickManager : MonoBehaviour
{
    void Update()
    {
        if (Mouse.current == null || Camera.main == null)
        {
            return;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null)
            {
                Debug.Log("Click en: " + hit.collider.name);
                Interactable obj = hit.collider.GetComponent<Interactable>();
                if (obj != null)
                {
                    obj.Interact();
                }
            }
        }
    }
}