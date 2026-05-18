using System;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Acertijo: 5 botones en secuencia → estados visuales del sprite sheet (cerrado → abierto).
/// Sprites: Assets/Sprites/Puzzle/acertijo 1.png (acertijo 1_0 … acertijo 1_5).
/// </summary>
public class AcertijoController : MonoBehaviour
{
    private const string RutaSpritesAcertijo = "Assets/Sprites/Puzzle/acertijo 1.png";
    private const int CantidadSprites = 7;
    private const int TotalPasos = 5;
    private const int SpriteResuelto = 6;

    private static readonly string[] NombresSprites =
    {
        "acertijo 1_0", "acertijo 1_1", "acertijo 1_2", "acertijo 1_3",
        "acertijo 1_4", "acertijo 1_5", "acertijo 1_5"
    };

    public event Action PuzzleCompleted;

    [Header("Frame del acertijo")]
    public Image frameAcertijo;

    [Tooltip("Sprites 0-6: cerrado → completamente abierto.")]
    public Sprite[] spritesAcertijo = new Sprite[CantidadSprites];

    [Header("Botones (izq → der: Boton1 … Boton5)")]
    public Button[] botones = new Button[TotalPasos];

    [Header("Secuencia correcta (números 1-5)")]
    public int[] ordenCorrecto = { 3, 1, 4, 2, 5 };

    private int pasoActual;
    private bool acertijoCompletado;

    private void Awake()
    {
        ResolverReferencias();
        CargarSpritesAcertijo();
        PrepararBotones();
        ConfigurarEnlacesBotones();
        OcultarElementosValidacionUI();
    }

    private void Start()
    {
        ReiniciarProgreso();
    }

    /// <summary>
    /// Llamado por AcertijoBotonEnlace (On Click del Inspector).
    /// </summary>
    public void ProcesarPulsacion(int numeroBoton)
    {
        if (acertijoCompletado)
            return;

        if (!EsOrdenValido())
        {
            Debug.LogError($"{nameof(AcertijoController)}: ordenCorrecto inválido.", this);
            return;
        }

        int esperado = ordenCorrecto[pasoActual];

        if (numeroBoton != esperado)
        {
            ReiniciarProgreso();
            return;
        }

        pasoActual++;

        if (pasoActual >= TotalPasos)
            CompletarAcertijo();
        else
            ActualizarSprite(pasoActual);
    }

    private void ActualizarSprite(int indiceEstado)
    {
        if (frameAcertijo == null)
            return;

        if (!SpritesListos())
        {
            CargarSpritesAcertijo();
            if (!SpritesListos())
                return;
        }

        indiceEstado = Mathf.Clamp(indiceEstado, 0, SpriteResuelto);
        Sprite sprite = spritesAcertijo[indiceEstado];

        if (sprite == null)
            return;

        frameAcertijo.sprite = sprite;
        frameAcertijo.preserveAspect = true;
    }

    private void ReiniciarProgreso()
    {
        pasoActual = 0;
        acertijoCompletado = false;
        ActualizarSprite(0);
        EstablecerBotonesInteractuables(true);
    }

    private void CompletarAcertijo()
    {
        acertijoCompletado = true;
        ActualizarSprite(SpriteResuelto);
        EstablecerBotonesInteractuables(false);
        Debug.Log("¡Acertijo resuelto!");
        PuzzleCompleted?.Invoke();
    }

    private void ResolverReferencias()
    {
        if (frameAcertijo == null)
        {
            GameObject frameGo = GameObject.Find("FrameAcertijo");
            if (frameGo != null)
                frameAcertijo = frameGo.GetComponent<Image>();
        }

        if (frameAcertijo != null)
            frameAcertijo.raycastTarget = false;

        if (!TieneBotonesAsignados())
            botones = BuscarBotonesPorNombre();
    }

    private void OcultarElementosValidacionUI()
    {
        DesactivarSiExiste("Titulo");
        DesactivarSiExiste("TextoValidacion");
        DesactivarSiExiste("MensajeConfirmacion");

        if (!TieneBotonesAsignados())
            return;

        for (int i = 0; i < TotalPasos; i++)
        {
            if (botones[i] == null)
                continue;

            foreach (Transform hijo in botones[i].transform)
                hijo.gameObject.SetActive(false);
        }
    }

