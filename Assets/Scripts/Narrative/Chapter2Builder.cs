using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Builder de contenido narrativo para el Capítulo 2: El Estudio.
/// Inyecta todas las conversaciones de hotspots, NPCs y decisiones del Cap 2
/// en la DialogueLibrary usando AddConversations (no destructivo).
///
/// Contenido basado en la_mansion_de_simon.py → capitulo_2() / explorar_estudio().
///
/// FLUJO NARRATIVO REFACTORIZADO:
/// 1. Jugador entra al estudio con los 5 NPCs
/// 2. Jugador habla con cada NPC (solo disponible en la sala de NPCs)
/// 3. Después de hablar con los 5 NPCs → PRIMERA DECISIÓN:
///    - Opción A: "Hablar sinceramente sobre Simon"
///    - Opción B: "Separarse"
/// 4. Jugador obtiene el libro de contabilidad
/// 5. Jugador regresa a la sala de NPCs
/// 6. Jugador interactúa con Ben sobre el libro → SEGUNDA DECISIÓN:
///    - Opción A: "Confrontar"
///    - Opción B: "Subir a buscar la habitación de Simon"
///
/// Hotspots del Estudio:
///   - Agenda de Simón
///   - Libro de contabilidad (evidencia contra Ben)
///   - Nota del tablón
///   - Tablero de corcho (fotos + sexta persona)
///   - Archivador (bloqueado, requiere llave pequeña del Cap 3)
///
/// VALIDACIONES CRÍTICAS:
///   - Diálogos de NPC SOLO cuando jugador está en la sala de NPCs (lobby)
///   - Decisiones SOLO cuando jugador está en la sala de NPCs
///   - El protagonista investiga SOLO en el estudio
///   - Los NPCs permanecen SIEMPRE en la sala de NPCs
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
                "Eso no significa nada. Podría ser cualquier inicial. Hay muchos nombres con B.",
                "chapter2.asked_ben_about_book"),
            BuildNpcItemQuestion("chapter2_npc_lisa_item_tablero_corcho", "Lisa",
                "En el tablero de corcho hay fotos de todos nosotros. Y una persona con el rostro tapado.",
                "Lo sabía. Simón nos estaba investigando. Y esa sexta persona... es la clave de todo."),
            BuildNpcItemQuestion("chapter2_npc_ana_item_nota_tablon", "Ana",
                "Una nota dice: 'Segunda copia hecha. Ellos no saben que tengo el duplicado.'",
                "Si Simón hizo copias de algo, era porque no confiaba en que lo original sobreviviera. Eso es lo que hacen los artistas antes de una despedida."),
            BuildNpcItemQuestion("chapter2_npc_lucas_item_archivador", "Lucas",
                "El archivador está cerrado. ¿Sabes algo de la llave?",
                "Simón guardaba algo importante ahí. Una vez lo vi meter un sobre con lacre. Nunca me dijo qué contenía."),
            // ─── Decisiones del Capítulo 2 ───
            BuildChapter2InitialDecision(),
            BuildChapter2BookDecision()
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
        return BuildNpcItemQuestion(id, npcName, promptLine, answerLine, null);
    }

    private static DialogueConversation BuildNpcItemQuestion(string id, string npcName, string promptLine, string answerLine, string additionalFlag)
    {
        var lines = new List<DialogueLine>
        {
            new DialogueLine { speaker = "Jugador", text = promptLine },
            new DialogueLine { speaker = npcName, text = answerLine, anxietyDelta = -2f },
            new DialogueLine { speaker = "Narrador", text = "La revelación abre una nueva grieta en la historia. Alguien aquí sabe más de lo que dice." }
        };

        // Si hay un flag adicional, agregarlo a la última línea
        if (!string.IsNullOrWhiteSpace(additionalFlag))
        {
            lines[lines.Count - 1].setFlag = additionalFlag;
        }

        DialogueNode start = new DialogueNode
        {
            id = "start",
            endsConversation = true,
            lines = lines
        };

        return new DialogueConversation { id = id, nodes = new List<DialogueNode> { start } };
    }

    // ═══════════════════════════════════════════════════════════════
    // DECISIONES DEL CAPÍTULO 2
    // ═══════════════════════════════════════════════════════════════

    private static DialogueConversation BuildChapter2InitialDecision()
    {
        DialogueNode start = new DialogueNode
        {
            id = "start",
            endsConversation = false,
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = "Narrador", text = "Los cinco se miran unos a otros en el silencio del estudio. La tensión es casi visible en el aire." },
                new DialogueLine { speaker = "Narrador", text = "¿Qué haces ahora?" }
            },
            choices = new List<DialogueChoice>
            {
                new DialogueChoice
                {
                    id = "talk_sincere",
                    text = "Hablar sinceramente sobre Simon",
                    nextNodeId = "result_talk_sincere",
                    anxietyDelta = 5f,
                    setFlag = "chapter2.choice.talk_sincere"
                },
                new DialogueChoice
                {
                    id = "separate",
                    text = "Separarse",
                    nextNodeId = "result_separate",
                    anxietyDelta = -3f,
                    setFlag = "chapter2.choice.separate"
                }
            }
        };

        DialogueNode resultTalkSincere = new DialogueNode
        {
            id = "result_talk_sincere",
            endsConversation = true,
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = "Jugador", text = "Necesitamos hablar sinceramente. ¿Qué saben realmente sobre Simon? ¿Alguien sabe más de lo que ha dicho?" },
                new DialogueLine { speaker = "Lisa", text = "Aquí hay algo que no cuadra. Las fotos en el tablero... la sexta persona. Simón estaba investigando a alguien.", anxietyDelta = 8f },
                new DialogueLine { speaker = "Robert", text = "No sé de qué hablan. Yo solo vinimos por el funeral." },
                new DialogueLine { speaker = "Narrador", text = "Las respuestas no llegan. Pero todos saben que el interrogatorio acaba de comenzar.", setFlag = "chapter2.npc_talked_sincere" }
            }
        };

        DialogueNode resultSeparate = new DialogueNode
        {
            id = "result_separate",
            endsConversation = true,
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = "Narrador", text = "Decides que es mejor no presionar a nadie ahora. El silencio es más elocuente que cualquier palabra." },
                new DialogueLine { speaker = "Narrador", text = "El grupo se dispersa en el estudio. Cada uno toma distancia del otro." },
                new DialogueLine { speaker = "Narrador", text = "Ahora tienes libertad para explorar lo que quieras.", setFlag = "chapter2.npc_separated" }
            }
        };

        return new DialogueConversation
        {
            id = "chapter2_initial_decision",
            nodes = new List<DialogueNode> { start, resultTalkSincere, resultSeparate }
        };
    }

    private static DialogueConversation BuildChapter2BookDecision()
    {
        DialogueNode start = new DialogueNode
        {
            id = "start",
            endsConversation = false,
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = "Narrador", text = "Ben nota que regresas con el libro de contabilidad en la mano. Su expresión cambia dramáticamente." },
                new DialogueLine { speaker = "Ben", text = "De dónde sacaste eso? Eso es... eso es propiedad privada de Simón." },
                new DialogueLine { speaker = "Narrador", text = "¿Qué haces ahora?" }
            },
            choices = new List<DialogueChoice>
            {
                new DialogueChoice
                {
                    id = "confront_ben",
                    text = "Confrontar",
                    nextNodeId = "result_confront_ben",
                    anxietyDelta = 15f,
                    setFlag = "chapter2.choice.confront_ben"
                },
                new DialogueChoice
                {
                    id = "search_bedroom",
                    text = "Subir a buscar la habitación de Simon",
                    nextNodeId = "result_search_bedroom",
                    anxietyDelta = 8f,
                    setFlag = "chapter2.choice.search_bedroom"
                }
            }
        };

        DialogueNode resultConfrontBen = new DialogueNode
        {
            id = "result_confront_ben",
            endsConversation = true,
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = "Jugador", text = "Las entradas marcadas con 'B'. Los montos desaparecidos. Necesito saber qué pasó entre Simón y tú." },
                new DialogueLine { speaker = "Ben", text = "Eso... eso no es lo que parece. Simón y yo teníamos un arreglo. Un préstamo informal.", anxietyDelta = 10f },
                new DialogueLine { speaker = "Narrador", text = "Sus manos tiemblan visiblemente. La mentira es transparente." },
                new DialogueLine { speaker = "Ben", text = "Pero todo fue devuelto. Todo. Simón debe tener mis pagos anotados en otro lugar.", anxietyDelta = 8f },
                new DialogueLine { speaker = "Narrador", text = "Pero no los tiene. Y Ben lo sabe. La confrontación deja un silencio devastador. Ahora todos en la sala entienden que Ben oculta algo más profundo.", setFlag = "chapter2.ben_confronted" }
            }
        };

        DialogueNode resultSearchBedroom = new DialogueNode
        {
            id = "result_search_bedroom",
            endsConversation = true,
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = "Narrador", text = "Ben empalidece. Sabe que evitaste la confrontación." },
                new DialogueLine { speaker = "Narrador", text = "Guardas el libro por ahora. Hay preguntas más urgentes en la habitación de Simón." },
                new DialogueLine { speaker = "Narrador", text = "Subes solo. Los escalones crujen bajo tus pies. El grupo permanece en la sala de abajo.", anxietyDelta = 5f, setFlag = "chapter2.choosing_bedroom_route" }
            }
        };

        return new DialogueConversation
        {
            id = "chapter2_book_decision",
            nodes = new List<DialogueNode> { start, resultConfrontBen, resultSearchBedroom }
        };
    }
}
