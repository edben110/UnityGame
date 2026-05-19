using System;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Rompecabezas deslizante 3x3 (8 piezas + espacio vacío).
/// Estado inicial fijo resoluble, mezclado con dificultad media.
/// </summary>
public class SlidingPuzzleController : MonoBehaviour
{
    private const string RutaSpritesRetrato = "Assets/Sprites/Puzzle/retrato_acertijo.png";

    public event Action PuzzleCompleted;
    private const int CantidadSpritesPiezas = 9;

    private const int Filas = 3;
    private const int Columnas = 3;
    private const int TotalCeldas = 9;
    private const int CeldaVacia = 0;

    /// <summary>Orden inicial (no resuelto, resoluble).</summary>
    private static readonly int[] EstadoInicial =
    {
        1, 4, 2,
        3, 5, 0,
        7, 8, 6
    };

    private static readonly int[] EstadoGanador =
    {
        1, 2, 3,
        4, 5, 6,
        7, 8, 0
    };

    [Header("Marco del acertijo")]
    [SerializeField] private Image marcoAcertijo;
    [SerializeField] private Sprite spriteMarcoNormal;
    [SerializeField] private Sprite spriteMarcoCompletado;
    [SerializeField] private Color colorMarcoCompletado = new Color(0.85f, 0.65f, 0.13f, 1f);

    [Header("Cuadrícula")]
    [SerializeField] private Transform contenedorCuadricula;
    [SerializeField] private Button[] botonesCeldas = new Button[TotalCeldas];

    [Header("Sprites del retrato (retrato_acertijo_0 … _8)")]
    [SerializeField] private Sprite[] spritesPiezas = new Sprite[CantidadSpritesPiezas];

    [Header("Apariencia de piezas")]
    [SerializeField] private Color colorCeldaVacia = new Color(1f, 1f, 1f, 0f);

    private readonly int[] cuadricula = new int[TotalCeldas];
    private bool acertijoResuelto;
    private Color colorMarcoNormalGuardado;

    private void Awake()
    {
        CargarSpritesPiezas();
        ResolverReferencias();
        InicializarCuadricula();
        ConfigurarBotones();
        RefrescarUI();
    }

    /// <summary>Llamado por SlidingPuzzleTile o On Click del Inspector.</summary>
    public void OnCeldaPulsada(int indiceCelda)
    {
        if (acertijoResuelto || indiceCelda < 0 || indiceCelda >= TotalCeldas)
            return;

        if (cuadricula[indiceCelda] == CeldaVacia)
            return;

        int indiceVacio = BuscarIndiceVacio();
        if (!SonAdyacentes(indiceCelda, indiceVacio))
            return;

        Intercambiar(indiceCelda, indiceVacio);
        RefrescarUI();

        if (EstaResuelto())
            CompletarAcertijo();
    }

    private void InicializarCuadricula()
    {
        for (int i = 0; i < TotalCeldas; i++)
            cuadricula[i] = EstadoInicial[i];
    }

    private void Intercambiar(int a, int b)
    {
        int temp = cuadricula[a];
        cuadricula[a] = cuadricula[b];
        cuadricula[b] = temp;
    }

    private int BuscarIndiceVacio()
    {
        for (int i = 0; i < TotalCeldas; i++)
        {
            if (cuadricula[i] == CeldaVacia)
                return i;
        }

        return TotalCeldas - 1;
    }

    private static bool SonAdyacentes(int a, int b)
    {
        int filaA = a / Columnas;
        int colA = a % Columnas;
        int filaB = b / Columnas;
        int colB = b % Columnas;

        int dist = Mathf.Abs(filaA - filaB) + Mathf.Abs(colA - colB);
        return dist == 1;
    }

    private bool EstaResuelto()
    {
        for (int i = 0; i < TotalCeldas; i++)
        {
            if (cuadricula[i] != EstadoGanador[i])
                return false;
        }

        return true;
    }

    private void CompletarAcertijo()
    {
        acertijoResuelto = true;
        AplicarMarcoCompletado();
        DesactivarInteraccionPiezas();
        Debug.Log("Acertijo 2 resuelto");
        PuzzleCompleted?.Invoke();
    }

    private void AplicarMarcoCompletado()
    {
        if (marcoAcertijo == null)
            return;

        if (spriteMarcoCompletado != null)
            marcoAcertijo.sprite = spriteMarcoCompletado;

        marcoAcertijo.color = colorMarcoCompletado;
    }

    private void RefrescarUI()
    {
        for (int i = 0; i < TotalCeldas; i++)
            ActualizarCelda(i);
    }

    private void ActualizarCelda(int indice)
    {
        if (indice < 0 || indice >= TotalCeldas || botonesCeldas[indice] == null)
            return;

        Button boton = botonesCeldas[indice];
        int valor = cuadricula[indice];
        bool esVacia = valor == CeldaVacia;

        Image imagen = boton.GetComponent<Image>();
        if (imagen != null)
        {
            Sprite spritePieza = ObtenerSpritePieza(valor);
            imagen.sprite = spritePieza;
            imagen.preserveAspect = true;
            imagen.color = esVacia ? colorCeldaVacia : Color.white;
        }

        OcultarTextoNumerico(boton);

        boton.interactable = !esVacia && !acertijoResuelto;
    }

