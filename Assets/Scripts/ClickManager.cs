using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;

public class ClickManager : MonoBehaviour
{
    private Camera cachedCamera;

    private void OnEnable()
    {
        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.RoomChanged += OnRoomChanged;
        }
    }

    private void Start()
    {
        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.RoomChanged -= OnRoomChanged;
            RoomManager.Instance.RoomChanged += OnRoomChanged;
        }
    }

    private void OnDisable()
    {
        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.RoomChanged -= OnRoomChanged;
        }
    }

    private void OnRoomChanged(string previousRoomId, string nextRoomId)
    {
        cachedCamera = null;
    }

    void Update()
    {
        Camera activeCamera = GetActiveCamera();
        if (Mouse.current == null || activeCamera == null)
        {
            return;
        }

        // --- Hover detection para cursor de mano sobre interactuables del mundo ---
        UpdateWorldHoverCursor(activeCamera);

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            // Bloquear clics del juego mientras el inventario está abierto
            if (InventoryOverlayCanvas.Instance != null && InventoryOverlayCanvas.Instance.IsOpen)
            {
                return;
            }

            if (IsPointerOverBlockingUi())
            {
                return;
            }

            Vector3 screenPos = Mouse.current.position.ReadValue();
            screenPos.z = Mathf.Abs(activeCamera.transform.position.z);
            Vector2 mousePos = activeCamera.ScreenToWorldPoint(screenPos);
            
            // Raycast que detecta múltiples hits
            RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.zero);

            RaycastHit2D selectedHit = new RaycastHit2D();
            Interactable selectedInteractable = null;
            float selectedDepth = float.MaxValue;

            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider == null)
                {
                    continue;
                }

                Interactable interactable = hit.collider.GetComponentInParent<Interactable>();
                if (!CanInteractWith(interactable))
                {
                    continue;
                }

                float depthFromCamera = Mathf.Abs(hit.collider.transform.position.z - activeCamera.transform.position.z);
                if (selectedInteractable == null || depthFromCamera < selectedDepth)
                {
                    selectedDepth = depthFromCamera;
                    selectedHit = hit;
                    selectedInteractable = interactable;
                }
            }

            if (selectedInteractable != null && selectedHit.collider != null)
            {
                Debug.Log($"Click en: {selectedHit.collider.name}");
                selectedInteractable.Interact();
            }
        }
    }

    private Camera GetActiveCamera()
    {
        if (cachedCamera != null && cachedCamera.isActiveAndEnabled)
        {
            return cachedCamera;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera != null && mainCamera.isActiveAndEnabled)
        {
            cachedCamera = mainCamera;
            return cachedCamera;
        }

        Camera any = FindAnyObjectByType<Camera>();
        if (any != null && any.isActiveAndEnabled)
        {
            cachedCamera = any;
            return cachedCamera;
        }

        return null;
    }

    private static bool CanInteractWith(Interactable interactable)
    {
        if (interactable == null || !interactable.isActiveAndEnabled || !interactable.gameObject.activeInHierarchy)
        {
            return false;
        }

        if (RoomManager.Instance == null)
        {
            return true;
        }

        return RoomManager.Instance.IsObjectInCurrentRoom(interactable.gameObject);
    }

    /// <summary>
    /// Detecta si el mouse está sobre un Interactable del mundo (hotspot, NPC, puerta)
    /// y cambia el cursor a mano. Si no hay interactuable debajo, restaura el cursor
    /// SOLO si no está sobre UI (para no interferir con CursorHoverUI).
    /// </summary>
    private void UpdateWorldHoverCursor(Camera activeCamera)
    {
        // Si el inventario está abierto, no hacer hover del mundo
        if (InventoryOverlayCanvas.Instance != null && InventoryOverlayCanvas.Instance.IsOpen)
        {
            return;
        }

        // Si el mouse está sobre UI interactuable, dejar que CursorHoverUI maneje el cursor
        if (IsPointerOverAnyUI())
        {
            return;
        }

        Vector3 screenPos = Mouse.current.position.ReadValue();
        screenPos.z = Mathf.Abs(activeCamera.transform.position.z);
        Vector2 mousePos = activeCamera.ScreenToWorldPoint(screenPos);

        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null)
        {
            Interactable interactable = hit.collider.GetComponentInParent<Interactable>();
            if (CanInteractWith(interactable))
            {
                CursorManager.SetHand();
                return;
            }
        }

        // No hay interactuable debajo: restaurar cursor
        if (CursorManager.IsHand)
        {
            CursorManager.SetDefault();
        }
    }

    /// <summary>
    /// Verifica si el puntero está sobre cualquier elemento UI con raycast activo.
    /// Usado para evitar que el hover del mundo interfiera con el hover de UI.
    /// </summary>
    private static bool IsPointerOverAnyUI()
    {
        if (EventSystem.current == null)
        {
            return false;
        }

        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Mouse.current.position.ReadValue()
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        return results.Count > 0;
    }

    private static bool IsPointerOverBlockingUi()
    {
        if (EventSystem.current == null)
        {
            return false;
        }

        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Mouse.current.position.ReadValue()
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        for (int i = 0; i < results.Count; i++)
        {
            GameObject currentObject = results[i].gameObject;
            if (currentObject == null)
            {
                continue;
            }

            if (currentObject.GetComponent<Button>() != null)
            {
                return true;
            }

            if (currentObject.GetComponent<UnityEngine.UI.Selectable>() != null)
            {
                CanvasGroup canvasGroup = currentObject.GetComponentInParent<CanvasGroup>();
                if (canvasGroup != null && canvasGroup.blocksRaycasts)
                {
                    return true;
                }

                if (currentObject.GetComponent<UnityEngine.UI.InputField>() != null || currentObject.GetComponent<TMPro.TMP_InputField>() != null)
                {
                    return true;
                }

                if (canvasGroup == null)
                {
                    return true;
                }
            }
        }

        return false;
    }
}