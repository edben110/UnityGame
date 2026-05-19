using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Events;
#endif

/// <summary>
/// Enlaza cada celda del puzzle con SlidingPuzzleController (On Click en Inspector).
/// </summary>
[RequireComponent(typeof(Button))]
public class SlidingPuzzleTile : MonoBehaviour
{
    [SerializeField] private SlidingPuzzleController controlador;
    [SerializeField] private int indiceCelda;

    private Button boton;

    private void Awake()
    {
        boton = GetComponent<Button>();

        if (controlador == null)
            controlador = FindFirstObjectByType<SlidingPuzzleController>();
    }

    public void Configurar(SlidingPuzzleController ctrl, int indice)
    {
        controlador = ctrl;
        indiceCelda = indice;
    }

    public void PulsarCelda()
    {
        if (controlador == null)
            controlador = FindFirstObjectByType<SlidingPuzzleController>();

        if (controlador != null)
            controlador.OnCeldaPulsada(indiceCelda);
    }

#if UNITY_EDITOR
    private void Reset()
    {
        boton = GetComponent<Button>();
        controlador = FindFirstObjectByType<SlidingPuzzleController>();

        string nombre = gameObject.name;
        if (nombre.StartsWith("Celda") && int.TryParse(nombre.Substring(5), out int n))
            indiceCelda = n;

        if (boton == null)
            return;

        while (boton.onClick.GetPersistentEventCount() > 0)
            UnityEventTools.RemovePersistentListener(boton.onClick, 0);

        UnityEventTools.AddVoidPersistentListener(boton.onClick, PulsarCelda);
        EditorUtility.SetDirty(boton);
    }
#endif
}