    private void ConfigurarBotones()
    {
        if (botonesCeldas == null || botonesCeldas.Length < TotalCeldas)
            return;

        for (int i = 0; i < TotalCeldas; i++)
        {
            Button boton = botonesCeldas[i];
            if (boton == null)
                continue;

            int indice = i;
            SlidingPuzzleTile tile = boton.GetComponent<SlidingPuzzleTile>();
            if (tile == null)
                tile = boton.gameObject.AddComponent<SlidingPuzzleTile>();

            tile.Configurar(this, indice);

            Image imagen = boton.GetComponent<Image>();
            if (imagen != null)
            {
                boton.targetGraphic = imagen;
                imagen.raycastTarget = true;
            }

            OcultarTextoNumerico(boton);
        }
    }

    private static void OcultarTextoNumerico(Button boton)
    {
        Transform texto = boton.transform.Find("Texto");
        if (texto != null)
            texto.gameObject.SetActive(false);
    }

    private Sprite ObtenerSpritePieza(int valorPieza)
    {
        if (valorPieza == CeldaVacia || spritesPiezas == null)
            return null;

        int indiceSprite = valorPieza - 1;
        if (indiceSprite < 0 || indiceSprite >= spritesPiezas.Length)
            return null;

        return spritesPiezas[indiceSprite];
    }

    private void CargarSpritesPiezas()
    {
        if (spritesPiezas == null || spritesPiezas.Length != CantidadSpritesPiezas)
            spritesPiezas = new Sprite[CantidadSpritesPiezas];

        if (SpritesPiezasListos())
            return;

#if UNITY_EDITOR
        UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(RutaSpritesRetrato);
        if (assets == null || assets.Length == 0)
            return;

        for (int i = 0; i < CantidadSpritesPiezas; i++)
            spritesPiezas[i] = BuscarSpritePorNombre(assets, $"retrato_acertijo_{i}");

        EditorUtility.SetDirty(this);
#endif

        if (!SpritesPiezasListos())
        {
            Sprite[] loaded = Resources.LoadAll<Sprite>("Sprites/Puzzle/retrato_acertijo");
            if (loaded != null && loaded.Length > 0)
            {
                Array.Sort(loaded, (a, b) => string.CompareOrdinal(a.name, b.name));
                for (int i = 0; i < CantidadSpritesPiezas && i < loaded.Length; i++)
                {
                    spritesPiezas[i] = loaded[i];
                }
            }
        }
    }

    private bool SpritesPiezasListos()
    {
        if (spritesPiezas == null || spritesPiezas.Length != CantidadSpritesPiezas)
            return false;

        for (int i = 0; i < CantidadSpritesPiezas; i++)
        {
            if (spritesPiezas[i] == null)
                return false;
        }

        return true;
    }

    private static Sprite BuscarSpritePorNombre(UnityEngine.Object[] assets, string nombre)
    {
        foreach (UnityEngine.Object obj in assets)
        {
            if (obj is Sprite sprite && sprite.name == nombre)
                return sprite;
        }

        return null;
    }

    private void DesactivarInteraccionPiezas()
    {
        foreach (Button boton in botonesCeldas)
        {
            if (boton != null)
                boton.interactable = false;
        }
    }

    private void ResolverReferencias()
    {
        if (marcoAcertijo == null)
        {
            GameObject marco = GameObject.Find("MarcoAcertijo");
            if (marco != null)
                marcoAcertijo = marco.GetComponent<Image>();
        }

        if (marcoAcertijo != null)
        {
            colorMarcoNormalGuardado = marcoAcertijo.color;
            if (spriteMarcoNormal != null)
                marcoAcertijo.sprite = spriteMarcoNormal;
        }

        if (contenedorCuadricula == null)
        {
            GameObject contenedor = GameObject.Find("CuadriculaPuzzle");
            if (contenedor != null)
                contenedorCuadricula = contenedor.transform;
        }

        if (contenedorCuadricula != null && !TieneBotonesAsignados())
            botonesCeldas = contenedorCuadricula.GetComponentsInChildren<Button>(true);
    }

    private bool TieneBotonesAsignados()
    {
        if (botonesCeldas == null || botonesCeldas.Length < TotalCeldas)
            return false;

        for (int i = 0; i < TotalCeldas; i++)
        {
            if (botonesCeldas[i] == null)
                return false;
        }

        return true;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        CargarSpritesPiezas();

        if (botonesCeldas != null && botonesCeldas.Length != TotalCeldas)
            Debug.LogWarning($"{nameof(SlidingPuzzleController)}: asigna {TotalCeldas} botones.", this);
    }
#endif
}
