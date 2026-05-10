using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Builder de contenido narrativo para el Capítulo 2: El Estudio.
/// Inyecta todas las conversaciones de hotspots, NPCs y decisiones del Cap 2
/// en la DialogueLibrary usando AddConversations (no destructivo).
///
/// Contenido basado en la_mansion_de_simon.py → capitulo_2() / explorar_estudio().
///
/// Hotspots del Estudio:
///   - Agenda de Simón
///   - Libro de contabilidad (evidencia contra Ben)
///   - Nota del tablón
///   - Tablero de corcho (fotos + sexta persona)
///   - Archivador (bloqueado, requiere llave pequeña del Cap 3)
///
/// Decisión crítica al final del Cap 2:
///   - Confrontar a Ben con el libro de contabilidad
///   - Preguntar a Lisa sobre la foto del tablero
///   - Subir al piso superior (avanzar a Cap 3)
/// </summary>
public class Chapter2Builder : MonoBehaviour
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
            Debug.LogWarning("Chapter2Builder no encontro DialogueLibrary.");
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
            Debug.LogWarning("Chapter2Builder no encontro DialogueLibrary.");
            return;
        }

        // Verificar si ya existen las conversaciones del Cap 2
        if (targetLibrary.HasConversation("chapter2_intro") && targetLibrary.HasConversation("chapter2_decision"))
        {
            Debug.Log("Chapter2Builder: Conversaciones del Cap 2 ya presentes. Saltando generación.");
            return;
        }

        List<DialogueConversation> chapter2 = new List<DialogueConversation>
        {
            BuildChapter2Intro(),
            // ─── Hotspots del Estudio ───
            BuildEstudioAgenda(),
            BuildEstudioLibroContabilidad(),
            BuildEstudioNotaTablon(),
            BuildEstudioTableroCorcho(),
            BuildEstudioArchivador(),
            // ─── NPC: diálogos de ansiedad Cap 2 ───
            BuildNpcAnxiety("chapter2_npc_robert", "Robert",
                "Este estudio... huele a tinta y a encierro. Simón pasaba horas aquí, lejos de todos.",
                "Hay demasiados papeles. Demasiados secretos escritos con letra cuidadosa."),
            BuildNpcAnxiety("chapter2_npc_ana", "Ana",
                "Mira las paredes. Simón organizaba todo como si supiera que alguien vendría a buscar.",
                "Me siento observada incluso aquí. Como si el polvo tuviera memoria."),
            BuildNpcAnxiety("chapter2_npc_ben", "Ben",
                "No deberíamos estar tocando nada. Esto es propiedad privada. ¿No lo ven?",
                "Cada cajón que abren me pone más nervioso. No me gusta esto."),
            BuildNpcAnxiety("chapter2_npc_lisa", "Lisa",
                "Todo está demasiado limpio para un muerto. Alguien ha estado aquí recientemente.",
                "Necesito ver ese tablero de cerca. Hay algo que no cuadra en las fotos."),
            BuildNpcAnxiety("chapter2_npc_lucas", "Lucas",
                "Simón me dejaba entrar aquí a veces. Siempre cerraba un cajón cuando yo llegaba.",
                "Ese archivador... nunca me dejó abrirlo. Decía que era 'para después'."),
            // ─── NPC: diálogos críticos (100% ansiedad) Cap 2 ───
            BuildNpcCritical("chapter2_npc_robert_critical", "Robert",
                "Los papeles me hablan. Cada firma es una condena. No puedo seguir fingiendo."),
            BuildNpcCritical("chapter2_npc_ana_critical", "Ana",
                "Los cuadros en el pasillo se movieron. Estoy segura. Las pinturas respiran."),
            BuildNpcCritical("chapter2_npc_ben_critical", "Ben",
                "El libro... el libro tiene mi nombre. Todos lo van a ver. Es el fin."),
            BuildNpcCritical("chapter2_npc_lisa_critical", "Lisa",
                "La sexta persona está aquí. La vi moverse detrás del tablero. Nadie me cree."),
            BuildNpcCritical("chapter2_npc_lucas_critical", "Lucas",
                "Simón me dijo una vez que esta casa castiga. Ahora lo entiendo."),
            // ─── NPC: preguntar sobre items del Cap 2 ───
            BuildNpcItemQuestion("chapter2_npc_robert_item_agenda", "Robert",
                "Encontré una agenda con una reunión cancelada tres días antes de la muerte.",
                "Si Simón canceló una reunión por 'peligro', sabía que algo iba a pasar. Y no hizo nada para evitarlo."),
            BuildNpcItemQuestion("chapter2_npc_ben_item_libro_contabilidad", "Ben",
                "Hay un libro de contabilidad con entradas marcadas con una 'B' roja.",
                "Eso no significa nada. Podría ser cualquier inicial. Hay muchos nombres con B."),
            BuildNpcItemQuestion("chapter2_npc_lisa_item_tablero_corcho", "Lisa",
                "En el tablero de corcho hay fotos de todos nosotros. Y una persona con el rostro tapado.",
                "Lo sabía. Simón nos estaba investigando. Y esa sexta persona... es la clave de todo."),
            BuildNpcItemQuestion("chapter2_npc_ana_item_nota_tablon", "Ana",
                "Una nota dice: 'Segunda copia hecha. Ellos no saben que tengo el duplicado.'",
                "Si Simón hizo copias de algo, era porque no confiaba en que lo original sobreviviera. Eso es lo que hacen los artistas antes de una despedida."),
            BuildNpcItemQuestion("chapter2_npc_lucas_item_archivador", "Lucas",
                "El archivador está cerrado. ¿Sabes algo de la llave?",
                "Simón guardaba algo importante ahí. Una vez lo vi meter un sobre con lacre. Nunca me dijo qué contenía."),
            // ─── Decisión del Capítulo 2 ───
            BuildChapter2Decision()
        };

        targetLibrary.AddConversations(chapter2);
        Debug.Log($"Chapter2Builder: Generadas {chapter2.Count} conversaciones del Capítulo 2.");
    }

    // ═══════════════════════════════════════════════════════════════
    // INTRO
    // ═══════════════════════════════════════════════════════════════

    private static DialogueConversation BuildChapter2Intro()
    {
        DialogueNode start = new DialogueNode
        {
            id = "start",
            endsConversation = true,
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = "Narrador", text = "Capítulo 2 — El Estudio. 4:30 PM." },
                new DialogueLine { speaker = "Narrador", text = "La puerta del estudio se abre con un chirrido lento. El aire dentro está quieto, cargado de olor a papel viejo y tinta seca." },
                new DialogueLine { speaker = "Narrador", text = "Simón pasaba horas aquí. Hay documentos, una agenda, un tablero de corcho con fotografías. Todo está demasiado ordenado para alguien que supuestamente murió de forma repentina.", anxietyDelta = 5f },
                new DialogueLine { speaker = "Lisa", text = "Esto no es el escritorio de un muerto. Es el de alguien que se preparó para irse.", anxietyDelta = 3f },
                new DialogueLine { speaker = "Robert", text = "No toquen nada hasta que sepamos qué estamos buscando." },
                new DialogueLine { speaker = "Narrador", text = "El grupo entra con cautela. Cada objeto en este estudio podría cambiar la historia.", setFlag = "chapter2.intro.seen" }
            }
        };

        return new DialogueConversation
        {
            id = "chapter2_intro",
            nodes = new List<DialogueNode> { start }
        };
    }

    // ═══════════════════════════════════════════════════════════════
    // HOTSPOTS DEL ESTUDIO
    // ═══════════════════════════════════════════════════════════════

    private static DialogueConversation BuildEstudioAgenda()
    {
        DialogueNode start = new DialogueNode
        {
            id = "start",
            endsConversation = true,
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = "Narrador", text = "Una agenda de cuero negro, abierta en la página de hace tres días. La letra de Simón es precisa, casi mecánica." },
                new DialogueLine { speaker = "Narrador", text = "En la última entrada legible dice: 'Reunión cancelada. Peligro. No volver a abrir la puerta norte.'", anxietyDelta = 10f },
                new DialogueLine { speaker = "Narrador", text = "Las páginas posteriores están en blanco. Como si el tiempo se hubiera detenido en esa fecha.", setFlag = "clue.estudio.agenda" }
            }
        };

        return new DialogueConversation
        {
            id = "chapter2_Studio_Agenda",
            nodes = new List<DialogueNode> { start }
        };
    }

    private static DialogueConversation BuildEstudioLibroContabilidad()
    {
        DialogueNode start = new DialogueNode
        {
            id = "start",
            endsConversation = true,
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = "Narrador", text = "Un libro de contabilidad grueso, encuadernado en tela verde oscura. Simón registraba cada transacción con precisión obsesiva." },
                new DialogueLine { speaker = "Narrador", text = "Varias entradas están marcadas con tinta roja. Junto a cada una, una inicial: 'B'. Los montos son significativos. Algunos tienen anotaciones al margen:", anxietyDelta = 8f },
                new DialogueLine { speaker = "Narrador", text = "'Discrepancia. Verificar con banco. No coincide con lo reportado por B.'", anxietyDelta = 6f },
                new DialogueLine { speaker = "Narrador", text = "Ben, al otro lado del estudio, nota que examinas el libro. Su expresión cambia imperceptiblemente.", setFlag = "clue.estudio.libro_contabilidad", addInventoryItemId = "libro_contabilidad" }
            }
        };

        return new DialogueConversation
        {
            id = "chapter2_Accountant_Book",
            nodes = new List<DialogueNode> { start }
        };
    }

    private static DialogueConversation BuildEstudioNotaTablon()
    {
        DialogueNode start = new DialogueNode
        {
            id = "start",
            endsConversation = true,
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = "Narrador", text = "Clavada en un tablón de corcho secundario, una nota escrita a mano con letra firme:" },
                new DialogueLine { speaker = "Nota", text = "Segunda copia hecha. Ellos no saben que tengo el duplicado. Si algo me pasa, buscar en el archivador.", anxietyDelta = 7f },
                new DialogueLine { speaker = "Narrador", text = "La nota no tiene fecha ni firma, pero la caligrafía es inconfundible: es de Simón.", setFlag = "clue.estudio.nota_tablon" }
            }
        };

        return new DialogueConversation
        {
            id = "chapter2_estudio_nota_tablon",
            nodes = new List<DialogueNode> { start }
        };
    }

    private static DialogueConversation BuildEstudioTableroCorcho()
    {
        DialogueNode start = new DialogueNode
        {
            id = "start",
            endsConversation = true,
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = "Narrador", text = "El tablero de corcho principal ocupa casi toda una pared. Hay fotografías conectadas con hilos rojos, como en una investigación policial." },
                new DialogueLine { speaker = "Narrador", text = "Reconoces cinco rostros: Robert, Ana, Ben, Lisa y Lucas. Cada foto tiene anotaciones debajo con fechas y lugares.", anxietyDelta = 10f },
                new DialogueLine { speaker = "Narrador", text = "Pero hay una sexta fotografía. El rostro está cubierto con cinta negra. Debajo, escrito con tinta roja: '¿Quién eres tú?'", anxietyDelta = 12f, setFlag = "clue.estudio.tablero_corcho" },
                new DialogueLine { speaker = "Narrador", text = "Simón estaba investigando a todos. Y a alguien más que ninguno de los presentes reconoce.", addInventoryItemId = "foto_tablero_corcho" }
            }
        };

        return new DialogueConversation
        {
            id = "chapter2_Cork_Board",
            nodes = new List<DialogueNode> { start }
        };
    }

    private static DialogueConversation BuildEstudioArchivador()
    {
        DialogueNode start = new DialogueNode
        {
            id = "start",
            endsConversation = true,
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = "Narrador", text = "Un archivador metálico de dos cajones. El superior está ligeramente abollado, como si alguien hubiera intentado forzarlo." },
                new DialogueLine { speaker = "Narrador", text = "Está cerrado con llave. Una cerradura pequeña, distinta a las puertas de la mansión. Necesitas una llave más pequeña.", anxietyDelta = 4f },
                new DialogueLine { speaker = "Narrador", text = "Sea lo que sea que Simón guardó aquí, se aseguró de que no fuera fácil de encontrar.", setFlag = "clue.estudio.archivador_visto" }
            }
        };

        return new DialogueConversation
        {
            id = "chapter2_estudio_archivador",
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
                new DialogueLine { speaker = npcName, text = "No puedo... no puedo dejar de temblar." },
                new DialogueLine { speaker = "Narrador", text = "Su mirada está perdida. La ansiedad lo ha consumido por completo." }
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
                new DialogueLine { speaker = "Narrador", text = "La revelación abre una nueva grieta en la historia. Alguien aquí sabe más de lo que dice." }
            }
        };

        return new DialogueConversation { id = id, nodes = new List<DialogueNode> { start } };
    }

    // ═══════════════════════════════════════════════════════════════
    // DECISIÓN DEL CAPÍTULO 2
    // ═══════════════════════════════════════════════════════════════

    private static DialogueConversation BuildChapter2Decision()
    {
        DialogueNode start = new DialogueNode
        {
            id = "start",
            endsConversation = false,
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = "Narrador", text = "Has visto lo suficiente en el estudio. La tensión crece. Debes decidir el siguiente paso." }
            },
            choices = new List<DialogueChoice>
            {
                new DialogueChoice
                {
                    id = "confront_ben",
                    text = "Confrontar a Ben con el libro de contabilidad",
                    nextNodeId = "result_confront_ben",
                    anxietyDelta = 15f,
                    setFlag = "chapter2.choice.confront_ben",
                    requiredFlag = "clue.estudio.libro_contabilidad"
                },
                new DialogueChoice
                {
                    id = "ask_lisa",
                    text = "Preguntar a Lisa sobre la sexta persona en el tablero",
                    nextNodeId = "result_ask_lisa",
                    anxietyDelta = 5f,
                    setFlag = "chapter2.choice.ask_lisa",
                    requiredFlag = "clue.estudio.tablero_corcho"
                },
                new DialogueChoice
                {
                    id = "go_upstairs",
                    text = "Subir al piso superior para buscar la habitación de Simón",
                    nextNodeId = "result_go_upstairs",
                    anxietyDelta = 8f,
                    setFlag = "chapter2.choice.go_upstairs"
                }
            }
        };

        DialogueNode resultConfrontBen = new DialogueNode
        {
            id = "result_confront_ben",
            endsConversation = true,
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = "Jugador", text = "Ben. Necesito que me expliques las entradas marcadas con 'B' en el libro de Simón." },
                new DialogueLine { speaker = "Ben", text = "¿Qué...? No sé de qué hablas. Esas iniciales podrían ser de cualquiera.", anxietyDelta = 10f },
                new DialogueLine { speaker = "Narrador", text = "Sus manos tiemblan. La negación es demasiado rápida. Demasiado practicada." },
                new DialogueLine { speaker = "Ben", text = "Simón y yo teníamos un acuerdo informal. Si hay discrepancias, es porque él no llevaba bien sus cuentas. No yo.", anxietyDelta = 8f },
                new DialogueLine { speaker = "Narrador", text = "Robert observa la escena sin intervenir. Lisa toma nota mental. La confianza del grupo se ha fracturado.", setFlag = "chapter2.ben_confronted" }
            }
        };

        DialogueNode resultAskLisa = new DialogueNode
        {
            id = "result_ask_lisa",
            endsConversation = true,
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = "Jugador", text = "Lisa, la sexta foto en el tablero... ¿la reconoces?" },
                new DialogueLine { speaker = "Lisa", text = "No. Pero he visto esa técnica antes. Simón tapaba rostros en sus bocetos cuando quería proteger identidades.", anxietyDelta = 4f },
                new DialogueLine { speaker = "Lisa", text = "Lo que me preocupa es el hilo rojo que conecta a esa persona con TODOS nosotros. No solo con Simón.", anxietyDelta = 7f },
                new DialogueLine { speaker = "Narrador", text = "La periodista tiene razón. El tablero no muestra una investigación sobre Simón. Muestra una investigación sobre todos los presentes. Y sobre alguien más.", setFlag = "chapter2.lisa_asked" }
            }
        };

        DialogueNode resultGoUpstairs = new DialogueNode
        {
            id = "result_go_upstairs",
            endsConversation = true,
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = "Narrador", text = "Decides avanzar. El segundo piso espera." },
                new DialogueLine { speaker = "Robert", text = "Si subimos, no sabemos qué encontraremos. Pero quedarnos aquí tampoco nos da respuestas." },
                new DialogueLine { speaker = "Narrador", text = "Las escaleras crujen bajo tus pasos. Cada peldaño te acerca a la verdad de Simón.", anxietyDelta = 5f, setFlag = "chapter2.completed" }
            }
        };

        return new DialogueConversation
        {
            id = "chapter2_decision",
            nodes = new List<DialogueNode> { start, resultConfrontBen, resultAskLisa, resultGoUpstairs }
        };
    }
}
