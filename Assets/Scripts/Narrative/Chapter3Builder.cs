using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Builder de contenido narrativo para el Capítulo 3: Habitación de Simón.
/// Inyecta conversaciones en la DialogueLibrary usando AddConversations (no destructivo).
///
/// Contenido basado en la_mansion_de_simon.py → capitulo_3() / explorar_habitacion().
///
/// Hotspots de la Habitación:
///   - Vaso de agua (condensación → Simón estuvo aquí hace poco)
///   - Mapa en la pared (marca el Ala Norte)
///   - Carta inconclusa ("Si alguien lee esto, no estoy muerto...")
///   - Mesita de noche (contiene la llave pequeña del archivador)
///   - Cama deshecha (evidencia de uso reciente)
///
/// Decisión crítica:
///   - Volver al estudio a abrir el archivador
///   - Ir al Ala Norte siguiendo el mapa
///   - Confrontar al grupo con la carta
/// </summary>
public class Chapter3Builder : MonoBehaviour
{
    [SerializeField] private DialogueLibrary targetLibrary;
    [SerializeField] private bool generateOnStartIfEmpty = true;

    private void Awake()
    {
        if (!generateOnStartIfEmpty)
        {
            return;
        }

        if (targetLibrary == null)
        {
            targetLibrary = GetComponent<DialogueLibrary>();
        }

        if (targetLibrary == null)
        {
            Debug.LogWarning("Chapter3Builder no encontro DialogueLibrary.");
            return;
        }

        EnsureData();
    }

    public void EnsureData()
    {
        if (targetLibrary == null)
        {
            targetLibrary = GetComponent<DialogueLibrary>();
        }

        if (targetLibrary == null)
        {
            Debug.LogWarning("Chapter3Builder no encontro DialogueLibrary.");
            return;
        }

        if (targetLibrary.HasConversation("chapter3_intro") && targetLibrary.HasConversation("chapter3_decision"))
        {
            Debug.Log("Chapter3Builder: Conversaciones del Cap 3 ya presentes. Saltando generación.");
            return;
        }

        List<DialogueConversation> chapter3 = new List<DialogueConversation>
        {
            BuildChapter3Intro(),
            // ─── Hotspots de la Habitación ───
            BuildHabitacionVasoAgua(),
            BuildHabitacionMapaPared(),
            BuildHabitacionCartaInconclusa(),
            BuildHabitacionMesitaNoche(),
            BuildHabitacionCama(),
            // ─── NPC: diálogos de ansiedad Cap 3 ───
            BuildNpcAnxiety("chapter3_npc_robert", "Robert",
                "Esta habitación... tiene un olor particular. A vida reciente. No a muerte.",
                "Si Simón estuvo aquí hace poco, entonces todo lo que nos dijeron fue mentira."),
            BuildNpcAnxiety("chapter3_npc_ana", "Ana",
                "Mira la cama. Mira el vaso. Esto no es un cuarto abandonado.",
                "Me estoy empezando a preguntar si realmente vine por un funeral o por una trampa."),
            BuildNpcAnxiety("chapter3_npc_ben", "Ben",
                "Tenemos que irnos. Ya. Esto está mal. Todo está demasiado fresco.",
                "Si Simón está vivo y nos atrajo aquí... ¿qué quiere de nosotros?"),
            BuildNpcAnxiety("chapter3_npc_lisa", "Lisa",
                "Periodísticamente hablando, esto es una escena manipulada. No hay duda.",
                "La carta, el mapa, el vaso... todo está puesto para que lo encontremos."),
            BuildNpcAnxiety("chapter3_npc_lucas", "Lucas",
                "Simón siempre dormía con la ventana abierta. Mira, está cerrada con llave por dentro.",
                "Él no se fue por voluntad propia. Alguien lo encerró. O él mismo se encerró."),
            // ─── NPC: diálogos críticos Cap 3 ───
            BuildNpcCritical("chapter3_npc_robert_critical", "Robert",
                "Es mi hermano. Simón es mi hermano y nunca se lo dije a nadie. Y ahora puede que esté muerto de verdad."),
            BuildNpcCritical("chapter3_npc_ana_critical", "Ana",
                "Las joyas. Las joyas que usé como garantía. Si Simón está vivo, vendrá a cobrar. No puedo pagar."),
            BuildNpcCritical("chapter3_npc_ben_critical", "Ben",
                "El libro de cuentas. Si Simón hizo una copia y está en ese archivador, estoy acabado."),
            BuildNpcCritical("chapter3_npc_lisa_critical", "Lisa",
                "Hay alguien más en la casa. Lo siento. Lo huelo. La sexta persona está aquí."),
            BuildNpcCritical("chapter3_npc_lucas_critical", "Lucas",
                "El relicario. Robé el relicario y Simón lo sabe. Siempre lo supo. Esta es mi condena."),
            // ─── NPC: preguntar sobre items del Cap 3 ───
            BuildNpcItemQuestion("chapter3_npc_robert_item_carta", "Robert",
                "La carta dice que Simón podría estar vivo.",
                "Si está vivo... entonces esto no es un funeral. Es una trampa. Y todos caímos en ella."),
            BuildNpcItemQuestion("chapter3_npc_ana_item_mapa", "Ana",
                "El mapa señala el Ala Norte. ¿Sabías que existía?",
                "No. Nunca vi esa parte de la mansión. Simón nunca la mencionó en nuestros tratos."),
            BuildNpcItemQuestion("chapter3_npc_ben_item_llave_pequena", "Ben",
                "Encontré una llave pequeña en la mesita de noche.",
                "¿Una llave pequeña? Eso... eso podría abrir el archivador del estudio. No, no vayas. No abras eso."),
            BuildNpcItemQuestion("chapter3_npc_lisa_item_vaso", "Lisa",
                "El vaso de agua todavía tiene condensación. Simón estuvo aquí hace muy poco.",
                "Horas. Tal vez menos. Esto confirma mi teoría. La muerte fue un montaje."),
            BuildNpcItemQuestion("chapter3_npc_lucas_item_cama", "Lucas",
                "La cama está deshecha y todavía tibia.",
                "Era su costumbre. Nunca hacía la cama. Decía que era un ritual inútil para gente que no vive de verdad."),
            // ─── Decisión del Capítulo 3 ───
            BuildChapter3Decision()
        };

        targetLibrary.AddConversations(chapter3);
        Debug.Log($"Chapter3Builder: Generadas {chapter3.Count} conversaciones del Capítulo 3.");
    }

