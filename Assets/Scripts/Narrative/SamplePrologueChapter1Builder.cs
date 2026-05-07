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

public void EnsureData()
    {
        if (targetLibrary == null)
        {
            targetLibrary = GetComponent<DialogueLibrary>();
        }

        if (targetLibrary == null)
        {
            Debug.LogWarning("SamplePrologueChapter1Builder no encontro DialogueLibrary.");
            return;
        }

        string[] requiredConversationIds =
        {
            "prologue_intro",
            "chapter1_intro",
            "chapter1_lobby_book",
            "chapter1_lobby_coat",
            "chapter1_lobby_photo",
            "chapter1_lobby_newspaper",
            // NPC: conversacion principal (ansiedad)
            "chapter1_npc_robert",
            "chapter1_npc_ana",
            "chapter1_npc_ben",
            "chapter1_npc_lisa",
            "chapter1_npc_lucas",
            // NPC: conversacion critica (100% ansiedad)
            "chapter1_npc_robert_critical",
            "chapter1_npc_ana_critical",
            "chapter1_npc_ben_critical",
            "chapter1_npc_lisa_critical",
            "chapter1_npc_lucas_critical",
            // NPC: motivo de visita
            "chapter1_npc_robert_motivo",
            "chapter1_npc_ana_motivo",
            "chapter1_npc_ben_motivo",
            "chapter1_npc_lisa_motivo",
            "chapter1_npc_lucas_motivo",
            // NPC: preguntar sobre objetos
            "chapter1_npc_robert_item_foto_padre_hijo",
            "chapter1_npc_robert_item_lobby_book",
            "chapter1_npc_robert_item_lobby_coat",
            "chapter1_npc_robert_item_lobby_newspaper",
            "chapter1_npc_ana_item_foto_padre_hijo",
            "chapter1_npc_ana_item_lobby_book",
            "chapter1_npc_ana_item_lobby_coat",
            "chapter1_npc_ana_item_lobby_newspaper",
            "chapter1_npc_ben_item_foto_padre_hijo",
            "chapter1_npc_ben_item_lobby_book",
            "chapter1_npc_ben_item_lobby_coat",
            "chapter1_npc_ben_item_lobby_newspaper",
            "chapter1_npc_lisa_item_foto_padre_hijo",
            "chapter1_npc_lisa_item_lobby_book",
            "chapter1_npc_lisa_item_lobby_coat",
            "chapter1_npc_lisa_item_lobby_newspaper",
            "chapter1_npc_lucas_item_foto_padre_hijo",
            "chapter1_npc_lucas_item_lobby_book",
            "chapter1_npc_lucas_item_lobby_coat",
            "chapter1_npc_lucas_item_lobby_newspaper",
            "chapter1_decision"
        };

        bool hasAllRequired = targetLibrary.HasAnyConversation();
        for (int i = 0; i < requiredConversationIds.Length; i++)
        {
            if (!targetLibrary.HasConversation(requiredConversationIds[i]))
            {
                hasAllRequired = false;
                break;
            }
        }

        if (hasAllRequired)
        {
            return;
        }

        targetLibrary.ClearAll();

        List<DialogueConversation> sample = new List<DialogueConversation>
        {
            BuildPrologueIntro(),
            BuildChapter1Intro(),
            BuildLobbyBookConversation(),
            BuildLobbyCoatConversation(),
            BuildLobbyPhotoConversation(),
            BuildLobbyNewspaperConversation(),
            // Ansiedad (calmar)
            BuildNpcAnxietyConversation_Robert(),
            BuildNpcAnxietyConversation_Ana(),
            BuildNpcAnxietyConversation_Ben(),
            BuildNpcAnxietyConversation_Lisa(),
            BuildNpcAnxietyConversation_Lucas(),
            // Criticos 100% ansiedad
            BuildNpcCriticalConversation("chapter1_npc_robert_critical", "Robert", "No... no me dejes aqu\u00ed. La casa escucha. Todos escuchan."),
            BuildNpcCriticalConversation("chapter1_npc_ana_critical", "Ana", "No mires los cuadros. Me est\u00e1n mirando de vuelta... todos a la vez."),
            BuildNpcCriticalConversation("chapter1_npc_ben_critical", "Ben", "No cierres la puerta... por favor. Escucho pasos cuando parpadeo."),
            BuildNpcCriticalConversation("chapter1_npc_lisa_critical", "Lisa", "No era un reflejo... no era yo. No era yo en el vidrio."),
            BuildNpcCriticalConversation("chapter1_npc_lucas_critical", "Lucas", "No me toques. Si me quedo quieto, tal vez no me vea."),
            // Motivo de visita
            BuildNpcMotivoConversation_Robert(),
            BuildNpcMotivoConversation_Ana(),
            BuildNpcMotivoConversation_Ben(),
            BuildNpcMotivoConversation_Lisa(),
            BuildNpcMotivoConversation_Lucas(),
            // Items
            BuildNpcItemQuestionConversation("chapter1_npc_robert_item_foto_padre_hijo", "Robert", "La foto de la chimenea dice 'Padre e hijo, 1987'.", "No sab\u00eda que quedaba una copia aqu\u00ed. Ese hombre era el padre de Sim\u00f3n... y tambi\u00e9n el m\u00edo. No estoy listo para contarlo al resto."),
            BuildNpcItemQuestionConversation("chapter1_npc_robert_item_lobby_book", "Robert", "En el libro de visitas hay una entrada tachada.", "Esa clase de tachadura no es casual. En tiempos as\u00ed, la gente borra nombres cuando teme represalias."),
            BuildNpcItemQuestionConversation("chapter1_npc_robert_item_lobby_coat", "Robert", "En el abrigo encontr\u00e9 una nota: 'No conf\u00edes en nadie que haya llegado antes'.", "Suena a advertencia de alguien que conoce esta casa mejor que nosotros."),
            BuildNpcItemQuestionConversation("chapter1_npc_robert_item_lobby_newspaper", "Robert", "El peri\u00f3dico habla de un incendio en el puerto.", "Si reabrieron esa investigaci\u00f3n, alguien aqu\u00ed va a ponerse nervioso."),
            BuildNpcItemQuestionConversation("chapter1_npc_ana_item_foto_padre_hijo", "Ana", "En la foto antigua Robert apart\u00f3 la mirada enseguida.", "Lo vi. Esa reacci\u00f3n no es casualidad. Si esa imagen sigue apareciendo en nuestras pistas, la historia familiar de Sim\u00f3n es m\u00e1s importante de lo que parece."),
            BuildNpcItemQuestionConversation("chapter1_npc_ana_item_lobby_book", "Ana", "El libro de visitas tiene una entrada tachada.", "En el arte, borrar un nombre es un mensaje. Alguien quiso que ese visitante no existiera."),
            BuildNpcItemQuestionConversation("chapter1_npc_ana_item_lobby_coat", "Ana", "Hay una nota escondida en un abrigo.", "Alguien dej\u00f3 esa advertencia con tiempo. Eso significa premeditaci\u00f3n."),
            BuildNpcItemQuestionConversation("chapter1_npc_ana_item_lobby_newspaper", "Ana", "El peri\u00f3dico menciona un incendio reabierto.", "Si Sim\u00f3n fue testigo, ese caso explica por qu\u00e9 nos reunieron."),
            BuildNpcItemQuestionConversation("chapter1_npc_ben_item_foto_padre_hijo", "Ben", "Necesito que me digas que sabes de la foto de la chimenea.", "Si la foto menciona al padre de Sim\u00f3n, entonces hay herencias y decisiones viejas metidas en esto. Robert sabe algo, seguro."),
            BuildNpcItemQuestionConversation("chapter1_npc_ben_item_lobby_book", "Ben", "El libro de visitas tiene una entrada tachada.", "Ese tipo de detalle es lo que termina costando dinero... y vidas."),
            BuildNpcItemQuestionConversation("chapter1_npc_ben_item_lobby_coat", "Ben", "Una nota escondida en un abrigo dice que no confiemos en quien lleg\u00f3 antes.", "Si alguien lleg\u00f3 antes, tuvo tiempo de preparar algo. Eso me inquieta."),
            BuildNpcItemQuestionConversation("chapter1_npc_ben_item_lobby_newspaper", "Ben", "El peri\u00f3dico habla de un incendio y una investigaci\u00f3n reabierta.", "Ese incendio fue un desastre financiero. Si alguien insiste en reabrirlo, es porque hay culpables."),
            BuildNpcItemQuestionConversation("chapter1_npc_lisa_item_foto_padre_hijo", "Lisa", "Encontramos una foto: 'Padre e hijo, 1987'.", "Esa foto conecta con lo que investigaba. Si Robert evita mirarla, es porque teme que alguien ate su nombre al de Sim\u00f3n."),
            BuildNpcItemQuestionConversation("chapter1_npc_lisa_item_lobby_book", "Lisa", "El libro de visitas tiene un nombre borrado.", "Eso es lo primero que revisar\u00eda si estuviera investigando. Ocultar un nombre es una pista en s\u00ed misma."),
            BuildNpcItemQuestionConversation("chapter1_npc_lisa_item_lobby_coat", "Lisa", "En el abrigo hay una nota de advertencia.", "Eso suena a alguien intentando salvarnos sin exponerse. Quiero saber qui\u00e9n la escribi\u00f3."),
            BuildNpcItemQuestionConversation("chapter1_npc_lisa_item_lobby_newspaper", "Lisa", "El peri\u00f3dico menciona un incendio reabierto.", "Ese es el incidente que vengo siguiendo. Sim\u00f3n estaba ah\u00ed."),
            BuildNpcItemQuestionConversation("chapter1_npc_lucas_item_foto_padre_hijo", "Lucas", "Quiero preguntarte por la foto de Sim\u00f3n con un hombre mayor.", "Sim\u00f3n nunca hablaba de su familia, pero guardaba esa foto cerca del fuego, como si la mirara seguido. Esa pista importa."),
            BuildNpcItemQuestionConversation("chapter1_npc_lucas_item_lobby_book", "Lucas", "En el libro de visitas hay una entrada tachada.", "Sim\u00f3n era cuidadoso con sus visitas. Si borr\u00f3 a alguien, ese alguien ten\u00eda peso."),
            BuildNpcItemQuestionConversation("chapter1_npc_lucas_item_lobby_coat", "Lucas", "En el abrigo hay una nota que advierte desconfiar.", "Suena a Sim\u00f3n. Siempre dejaba pistas como si no pudiera hablar directo."),
            BuildNpcItemQuestionConversation("chapter1_npc_lucas_item_lobby_newspaper", "Lucas", "El peri\u00f3dico habla del incendio del puerto.", "Sim\u00f3n mencion\u00f3 ese incendio una vez. Dijo que alguien miente sobre lo que pas\u00f3."),
            BuildChapter1Decision()
        };

        targetLibrary.ReplaceConversations(sample);
        Debug.Log($"Se genero contenido de muestra para prologo y capitulo 1. Total: {sample.Count} conversations.");
        Debug.Log($"Library now contains {targetLibrary.GetAllConversationIds().Count} conversations.");

        int criticalCount = 0;
        foreach (string id in targetLibrary.GetAllConversationIds())
        {
            if (id.Contains("critical"))
            {
                criticalCount++;
                Debug.Log($"Critical conversation found: {id}");
            }
        }

        Debug.Log($"Critical conversations in library: {criticalCount}");
        if (criticalCount != 5)
        {
            Debug.LogWarning($"Expected 5 critical conversations, but found {criticalCount}.");
        }
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
                new DialogueLine { speaker = "Narrador", text = "Sobre la mesita de entrada hay un libro donde los visitantes de la mansi\u00f3n firmaban su llegada. Las \u00faltimas tres entradas son de los \u00faltimos seis meses." },
                new DialogueLine { speaker = "Narrador", text = "Una de ellas est\u00e1 tachada con tinta roja. El nombre debajo de la tachadura es ilegible, pero la fecha es de hace exactamente tres semanas.", anxietyDelta = 8f, setFlag = "clue.lobby.book" }
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
                new DialogueLine { speaker = "Narrador", text = "Un abrigo de hombre de talla grande. En el bolsillo interior hay una nota manuscrita, sin firma." },
                new DialogueLine { speaker = "Nota", text = "No conf\u00edes en nadie que llegue antes que t\u00fa.", anxietyDelta = 6f },
                new DialogueLine { speaker = "Narrador", text = "La letra es firme, decidida. No es la letra de alguien que escribe con miedo. Es la de alguien que advierte.", setFlag = "clue.lobby.coat" }
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
                new DialogueLine { speaker = "Narrador", text = "Una fotograf\u00eda en blanco y negro de Sim\u00f3n joven, junto a un hombre mayor de rasgos severos. Al dorso, escrito a l\u00e1piz:" },
                new DialogueLine { speaker = "Dorso", text = "Padre e hijo. 1987.", anxietyDelta = 5f },
                new DialogueLine { speaker = "Narrador", text = "Robert, al otro lado de la sala, desv\u00eda la mirada cuando notas que examinas la fotograf\u00eda.", setFlag = "clue.lobby.photo", addInventoryItemId = "foto_padre_hijo" }
            }
        };

        return new DialogueConversation
        {
            id = "chapter1_lobby_photo",
            nodes = new List<DialogueNode> { start }
        };
    }

