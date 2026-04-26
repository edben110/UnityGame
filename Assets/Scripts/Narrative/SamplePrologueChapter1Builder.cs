using System.Collections.Generic;
using UnityEngine;

public class SamplePrologueChapter1Builder : MonoBehaviour
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
            Debug.LogWarning("SamplePrologueChapter1Builder no encontro DialogueLibrary.");
            return;
        }

        EnsureData();
    }

    [ContextMenu("Generate Sample Data")]
    public void EnsureData()
    {
        bool hasData = targetLibrary.HasAnyConversation();
        bool hasNpcLayer = targetLibrary.HasConversation("chapter1_npc_robert") && targetLibrary.HasConversation("chapter1_npc_lisa");
        if (hasData && hasNpcLayer)
        {
            return;
        }

        List<DialogueConversation> sample = new List<DialogueConversation>
        {
            BuildPrologueIntro(),
            BuildChapter1Intro(),
            BuildLobbyBookConversation(),
            BuildLobbyCoatConversation(),
            BuildLobbyPhotoConversation(),
            BuildNpcConversation("chapter1_npc_robert", "Robert", "Vine por una carta legal. Hay asuntos de herencia pendientes.", "No confiaria en separarnos. Esta casa tiene mas preguntas que respuestas."),
            BuildNpcItemQuestionConversation("chapter1_npc_robert_item_foto_padre_hijo", "Robert", "La foto de la chimenea dice 'Padre e hijo, 1987'.", "No sabia que quedaba una copia aqui. Ese hombre era el padre de Simón... y tambien el mio. No estoy listo para contarlo al resto."),
            BuildNpcConversation("chapter1_npc_ana", "Ana", "Simón era un cliente brillante, pero guardaba demasiado silencio.", "Si encontramos mas pistas en el lobby, podriamos entender por que nos llamo."),
            BuildNpcItemQuestionConversation("chapter1_npc_ana_item_foto_padre_hijo", "Ana", "En la foto antigua Robert aparto la mirada enseguida.", "Lo vi. Esa reaccion no es casualidad. Si esa imagen sigue apareciendo en nuestras pistas, la historia familiar de Simón es mas importante de lo que parece."),
            BuildNpcConversation("chapter1_npc_ben", "Ben", "Yo llevaba las cuentas. No quiero problemas, solo aclarar todo.", "Escucha, si hablas conmigo seguido me mantengo estable. Si me dejas solo, me derrumbo."),
            BuildNpcItemQuestionConversation("chapter1_npc_ben_item_foto_padre_hijo", "Ben", "Necesito que me digas que sabes de la foto de la chimenea.", "Si la foto menciona al padre de Simón, entonces hay herencias y decisiones viejas metidas en esto. Robert sabe algo, seguro."),
            BuildNpcConversation("chapter1_npc_lisa", "Lisa", "La version oficial de su muerte no me convence.", "Si encontramos evidencia real, podremos proteger al grupo de decisiones ciegas."),
            BuildNpcItemQuestionConversation("chapter1_npc_lisa_item_foto_padre_hijo", "Lisa", "Encontramos una foto: 'Padre e hijo, 1987'.", "Esa foto conecta con lo que investigaba. Si Robert evita mirarla, es porque teme que alguien ate su nombre al de Simón."),
            BuildNpcConversation("chapter1_npc_lucas", "Lucas", "Trabaje con Simón en el taller. Conozco varios rincones de la mansion.", "Si notas que mi ansiedad sube, hablame antes de explorar otra zona."),
            BuildNpcItemQuestionConversation("chapter1_npc_lucas_item_foto_padre_hijo", "Lucas", "Quiero preguntarte por la foto de Simón con un hombre mayor.", "Simón nunca hablaba de su familia, pero guardaba esa foto cerca del fuego, como si la mirara seguido. Esa pista importa."),
            BuildChapter1Decision()
        };

        targetLibrary.ReplaceConversations(sample);
        Debug.Log("Se genero contenido de muestra para prologo y capitulo 1.");
    }

    private static DialogueConversation BuildPrologueIntro()
    {
        DialogueNode start = new DialogueNode
        {
            id = "start",
            endsConversation = false,
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = "Narrador", text = "Europa, 1943. La mansion de Simon recibe a cinco visitantes unidos por secretos." },
                new DialogueLine { speaker = "Narrador", text = "No estan aqui solo por duelo. Todos buscan algo.", anxietyDelta = 5f },
                new DialogueLine { speaker = "Narrador", text = "Tu rol: observar, hablar y evitar que la ansiedad rompa al grupo." }
            },
            choices = new List<DialogueChoice>
            {
                new DialogueChoice { id = "focus_group", text = "Prometer mantener unido al grupo", nextNodeId = "end", anxietyDelta = -5f, setFlag = "choice.prologue.unity" },
                new DialogueChoice { id = "focus_truth", text = "Prometer buscar la verdad a cualquier costo", nextNodeId = "end", anxietyDelta = 5f, setFlag = "choice.prologue.truth" }
            }
        };

        DialogueNode end = new DialogueNode
        {
            id = "end",
            endsConversation = true,
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = "Narrador", text = "Prologo completado. Comienza el Capitulo 1: La Llegada." }
            }
        };

        return new DialogueConversation
        {
            id = "prologue_intro",
            nodes = new List<DialogueNode> { start, end }
        };
    }

    private static DialogueConversation BuildChapter1Intro()
    {
        DialogueNode start = new DialogueNode
        {
            id = "start",
            endsConversation = true,
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = "Narrador", text = "Capitulo 1, 3:00 PM. El lobby esta en silencio." },
                new DialogueLine { speaker = "Robert", text = "Llegue por una carta. No confio en esta reunion." },
                new DialogueLine { speaker = "Lisa", text = "La noticia de la muerte de Simon fue demasiado vaga.", anxietyDelta = 3f },
                new DialogueLine { speaker = "Narrador", text = "Explora el mapa. Cada objeto puede alterar la ansiedad y la historia." }
            }
        };

        return new DialogueConversation
        {
            id = "chapter1_intro",
            nodes = new List<DialogueNode> { start }
        };
    }

    private static DialogueConversation BuildLobbyBookConversation()
    {
        DialogueNode start = new DialogueNode
        {
            id = "start",
            endsConversation = true,
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = "Narrador", text = "Examinas el libro de visitas. Una entrada esta tachada con tinta roja." },
                new DialogueLine { speaker = "Narrador", text = "Fecha: hace tres semanas. Nombre ilegible.", anxietyDelta = 8f, setFlag = "clue.lobby.book" }
            }
        };

        return new DialogueConversation
        {
            id = "chapter1_lobby_book",
            nodes = new List<DialogueNode> { start }
        };
    }

    private static DialogueConversation BuildLobbyCoatConversation()
    {
        DialogueNode start = new DialogueNode
        {
            id = "start",
            endsConversation = true,
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = "Narrador", text = "Revisas un abrigo colgado en el perchero. En el bolsillo hay una nota." },
                new DialogueLine { speaker = "Nota", text = "No confies en nadie que haya llegado antes que tu.", anxietyDelta = 6f, setFlag = "clue.lobby.coat" }
            }
        };

        return new DialogueConversation
        {
            id = "chapter1_lobby_coat",
            nodes = new List<DialogueNode> { start }
        };
    }

    private static DialogueConversation BuildLobbyPhotoConversation()
    {
        DialogueNode start = new DialogueNode
        {
            id = "start",
            endsConversation = true,
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = "Narrador", text = "En la chimenea hay una foto antigua de Simón con un hombre mayor." },
                new DialogueLine { speaker = "Narrador", text = "Al dorso: 'Padre e hijo, 1987'. Robert aparta la mirada.", anxietyDelta = 5f, setFlag = "clue.lobby.photo", addInventoryItemId = "foto_padre_hijo" }
            }
        };

        return new DialogueConversation
        {
            id = "chapter1_lobby_photo",
            nodes = new List<DialogueNode> { start }
        };
    }

    private static DialogueConversation BuildNpcConversation(string id, string npcName, string lineA, string lineB)
    {
        DialogueNode start = new DialogueNode
        {
            id = "start",
            endsConversation = true,
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = npcName, text = lineA, anxietyDelta = -4f },
                new DialogueLine { speaker = npcName, text = lineB, anxietyDelta = -3f },
                new DialogueLine { speaker = "Narrador", text = "La conversacion te ayuda a contener la tension del grupo." }
            }
        };

        return new DialogueConversation
        {
            id = id,
            nodes = new List<DialogueNode> { start }
        };
    }

    private static DialogueConversation BuildNpcItemQuestionConversation(string id, string npcName, string promptLine, string answerLine)
    {
        DialogueNode start = new DialogueNode
        {
            id = "start",
            endsConversation = true,
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = "Jugador", text = promptLine },
                new DialogueLine { speaker = npcName, text = answerLine, anxietyDelta = -2f },
                new DialogueLine { speaker = "Narrador", text = "La reaccion abre una nueva capa de la historia sin romper el grupo." }
            }
        };

        return new DialogueConversation
        {
            id = id,
            nodes = new List<DialogueNode> { start }
        };
    }

    private static DialogueConversation BuildChapter1Decision()
    {
        DialogueNode start = new DialogueNode
        {
            id = "start",
            endsConversation = false,
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = "Narrador", text = "La tension en el lobby sube. Debes decidir como continua el grupo." }
            },
            choices = new List<DialogueChoice>
            {
                new DialogueChoice { id = "go_together", text = "Explorar juntos la mansion", nextNodeId = "result_together", anxietyDelta = -12f, setFlag = "chapter1.choice.together" },
                new DialogueChoice { id = "split", text = "Separarse para cubrir mas terreno", nextNodeId = "result_split", anxietyDelta = 14f, setFlag = "chapter1.choice.split" },
                new DialogueChoice { id = "talk", text = "Forzar una conversacion sincera sobre Simon", nextNodeId = "result_talk", anxietyDelta = -4f, setFlag = "chapter1.choice.talk" }
            }
        };

        DialogueNode resultTogether = new DialogueNode
        {
            id = "result_together",
            endsConversation = true,
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = "Narrador", text = "El grupo avanza unido. El miedo sigue, pero nadie queda solo." }
            }
        };

        DialogueNode resultSplit = new DialogueNode
        {
            id = "result_split",
            endsConversation = true,
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = "Narrador", text = "Cada quien toma una direccion. La mansion traga el sonido de los pasos.", anxietyDelta = 6f },
                new DialogueLine { speaker = "Narrador", text = "Se desbloquea una ruta mas oscura para capitulo 2.", setFlag = "route.dark" }
            }
        };

        DialogueNode resultTalk = new DialogueNode
        {
            id = "result_talk",
            endsConversation = true,
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = "Narrador", text = "Nadie dice toda la verdad, pero el silencio deja de ser absoluto." }
            }
        };

        return new DialogueConversation
        {
            id = "chapter1_decision",
            nodes = new List<DialogueNode> { start, resultTogether, resultSplit, resultTalk }
        };
    }
}
