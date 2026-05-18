using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Events;
#endif

/// <summary>
/// Enlaza un botón UI con AcertijoController (visible en On Click del Inspector).
/// </summary>
[RequireComponent(typeof(Button))]
public class AcertijoBotonEnlace : MonoBehaviour
{
    [SerializeField] private AcertijoController controlador;
    [SerializeField] private int numeroBoton = 1;

    private Button boton;

    private void Awake()
    {
        boton = GetComponent<Button>();

        if (controlador == null)
            controlador = FindFirstObjectByType<AcertijoController>();

        if (numeroBoton <= 0)
        {
            string nombre = gameObject.name;
            if (nombre.StartsWith("Boton") && int.TryParse(nombre.Substring(5), out int n))
                numeroBoton = n;
        }
    }

    /// <summary>Conectado en On Click() del Button (Inspector). No duplicar listeners en código.</summary>
    public void EnviarPulsacion()
    {
        if (controlador == null)
            controlador = FindFirstObjectByType<AcertijoController>();

        if (controlador != null)
            controlador.ProcesarPulsacion(numeroBoton);
    }

    public void Configurar(AcertijoController ctrl, int numero)
    {
        controlador = ctrl;
        numeroBoton = numero;
    }

#if UNITY_EDITOR
    private void Reset()
    {
        boton = GetComponent<Button>();
        controlador = FindFirstObjectByType<AcertijoController>();

        string nombre = gameObject.name;
        if (nombre.StartsWith("Boton") && int.TryParse(nombre.Substring(5), out int n))
            numeroBoton = n;

        if (boton == null)
            return;

        while (boton.onClick.GetPersistentEventCount() > 0)
            UnityEventTools.RemovePersistentListener(boton.onClick, 0);

        UnityEventTools.AddVoidPersistentListener(boton.onClick, EnviarPulsacion);
        EditorUtility.SetDirty(boton);
    }
#endif
}
