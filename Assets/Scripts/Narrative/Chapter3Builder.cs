using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Builder de contenido narrativo para el Capítulo 3: Habitación de Simón.
/// Inyecta conversaciones en la DialogueLibrary usando AddConversations (no destructivo).
///
/// FLUJO NARRATIVO REFACTORIZADO:
/// 1. Jugador obtiene acceso a la habitación de Simón
/// 2. Interactúa con hotspots para obtener evidencia
/// 3. Obtiene la llave pequeña (requisito para avanzar)
/// 4. Puede intentar abrir el archivador del estudio O
/// 5. Puede buscar el Ala Norte
/// 
/// VALIDACIONES CRÍTICAS:
///   - El jugador solo puede avanzar si tiene la llave pequeña
///   - Debe haber obtenido la carta de Simón
///   - Decisión controlada (no automática)
///   - Mismo nivel de robustez que Capítulo 2
///
/// Hotspots de la Habitación:
///   - Vaso de agua (condensación → Simón estuvo aquí hace poco)
///   - Mapa en la pared (marca el Ala Norte)
///   - Carta inconclusa ("Si alguien lee esto, no estoy muerto...")
///   - Mesita de noche (contiene la llave pequeña del archivador)
///   - Cama deshecha (evidencia de uso reciente)
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

        // if (targetLibrary.HasConversation("chapter3_intro") && targetLibrary.HasConversation("chapter3_decision"))
        // {
        //     Debug.Log("Chapter3Builder: Conversaciones del Cap 3 ya presentes. Saltando generación.");
        //     return;
        // }

        List<DialogueConversation> chapter3 = new List<DialogueConversation>
        {
            BuildChapter3Intro(),
            // ─── Hotspots de la Habitación ───
            BuildHabitacionVasoAgua(),
            BuildHabitacionMapaPared(),
            BuildHabitacionCartaInconclusa(),
            BuildHabitacionMesitaNoche(),
            BuildHabitacionCama(),
            // ─── Hotspots de la Galería ───
            BuildGaleriaPinturaMisteriosa(),
            BuildGaleriaCajaPuzzle2(),
            BuildGaleriaCajaPuzzle3(),
            BuildGaleriaAlfombraSospechosa(),
            // ─── NPC: diálogos de ansiedad Cap 3 ───
            BuildNpcAnxiety("chapter3_npc_robert", "Robert",
                "Tardas demasiado allí dentro. ¿Encontraste algo en su habitación o solo estás perdiendo el tiempo?",
                "Mientras tú juegas a ser detective, nosotros seguimos atrapados aquí."),
            BuildNpcAnxiety("chapter3_npc_ana", "Ana",
                "¿Cómo estaba la habitación? Por favor dime que no tocaste nada que no debías.",
                "Me estoy empezando a preguntar si realmente vine por un funeral o por una trampa."),
            BuildNpcAnxiety("chapter3_npc_ben", "Ben",
                "No me gusta que te separes del grupo. Deberíamos mantenernos juntos.",
                "Cualquier cosa que encuentres ahí, espero que nos lo digas."),
            BuildNpcAnxiety("chapter3_npc_lisa", "Lisa",
                "¿Algún hallazgo periodístico en su santuario privado? Cuéntamelo todo.",
                "Seguro que Simón dejó migajas para que las sigas. Es su estilo."),
            BuildNpcAnxiety("chapter3_npc_lucas", "Lucas",
                "Simón nunca dejaba a nadie entrar ahí. Qué ironía que ahora revolvamos sus cosas.",
                "Solo espero que no encuentres algo que sea mejor dejar enterrado."),
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
            BuildNpcItemQuestion("chapter3_npc_robert_item_carta_inconclusa", "Robert",
                "Encontré esta carta en su habitación. Dice que podría estar vivo.",
                "¿Vivo? Eso... eso cambia todo. Si está vivo... entonces esto no es un funeral. Es una trampa."),
            BuildNpcItemQuestion("chapter3_npc_robert_item_carta_padre", "Robert",
                "Encontré una carta manuscrita. Dice 'Padre e hijo' y tiene fecha de 1929. ¿Qué sabes de esto?",
                "Esa carta... no debería existir. Mi padre la escribió antes de morir. Simón y yo somos hermanos. Nunca se lo dije a nadie. Él guardó esa carta como prueba de que compartimos sangre."),
            BuildNpcItemQuestion("chapter3_npc_ana_item_mapa_ala_norte", "Ana",
                "Encontré un mapa en su habitación. Señala el Ala Norte. ¿Sabías que existía?",
                "¿Un mapa? No. Nunca vi esa parte de la mansión. Simón nunca la mencionó."),
            BuildNpcItemQuestion("chapter3_npc_ben_item_papeles_lisa", "Ben",
                "Encontré unos papeles de Lisa bajo su cama. Los estaba buscando alguien más.",
                "¿Papeles de Lisa? Eso ya es más serio. Si estaban escondidos, es porque no debían verse todavía."),
            BuildNpcItemQuestion("chapter3_npc_lisa_item_vaso_agua", "Lisa",
                "Encontré un vaso de agua en su cuarto. Todavía tenía condensación. Alguien estuvo ahí hace muy poco.",
                "¿Condensación? Horas. Tal vez menos. Esto confirma mi teoría. La muerte fue un montaje."),
            BuildNpcItemQuestion("chapter3_npc_lucas_item_caja_puzzle_1", "Lucas",
                "Encontré esta caja puzzle bajo su cama. ¿Te resulta familiar?",
                "¿Una caja puzzle? A Simón siempre le gustaron esos artilugios. Seguro esconde algo importante."),
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
                new DialogueLine { speaker = "Jugador", text = "Por fin… esta llave debería abrir la habitación de Simón." },
                new DialogueLine { speaker = "Narrador", text = "La cerradura cede con un chasquido mecánico. La puerta se abre, revelando el santuario del fallecido." },
                new DialogueLine { speaker = "Jugador", text = "Aquí es donde Simón pasaba más tiempo. Veamos qué escondía aquí dentro.", anxietyDelta = 6f },
                new DialogueLine { speaker = "Narrador", text = "El cuarto es espartano: una cama, una mesita de noche, un escritorio y un mapa clavado en la pared. Un vaso de agua descansa en la mesita.", anxietyDelta = 8f },
                new DialogueLine { speaker = "Jugador", text = "Debo investigar rápido antes de que los demás empiecen a hacer preguntas.", setFlag = "chapter3.intro.seen" }
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
                new DialogueLine { speaker = "Narrador", text = "Simón está vivo. Y está atrapado.", setFlag = "simon_vivo", addInventoryItemId = "HS_Unfinished_Letter" }
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
                new DialogueLine { speaker = "Narrador", text = "La mesita de noche tiene un solo cajón. Lo abres con cuidado. Dentro, encuentras algunas notas esparcidas." },
                new DialogueLine { speaker = "Jugador", text = "Parecen registros médicos. 'Dosis incrementada... alucinaciones recurrentes'. Simón estaba medicado.", anxietyDelta = 8f },
                new DialogueLine { speaker = "Narrador", text = "También hay una llave pequeña envuelta en un pañuelo doblado. Es la clase de llave que Simón no dejaría a la vista.", setFlag = "clue.habitacion.mesita_noche", addInventoryItemId = "llave_pequena" }
            }
        };

        return new DialogueConversation
        {
            id = "chapter3_Bedside_Table",
            nodes = new List<DialogueNode> { start }
        };
    }

    // ═══════════════════════════════════════════════════════════════
    // HOTSPOTS DE LA GALERÍA
    // ═══════════════════════════════════════════════════════════════

    private static DialogueConversation BuildGaleriaPinturaMisteriosa()
    {
        DialogueNode start = new DialogueNode
        {
            id = "start",
            endsConversation = true,
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = "Narrador", text = "Un retrato familiar antiguo. Los rostros están desgastados, excepto uno que parece haber sido retocado recientemente." },
                new DialogueLine { speaker = "Jugador", text = "Hay algo detrás del marco.", anxietyDelta = 5f },
                new DialogueLine { speaker = "Narrador", text = "Al mover el cuadro, encuentras un compartimento hueco.", setFlag = "clue.galeria.pintura_misteriosa" }
            }
        };

        return new DialogueConversation
        {
            id = "chapter3_Gallery_Painting",
            nodes = new List<DialogueNode> { start }
        };
    }

    private static DialogueConversation BuildGaleriaCajaPuzzle2()
    {
        DialogueNode start = new DialogueNode
        {
            id = "start",
            endsConversation = true,
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = "Narrador", text = "Una segunda caja puzzle descansa sobre una peana de mármol. El mecanismo es similar a la primera, pero los símbolos son diferentes." },
                new DialogueLine { speaker = "Jugador", text = "Si uso el mismo razonamiento que con la primera caja, podría abrirla." },
                new DialogueLine { speaker = "Narrador", text = "Tras manipular los engranajes por unos minutos, el compartimento se abre con un clic.", requiredFlag = "clue.habitacion.caja_puzzle_1", setFlag = "clue.galeria.caja_puzzle_2", addInventoryItemId = "GalleryKey" },
                new DialogueLine { speaker = "Jugador", text = "Dentro hay una llave de la galería. La caja del dormitorio tenía que esconder algo más grande.", requiredFlag = "clue.galeria.caja_puzzle_2" }
            }
        };

        return new DialogueConversation
        {
            id = "chapter3_Gallery_PuzzleBox2",
            nodes = new List<DialogueNode> { start }
        };
    }

    private static DialogueConversation BuildGaleriaCajaPuzzle3()
    {
        DialogueNode start = new DialogueNode
        {
            id = "start",
            endsConversation = true,
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = "Narrador", text = "En un rincón apartado de la galería descansa la tercera y última caja puzzle." },
                new DialogueLine { speaker = "Jugador", text = "Si las dos primeras escondían llaves, esta debe tener algo importante." },
                new DialogueLine { speaker = "Narrador", text = "El intrincado mecanismo cede tras aplicar la lógica de los anteriores.", requiredFlag = "clue.galeria.caja_puzzle_2", setFlag = "clue.galeria.caja_puzzle_3", addInventoryItemId = "BasementKey" },
                new DialogueLine { speaker = "Jugador", text = "Una llave pesada de hierro negro. Tiene una etiqueta desgastada que apenas se lee: 'Sótano'.", requiredFlag = "clue.galeria.caja_puzzle_3", anxietyDelta = 10f }
            }
        };

        return new DialogueConversation
        {
            id = "chapter3_Gallery_PuzzleBox3",
            nodes = new List<DialogueNode> { start }
        };
    }

    private static DialogueConversation BuildGaleriaAlfombraSospechosa()
    {
        DialogueNode start = new DialogueNode
        {
            id = "start",
            endsConversation = true,
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = "Narrador", text = "Una gran alfombra persa cubre el centro de la galería. Al pisarla, suena ligeramente hueco." },
                new DialogueLine { speaker = "Jugador", text = "El sonido no es consistente con el resto del suelo de madera maciza." },
                new DialogueLine { speaker = "Narrador", text = "Acorralando el mueble cercano y retirando la pesada alfombra, revelas una puerta de madera oscura incrustada en el suelo. Una cerradura de hierro negro bloquea el acceso." },
                new DialogueLine { speaker = "Jugador", text = "Una trampilla oculta. Debe llevar al sótano.", anxietyDelta = 15f },
                new DialogueLine { speaker = "Narrador", text = "Has descubierto la entrada oculta al sótano.", setFlag = "BasementDiscovered" }
            }
        };

        return new DialogueConversation
        {
            id = "chapter3_Gallery_SuspectCarpet",
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
                new DialogueLine { speaker = "Narrador", text = "Te arrodillas y revisas debajo de la cama. Está oscuro, pero alcanzas a tocar algo sólido." },
                new DialogueLine { speaker = "Narrador", text = "Sacas una caja de madera con un extraño mecanismo en la tapa. Parece un rompecabezas.", setFlag = "clue.habitacion.caja_puzzle_1", addInventoryItemId = "caja_puzzle_1" },
                new DialogueLine { speaker = "Narrador", text = "Si consigues abrirla más tarde, quizá esconda algo útil para la galería.", anxietyDelta = 5f },
                new DialogueLine { speaker = "Narrador", text = "Junto a la caja, hay unos papeles sueltos. Lisa los estaba buscando. Los guardas porque pueden cambiar el final.", anxietyDelta = 5f, setFlag = "FoundLisaDocuments", addInventoryItemId = "papeles_lisa" }
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
                new DialogueLine { speaker = "Jugador", text = "Esta carta lo cambia todo. Simón podría estar vivo... o alguien quiere hacérmelo creer." },
                new DialogueLine { speaker = "Jugador", text = "¿Qué debería hacer con esta información?" }
            },
            choices = new List<DialogueChoice>
            {
                new DialogueChoice
                {
                    id = "confront_group",
                    text = "Mostrar la carta al grupo y proponer buscar a Simón ahora mismo",
                    nextNodeId = "result_confront_group",
                    anxietyDelta = 12f,
                    setFlag = "chapter3.choice.confront_group",
                    requiredFlag = "clue.habitacion.carta_inconclusa"
                },
                new DialogueChoice
                {
                    id = "keep_hidden",
                    text = "Guardar la carta y seguir recopilando información antes de actuar",
                    nextNodeId = "result_keep_hidden",
                    anxietyDelta = 4f,
                    setFlag = "chapter3.choice.keep_hidden",
                    requiredFlag = "clue.habitacion.carta_inconclusa"
                },
                new DialogueChoice
                {
                    id = "ask_robert",
                    text = "Preguntarle a Robert directamente sobre su relación con Simón",
                    nextNodeId = "result_ask_robert",
                    anxietyDelta = 8f,
                    setFlag = "chapter3.choice.ask_robert",
                    requiredFlag = "clue.habitacion.carta_inconclusa"
                }
            }
        };

        DialogueNode resultConfrontGroup = new DialogueNode
        {
            id = "result_confront_group",
            endsConversation = true,
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = "Jugador", text = "Escuchen todos. Encontré una carta de Simón en su habitación. Dice que no está muerto." },
                new DialogueLine { speaker = "Ana", text = "¿Qué? No... eso no tiene sentido. Vimos el cuerpo.", anxietyDelta = 12f },
                new DialogueLine { speaker = "Ben", text = "Nos mintieron. Todo esto es una trampa. ¿Por qué nos trajeron aquí?", anxietyDelta = 15f },
                new DialogueLine { speaker = "Lisa", text = "Lo sabía. Desde el principio supe que algo no cuadraba. La escena era demasiado perfecta.", anxietyDelta = 5f },
                new DialogueLine { speaker = "Narrador", text = "El grupo estalla en caos. La confianza que quedaba se desmorona. Alguien en esta habitación sabe la verdad. Y está aquí contigo.", anxietyDelta = 10f, setFlag = "chapter3.group_confronted" }
            }
        };

        DialogueNode resultKeepHidden = new DialogueNode
        {
            id = "result_keep_hidden",
            endsConversation = true,
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = "Jugador", text = "No puedo confiar en nadie todavía. Guardaré la carta hasta que sepa más." },
                new DialogueLine { speaker = "Narrador", text = "Escondes la carta en tu bolsillo. Cada sombra en la casa ahora parece ocultar un secreto más oscuro.", anxietyDelta = 5f, setFlag = "chapter3.kept_hidden" }
            }
        };

        DialogueNode resultAskRobert = new DialogueNode
        {
            id = "result_ask_robert",
            endsConversation = true,
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = "Jugador", text = "Robert, encontré algo. Una carta. Necesito que me digas la verdad sobre ti y Simón." },
                new DialogueLine { speaker = "Robert", text = "¿Qué estás insinuando? Él era mi hermano, aunque nunca se lo dije a nadie. ¿Crees que yo...?", anxietyDelta = 15f },
                new DialogueLine { speaker = "Narrador", text = "La tensión corta el aire. Acabas de revelar una carta que quizás no debías, pero tienes una pieza vital de información.", anxietyDelta = 5f, setFlag = "chapter3.asked_robert" }
            }
        };

        return new DialogueConversation
        {
            id = "chapter3_decision",
            nodes = new List<DialogueNode> { start, resultConfrontGroup, resultKeepHidden, resultAskRobert }
        };
    }
}
