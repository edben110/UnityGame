using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;

public class ClickManager : MonoBehaviour
{
    private Camera cachedCamera;

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

            // Seleccionar el hit más cercano (menor distance)
            RaycastHit2D closestHit = new RaycastHit2D();
            float closestDistance = float.MaxValue;

            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider != null && hit.distance < closestDistance)
                {
                    closestDistance = hit.distance;
                    closestHit = hit;
                }
            }

            if (closestHit.collider != null)
            {
                Debug.Log($"Click en: {closestHit.collider.name} (distance: {closestDistance:F3})");
                Interactable obj = closestHit.collider.GetComponent<Interactable>();
                if (obj != null)
                {
                    obj.Interact();
                }
                else
                {
                    Debug.LogWarning($"No Interactable found on {closestHit.collider.name}");
                }
            }
        }
    }

    private Camera GetActiveCamera()
    {
        if (cachedCamera != null && cachedCamera.isActiveAndEnabled)
        {
            return cachedCamera;
        }

        if (Camera.main != null)
        {
            cachedCamera = Camera.main;
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