private static DialogueConversation BuildLobbyNewspaperConversation()
    {
        DialogueNode start = new DialogueNode
        {
            id = "start",
            endsConversation = true,
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = "Narrador", text = "Un peri\u00f3dico local de hace tres d\u00edas. La esquina de la p\u00e1gina de sucesos est\u00e1 doblada. El titular visible dice:" },
                new DialogueLine { speaker = "Titular", text = "Incendio en almac\u00e9n del puerto, investigaci\u00f3n reabierta.", anxietyDelta = 6f },
                new DialogueLine { speaker = "Narrador", text = "Lisa, desde su sill\u00f3n, mira el peri\u00f3dico con una expresi\u00f3n que intenta ser indiferente y no lo consigue.", setFlag = "clue.lobby.newspaper" }
            }
        };

        return new DialogueConversation
        {
            id = "chapter1_lobby_newspaper",
            nodes = new List<DialogueNode> { start }
        };
    }

    // ======= CONVERSACIONES DE ANSIEDAD (tipo 1: calmar al NPC) =======

    private static DialogueConversation BuildNpcAnxietyConversation_Robert()
    {
        DialogueNode start = new DialogueNode
        {
            id = "start",
            endsConversation = true,
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = "Robert", text = "No me gusta este lugar. El silencio pesa. Me cuesta respirar con tanta madera y tanto pasado encima.", anxietyDelta = -8f },
                new DialogueLine { speaker = "Robert", text = "No es miedo. Es una sensaci\u00f3n de deuda.", anxietyDelta = -7f }
            }
        };
        return new DialogueConversation { id = "chapter1_npc_robert", nodes = new List<DialogueNode> { start } };
    }

    private static DialogueConversation BuildNpcAnxietyConversation_Ana()
    {
        DialogueNode start = new DialogueNode
        {
            id = "start",
            endsConversation = true,
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = "Ana", text = "Pens\u00e9 que lo ten\u00eda todo bajo control, pero este lobby me hace sentir observada.", anxietyDelta = -8f },
                new DialogueLine { speaker = "Ana", text = "Como si cada cuadro tuviera memoria.", anxietyDelta = -7f }
            }
        };
        return new DialogueConversation { id = "chapter1_npc_ana", nodes = new List<DialogueNode> { start } };
    }

    private static DialogueConversation BuildNpcAnxietyConversation_Ben()
    {
        DialogueNode start = new DialogueNode
        {
            id = "start",
            endsConversation = true,
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = "Ben", text = "No he dormido en d\u00edas. En serio. Me tiembla la mano y no s\u00e9 si es el fr\u00edo o los nervios.", anxietyDelta = -8f },
                new DialogueLine { speaker = "Ben", text = "Este sitio me aprieta el pecho.", anxietyDelta = -7f }
            }
        };
        return new DialogueConversation { id = "chapter1_npc_ben", nodes = new List<DialogueNode> { start } };
    }

    private static DialogueConversation BuildNpcAnxietyConversation_Lisa()
    {
        DialogueNode start = new DialogueNode
        {
            id = "start",
            endsConversation = true,
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = "Lisa", text = "El silencio me pone alerta. Demasiado quieto, demasiado perfecto.", anxietyDelta = -8f },
                new DialogueLine { speaker = "Lisa", text = "No suelo admitirlo, pero estoy tensa.", anxietyDelta = -7f }
            }
        };
        return new DialogueConversation { id = "chapter1_npc_lisa", nodes = new List<DialogueNode> { start } };
    }

    private static DialogueConversation BuildNpcAnxietyConversation_Lucas()
    {
        DialogueNode start = new DialogueNode
        {
            id = "start",
            endsConversation = true,
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = "Lucas", text = "Siento que no deber\u00eda estar aqu\u00ed. Me cuesta quitarme esa sensaci\u00f3n.", anxietyDelta = -8f },
                new DialogueLine { speaker = "Lucas", text = "La casa me conoce. Y eso me pone nervioso.", anxietyDelta = -7f }
            }
        };
        return new DialogueConversation { id = "chapter1_npc_lucas", nodes = new List<DialogueNode> { start } };
    }

    // ======= CONVERSACIONES DE MOTIVO (tipo 2: por qu\u00e9 vinieron) =======

    private static DialogueConversation BuildNpcMotivoConversation_Robert()
    {
        DialogueNode start = new DialogueNode
        {
            id = "start",
            endsConversation = true,
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = "Robert", text = "Vine porque me lo pidieron. Un abogado, no s\u00e9 qui\u00e9n lo contrat\u00f3, me envi\u00f3 una carta diciendome que hab\u00eda asuntos pendientes relacionados con la herencia de Sim\u00f3n que requer\u00edan mi presencia.", anxietyDelta = -5f },
                new DialogueLine { speaker = "Robert", text = "No lo conoc\u00eda bien. Nos cruzamos en algunos c\u00edrculos. Nada m\u00e1s.", anxietyDelta = -3f },
                new DialogueLine { speaker = "Narrador", text = "Lo dice con demasiada calma. La clase de calma que se ensaya frente al espejo antes de salir de casa." }
            }
        };
        return new DialogueConversation { id = "chapter1_npc_robert_motivo", nodes = new List<DialogueNode> { start } };
    }

    private static DialogueConversation BuildNpcMotivoConversation_Ana()
    {
        DialogueNode start = new DialogueNode
        {
            id = "start",
            endsConversation = true,
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = "Ana", text = "Era mi cliente. Uno de los mejores que he tenido, honestamente. Cuando me dijeron que hab\u00eda muerto...", anxietyDelta = -5f },
                new DialogueLine { speaker = "Ana", text = "Supongo que vine por respeto. Y porque alguien tiene que asegurarse de que su obra quede bien catalogada. No es una colecci\u00f3n menor.", anxietyDelta = -3f },
                new DialogueLine { speaker = "Narrador", text = "Sonr\u00ede brevemente. Sus ojos recorren la sala como tasando cada objeto visible." }
            }
        };
        return new DialogueConversation { id = "chapter1_npc_ana_motivo", nodes = new List<DialogueNode> { start } };
    }

    private static DialogueConversation BuildNpcMotivoConversation_Ben()
    {
        DialogueNode start = new DialogueNode
        {
            id = "start",
            endsConversation = true,
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = "Ben", text = "Sim\u00f3n y yo \u00e9ramos socios, de cierta forma. \u00c9l pintaba, yo me encargaba del lado financiero. Muy informal, ya sabe. Sin contratos de por medio. La gente creativa suele preferirlo as\u00ed.", anxietyDelta = -5f },
                new DialogueLine { speaker = "Ben", text = "La verdad es que me enter\u00e9 de su muerte y quise venir a... no s\u00e9, despedirme. Algo as\u00ed.", anxietyDelta = -3f },
                new DialogueLine { speaker = "Narrador", text = "Hay algo en su voz que no alcanza a esconder. Urgencia disfrazada de nostalgia." }
            }
        };
        return new DialogueConversation { id = "chapter1_npc_ben_motivo", nodes = new List<DialogueNode> { start } };
    }

    private static DialogueConversation BuildNpcMotivoConversation_Lisa()
    {
        DialogueNode start = new DialogueNode
        {
            id = "start",
            endsConversation = true,
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = "Lisa", text = "Lo conoc\u00ed en una exposici\u00f3n. Hace tres a\u00f1os, creo. \u00c9ramos amigos. Buenos amigos.", anxietyDelta = -5f },
                new DialogueLine { speaker = "Lisa", text = "\u00bfAlguien sabe qu\u00e9 pas\u00f3 exactamente? La noticia fue muy vaga. Muerte repentina, dicen. Pero eso no significa nada.", anxietyDelta = -3f },
                new DialogueLine { speaker = "Narrador", text = "Lo dice como quien ya tiene una teor\u00eda y busca que alguien la confirme sin saberlo." }
            }
        };
        return new DialogueConversation { id = "chapter1_npc_lisa_motivo", nodes = new List<DialogueNode> { start } };
    }

    private static DialogueConversation BuildNpcMotivoConversation_Lucas()
    {
        DialogueNode start = new DialogueNode
        {
            id = "start",
            endsConversation = true,
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = "Lucas", text = "Yo trabaj\u00e9 para \u00e9l. Ayudante de estudio, b\u00e1sicamente. Limpiaba pinceles, preparaba lienzos, a veces mezclaba pigmentos.", anxietyDelta = -5f },
                new DialogueLine { speaker = "Lucas", text = "Era muy exigente. Pero aprend\u00ed m\u00e1s con \u00e9l que en tres a\u00f1os de facultad. No s\u00e9 por qu\u00e9 vine. Supongo que quer\u00eda ver el lugar una \u00faltima vez.", anxietyDelta = -3f },
                new DialogueLine { speaker = "Narrador", text = "Lo dice mirando al suelo. Algo en su postura sugiere que hay mucho m\u00e1s detr\u00e1s de esas palabras." }
            }
        };
        return new DialogueConversation { id = "chapter1_npc_lucas_motivo", nodes = new List<DialogueNode> { start } };
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

    private static DialogueConversation BuildNpcCriticalConversation(string id, string npcName, string deliriousLine)
    {
        DialogueNode start = new DialogueNode
        {
            id = "start",
            endsConversation = true,
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = npcName, text = deliriousLine },
                new DialogueLine { speaker = npcName, text = "Respira... no puedo. No puedo bajar esto." },
                new DialogueLine { speaker = "Narrador", text = "Su ansiedad es total. No logras estabilizarlo." }
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