    private static void DesactivarSiExiste(string nombre)
    {
        GameObject go = GameObject.Find(nombre);
        if (go != null)
            go.SetActive(false);
    }

    private void CargarSpritesAcertijo()
    {
        if (spritesAcertijo == null || spritesAcertijo.Length != CantidadSprites)
            spritesAcertijo = new Sprite[CantidadSprites];

        if (SpritesListos())
            return;

#if UNITY_EDITOR
        UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(RutaSpritesAcertijo);
        if (assets == null || assets.Length == 0)
            return;

        for (int i = 0; i < CantidadSprites; i++)
        {
            spritesAcertijo[i] = BuscarSpritePorNombre(assets, NombresSprites[i]);
            if (spritesAcertijo[i] == null && i == 1)
                spritesAcertijo[i] = BuscarSpritePorNombre(assets, "acertijo 1_1");
        }

        if (!SpritesListos())
        {
            Sprite[] loaded = Resources.LoadAll<Sprite>("Sprites/Puzzle/acertijo 1");
            if (loaded != null && loaded.Length > 0)
            {
                Array.Sort(loaded, (a, b) => string.CompareOrdinal(a.name, b.name));
                for (int i = 0; i < CantidadSprites && i < loaded.Length; i++)
                {
                    spritesAcertijo[i] = loaded[i];
                }

                if (spritesAcertijo[SpriteResuelto] == null && spritesAcertijo[5] != null)
                {
                    spritesAcertijo[SpriteResuelto] = spritesAcertijo[5];
                }
            }
        }

        EditorUtility.SetDirty(this);
#endif
    }

    private bool SpritesListos()
    {
        if (spritesAcertijo == null || spritesAcertijo.Length != CantidadSprites)
            return false;

        for (int i = 0; i < CantidadSprites; i++)
        {
            if (spritesAcertijo[i] == null)
                return false;

            if (!spritesAcertijo[i].name.StartsWith("acertijo"))
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

    private bool TieneBotonesAsignados()
    {
        if (botones == null || botones.Length < TotalPasos)
            return false;

        for (int i = 0; i < TotalPasos; i++)
        {
            if (botones[i] == null)
                return false;
        }

        return true;
    }

    private Button[] BuscarBotonesPorNombre()
    {
        Button[] encontrados = new Button[TotalPasos];

        for (int i = 0; i < TotalPasos; i++)
        {
            GameObject go = GameObject.Find($"Boton{i + 1}");
            if (go != null)
                encontrados[i] = go.GetComponent<Button>();
        }

        return encontrados;
    }

    private void PrepararBotones()
    {
        if (!TieneBotonesAsignados())
            return;

        for (int i = 0; i < TotalPasos; i++)
        {
            Button boton = botones[i];
            if (boton == null)
                continue;

            boton.interactable = true;

            Image imagen = boton.GetComponent<Image>();
            if (imagen != null)
            {
                boton.targetGraphic = imagen;
                imagen.raycastTarget = true;
            }

        }
    }

    private void ConfigurarEnlacesBotones()
    {
        if (!TieneBotonesAsignados())
            return;

        for (int i = 0; i < TotalPasos; i++)
        {
            if (botones[i] == null)
                continue;

            AcertijoBotonEnlace enlace = botones[i].GetComponent<AcertijoBotonEnlace>();
            if (enlace == null)
                enlace = botones[i].gameObject.AddComponent<AcertijoBotonEnlace>();

            enlace.Configurar(this, i + 1);
        }
    }

    private void EstablecerBotonesInteractuables(bool interactuable)
    {
        if (!TieneBotonesAsignados())
            return;

        for (int i = 0; i < TotalPasos; i++)
        {
            if (botones[i] != null)
                botones[i].interactable = interactuable;
        }
    }

    private bool EsOrdenValido()
    {
        return ordenCorrecto != null && ordenCorrecto.Length >= TotalPasos;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        CargarSpritesAcertijo();

        if (frameAcertijo != null && SpritesListos())
            frameAcertijo.sprite = spritesAcertijo[0];
    }
#endif
}
