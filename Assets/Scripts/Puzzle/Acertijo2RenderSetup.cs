using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Garantiza una cámara activa en cada display usado por el editor (evita "No Cameras Rendering").
/// </summary>
[DefaultExecutionOrder(-200)]
public class Acertijo2RenderSetup : MonoBehaviour
{
    private static readonly Color ColorFondo = new(0.12f, 0.14f, 0.18f, 1f);

    private void Awake()
    {
        AsegurarCamaraDisplay(0, esPrincipal: true);

        for (int display = 1; display < 8; display++)
        {
            if (!ExisteCamaraEnDisplay(display))
                AsegurarCamaraDisplay(display, esPrincipal: false);
        }
    }

    private static bool ExisteCamaraEnDisplay(int display)
    {
        Camera[] camaras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
        foreach (Camera camara in camaras)
        {
            if (camara.enabled && camara.gameObject.activeInHierarchy && camara.targetDisplay == display)
                return true;
        }

        return false;
    }

    private static void AsegurarCamaraDisplay(int display, bool esPrincipal)
    {
        Camera camara = esPrincipal ? Camera.main : null;
        if (camara == null)
        {
            foreach (Camera candidata in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
            {
                if (candidata.targetDisplay == display && candidata.enabled)
                {
                    camara = candidata;
                    break;
                }
            }
        }

        if (camara == null)
        {
            var go = new GameObject(esPrincipal ? "Main Camera" : $"Display{display + 1} Camera");
            if (esPrincipal)
            {
                go.tag = "MainCamera";
                if (go.GetComponent<AudioListener>() == null)
                    go.AddComponent<AudioListener>();
            }

            camara = go.AddComponent<Camera>();
            if (esPrincipal && go.GetComponent<UniversalAdditionalCameraData>() == null)
                go.AddComponent<UniversalAdditionalCameraData>();
        }
        else if (esPrincipal && camara.gameObject.tag != "MainCamera")
        {
            camara.gameObject.tag = "MainCamera";
        }

        ConfigurarCamara(camara, display, esPrincipal);
    }

    private static void ConfigurarCamara(Camera camara, int display, bool esPrincipal)
    {
        Transform t = camara.transform;
        t.position = new Vector3(0f, 0f, -10f);

        camara.targetDisplay = display;
        camara.clearFlags = CameraClearFlags.SolidColor;
        camara.backgroundColor = ColorFondo;
        camara.orthographic = true;
        camara.orthographicSize = 5f;
        camara.nearClipPlane = 0.3f;
        camara.farClipPlane = 1000f;
        camara.depth = esPrincipal ? -1f : -2f;
        camara.cullingMask = esPrincipal ? -1 : 0;
        camara.enabled = true;
        camara.gameObject.SetActive(true);
    }
}
