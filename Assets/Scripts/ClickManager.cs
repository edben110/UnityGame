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

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
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
        if (WorldInteractionGate.BlocksMapPointAndClick)
        {
            return false;
        }

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