    // ═══════════════════════════════════════════════════════════════
    // INTRO
    // ═══════════════════════════════════════════════════════════════

    private static DialogueConversation BuildChapter3Intro()
    {
        DialogueNode start = new DialogueNode
        {
            id = "start",
            endsConversation = true,
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = "Narrador", text = "Capítulo 3 — La Habitación de Simón. 6:00 PM." },
                new DialogueLine { speaker = "Narrador", text = "La puerta se abre sin resistencia. No estaba cerrada con llave. Como si alguien quisiera que entraras." },
                new DialogueLine { speaker = "Narrador", text = "El cuarto es espartano. Una cama individual, una mesita de noche, un escritorio contra la pared. Un mapa clavado con chinchetas. Un vaso de agua.", anxietyDelta = 6f },
                new DialogueLine { speaker = "Narrador", text = "Y un olor. Sutil pero inconfundible. Alguien estuvo aquí hace muy poco.", anxietyDelta = 8f },
                new DialogueLine { speaker = "Ana", text = "Esto no huele a habitación cerrada. Huele a alguien que acaba de irse.", anxietyDelta = 5f },
                new DialogueLine { speaker = "Lucas", text = "Es su olor. Es Simón. Estoy seguro.", anxietyDelta = 10f, setFlag = "chapter3.intro.seen" }
            }
        };

        return new DialogueConversation
        {
            id = "chapter3_intro",
            nodes = new List<DialogueNode> { start }
        };
    }

    // ═══════════════════════════════════════════════════════════════
    // HOTSPOTS DE LA HABITACIÓN
    // ═══════════════════════════════════════════════════════════════

    private static DialogueConversation BuildHabitacionVasoAgua()
    {
        DialogueNode start = new DialogueNode
        {
            id = "start",
            endsConversation = true,
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = "Narrador", text = "Un vaso de vidrio sobre la mesita. Hay agua dentro. La superficie del vidrio está empañada por la condensación." },
                new DialogueLine { speaker = "Narrador", text = "Tocas el vidrio. Está frío. Pero la condensación es reciente. Alguien sirvió este vaso hace no más de dos horas.", anxietyDelta = 10f },
                new DialogueLine { speaker = "Narrador", text = "Los muertos no beben agua.", setFlag = "clue.habitacion.vaso_agua" }
            }
        };

        return new DialogueConversation
        {
            id = "chapter3_Water_Glass",
            nodes = new List<DialogueNode> { start }
        };
    }

    private static DialogueConversation BuildHabitacionMapaPared()
    {
        DialogueNode start = new DialogueNode
        {
            id = "start",
            endsConversation = true,
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = "Narrador", text = "Un mapa antiguo de la mansión y sus alrededores, clavado en la pared con cuatro chinchetas. Está amarillento por el tiempo." },
                new DialogueLine { speaker = "Narrador", text = "Una zona está marcada con un círculo rojo grueso: el 'Ala Norte'. Es una sección de la mansión que no aparece en ningún plano moderno.", anxietyDelta = 8f },
                new DialogueLine { speaker = "Narrador", text = "Debajo del círculo, con letra pequeña: 'Aquí termina todo. O empieza.'", anxietyDelta = 6f, setFlag = "clue.habitacion.mapa_ala_norte", addInventoryItemId = "mapa_ala_norte" }
            }
        };

        return new DialogueConversation
        {
            id = "chapter3_Wall_Map",
            nodes = new List<DialogueNode> { start }
        };
    }

    private static DialogueConversation BuildHabitacionCartaInconclusa()
    {
        DialogueNode start = new DialogueNode
        {
            id = "start",
            endsConversation = true,
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = "Narrador", text = "Sobre el escritorio hay una carta a medio escribir. La tinta todavía huele fresca. La pluma está junto a ella, como si el autor hubiera sido interrumpido." },
                new DialogueLine { speaker = "Narrador", text = "La carta dice:", anxietyDelta = 5f },
                new DialogueLine { speaker = "Carta", text = "Si alguien lee esto, no estoy muerto. Estoy en el Ala Norte. No vine por voluntad propia. Hay alguien más en esta casa que no debería estar. No confíen en—", anxietyDelta = 15f },
                new DialogueLine { speaker = "Narrador", text = "La frase se corta abruptamente. Una mancha de tinta marca el punto donde la pluma resbaló.", setFlag = "clue.habitacion.carta_inconclusa" },
                new DialogueLine { speaker = "Narrador", text = "Simón está vivo. Y está atrapado.", setFlag = "simon_vivo", addInventoryItemId = "carta_inconclusa" }
            }
        };

        return new DialogueConversation
        {
            id = "chapter3_Unfinished_Letter",
            nodes = new List<DialogueNode> { start }
        };
    }

    private static DialogueConversation BuildHabitacionMesitaNoche()
    {
        DialogueNode start = new DialogueNode
        {
            id = "start",
            endsConversation = true,
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = "Narrador", text = "La mesita de noche tiene un solo cajón. Lo abres con cuidado. Dentro, envuelta en un pañuelo de seda:" },
                new DialogueLine { speaker = "Narrador", text = "Una llave pequeña y oxidada. No es para una puerta. Es para algo más íntimo. Un cajón. Un archivador.", anxietyDelta = 3f, setFlag = "clue.habitacion.llave_pequena" },
                new DialogueLine { speaker = "Narrador", text = "Recuerdas el archivador cerrado del estudio. Esta llave podría abrirlo." }
            }
        };

        return new DialogueConversation
        {
            id = "chapter3_Bedside_Table",
            nodes = new List<DialogueNode> { start }
        };
    }

    private static DialogueConversation BuildHabitacionCama()
    {
        DialogueNode start = new DialogueNode
        {
            id = "start",
            endsConversation = true,
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = "Narrador", text = "La cama está deshecha. Las sábanas todavía conservan la forma de un cuerpo. La almohada tiene una marca de cabeza." },
                new DialogueLine { speaker = "Narrador", text = "Pasas la mano sobre la tela. Está tibia. No caliente, pero tampoco fría. Como si alguien se hubiera levantado hace no mucho.", anxietyDelta = 8f },
                new DialogueLine { speaker = "Narrador", text = "Debajo de la almohada, un mechón de cabello oscuro. De Simón, probablemente.", setFlag = "clue.habitacion.cama" }
            }
        };

        return new DialogueConversation
        {
            id = "chapter3_Bed",
            nodes = new List<DialogueNode> { start }
        };
    }

    // ═══════════════════════════════════════════════════════════════
    // DIÁLOGOS NPC (REUTILIZABLES)
    // ═══════════════════════════════════════════════════════════════

    private static DialogueConversation BuildNpcAnxiety(string id, string npcName, string lineA, string lineB)
    {
        DialogueNode start = new DialogueNode
        {
            id = "start",
            endsConversation = true,
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = npcName, text = lineA, anxietyDelta = -8f },
                new DialogueLine { speaker = npcName, text = lineB, anxietyDelta = -7f }
            }
        };

        return new DialogueConversation { id = id, nodes = new List<DialogueNode> { start } };
    }

    private static DialogueConversation BuildNpcCritical(string id, string npcName, string deliriousLine)
    {
        DialogueNode start = new DialogueNode
        {
            id = "start",
            endsConversation = true,
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = npcName, text = deliriousLine },
                new DialogueLine { speaker = npcName, text = "Ya no importa nada. Ya no importa." },
                new DialogueLine { speaker = "Narrador", text = "Ha llegado a un punto sin retorno. La casa lo ha roto." }
            }
        };

        return new DialogueConversation { id = id, nodes = new List<DialogueNode> { start } };
    }

    private static DialogueConversation BuildNpcItemQuestion(string id, string npcName, string promptLine, string answerLine)
    {
        DialogueNode start = new DialogueNode
        {
            id = "start",
            endsConversation = true,
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = "Jugador", text = promptLine },
                new DialogueLine { speaker = npcName, text = answerLine, anxietyDelta = -2f },
                new DialogueLine { speaker = "Narrador", text = "La verdad se acerca. Y con ella, el peligro." }
            }
        };

        return new DialogueConversation { id = id, nodes = new List<DialogueNode> { start } };
    }

    // ═══════════════════════════════════════════════════════════════
    // DECISIÓN DEL CAPÍTULO 3
    // ═══════════════════════════════════════════════════════════════

    private static DialogueConversation BuildChapter3Decision()
    {
        DialogueNode start = new DialogueNode
        {
            id = "start",
            endsConversation = false,
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = "Narrador", text = "La habitación de Simón ha revelado más de lo esperado. Simón podría estar vivo. Y hay alguien más en la casa. ¿Qué harás?" }
            },
            choices = new List<DialogueChoice>
            {
                new DialogueChoice
                {
                    id = "return_archivador",
                    text = "Volver al estudio con la llave pequeña para abrir el archivador",
                    nextNodeId = "result_archivador",
                    anxietyDelta = 4f,
                    setFlag = "chapter3.choice.archivador",
                    requiredFlag = "clue.habitacion.llave_pequena"
                },
                new DialogueChoice
                {
                    id = "go_north_wing",
                    text = "Buscar el Ala Norte marcada en el mapa",
                    nextNodeId = "result_north_wing",
                    anxietyDelta = 12f,
                    setFlag = "chapter3.choice.north_wing",
                    requiredFlag = "clue.habitacion.mapa_ala_norte"
                },
                new DialogueChoice
                {
                    id = "confront_group",
                    text = "Mostrar la carta de Simón a todos",
                    nextNodeId = "result_confront_group",
                    anxietyDelta = 10f,
                    setFlag = "chapter3.choice.confront_group",
                    requiredFlag = "clue.habitacion.carta_inconclusa"
                }
            }
        };

        DialogueNode resultArchivador = new DialogueNode
        {
            id = "result_archivador",
            endsConversation = true,
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = "Narrador", text = "Decides volver al estudio. El archivador guarda los secretos que Simón quería proteger." },
                new DialogueLine { speaker = "Narrador", text = "La llave gira en la cerradura con un clic satisfactorio. La respuesta está dentro.", setFlag = "chapter3.completed" }
            }
        };

        DialogueNode resultNorthWing = new DialogueNode
        {
            id = "result_north_wing",
            endsConversation = true,
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = "Narrador", text = "Decides seguir el mapa hacia el Ala Norte. Si Simón está ahí, lo encontrarás." },
                new DialogueLine { speaker = "Robert", text = "No vayamos solos. Si hay alguien más en la casa, necesitamos estar juntos." },
                new DialogueLine { speaker = "Narrador", text = "El pasillo hacia el Ala Norte es más oscuro de lo esperado. Las paredes se estrechan.", anxietyDelta = 8f, setFlag = "chapter3.completed" }
            }
        };

        DialogueNode resultConfrontGroup = new DialogueNode
        {
            id = "result_confront_group",
            endsConversation = true,
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = "Jugador", text = "Escuchen todos. Encontré una carta de Simón. Dice que no está muerto. Y que hay alguien más aquí." },
                new DialogueLine { speaker = "Ana", text = "¿Qué? No... eso no tiene sentido.", anxietyDelta = 12f },
                new DialogueLine { speaker = "Ben", text = "Nos mintieron. Todo esto es una trampa.", anxietyDelta = 15f },
                new DialogueLine { speaker = "Lisa", text = "Lo sabía. Lo sabía desde el principio.", anxietyDelta = 5f },
                new DialogueLine { speaker = "Narrador", text = "El grupo estalla. La confianza se rompe definitivamente. Pero ahora todos saben la verdad.", setFlag = "chapter3.completed" }
            }
        };

        return new DialogueConversation
        {
            id = "chapter3_decision",
            nodes = new List<DialogueNode> { start, resultArchivador, resultNorthWing, resultConfrontGroup }
        };
    }
}
