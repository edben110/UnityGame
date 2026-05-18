using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Builder de conversaciones del epílogo.
/// Genera los diálogos de los 3 finales basados en flujoHistoria.txt.
/// </summary>
public class EpilogueBuilder : MonoBehaviour
{
    [SerializeField] private DialogueLibrary targetLibrary;
    [SerializeField] private bool generateOnStartIfEmpty = true;

    private void Awake()
    {
        if (!generateOnStartIfEmpty) return;

        if (targetLibrary == null)
            targetLibrary = GetComponent<DialogueLibrary>();

        if (targetLibrary == null)
        {
            Debug.LogWarning("EpilogueBuilder no encontró DialogueLibrary.");
            return;
        }

        EnsureData();
    }

    public void EnsureData()
    {
        if (targetLibrary == null)
            targetLibrary = GetComponent<DialogueLibrary>();
        if (targetLibrary == null) return;

        List<DialogueConversation> epilogues = new List<DialogueConversation>();
        epilogues.Add(BuildFinalA());
        epilogues.Add(BuildFinalB());
        epilogues.Add(BuildFinalC());
        epilogues.Add(BuildChapter5Intro());
        epilogues.Add(BuildSimonRescue());
        epilogues.Add(BuildDoorChoiceEmpty());
        epilogues.Add(BuildDoorChoiceKiller());

        targetLibrary.AddConversations(epilogues);
        Debug.Log("EpilogueBuilder: Conversaciones de epílogo inyectadas.");
    }

    private DialogueConversation BuildFinalA()
    {
        return new DialogueConversation
        {
            id = "epilogue_final_a",
            nodes = new List<DialogueNode>
            {
                new DialogueNode
                {
                    id = "start",
                    endsConversation = true,
                    lines = new List<DialogueLine>
                    {
                        new DialogueLine { speaker = "Narrador", text = "FINAL A — Todos sobreviven." },
                        new DialogueLine { speaker = "Narrador", text = "El grupo sale unido de la mansión. Simón es liberado. La policía llega al día siguiente tras la denuncia del grupo." },
                        new DialogueLine { speaker = "Narrador", text = "La investigación revela que la sexta persona era un antiguo socio de Simón que orquestó todo para recuperar documentos comprometedores. Fue arrestado semanas después en la frontera con Austria." },
                        new DialogueLine { speaker = "Narrador", text = "Ben devuelve el dinero y evita cargos. Lisa publica la investigación. Robert reconoce su parentesco con Simón. Ana devuelve las joyas. Lucas devuelve el relicario y Simón lo perdona." },
                        new DialogueLine { speaker = "Narrador", text = "Keller cierra el caso. La mansión es vendida." },
                        new DialogueLine { speaker = "Narrador", text = "\"A veces la verdad no mata. A veces salva.\"", setFlag = "game.complete" }
                    }
                }
            }
        };
    }

    private DialogueConversation BuildFinalB()
    {
        return new DialogueConversation
        {
            id = "epilogue_final_b",
            nodes = new List<DialogueNode>
            {
                new DialogueNode
                {
                    id = "start",
                    endsConversation = true,
                    lines = new List<DialogueLine>
                    {
                        new DialogueLine { speaker = "Narrador", text = "FINAL B — No todos sobreviven." },
                        new DialogueLine { speaker = "Narrador", text = "Al menos uno de los NPCs abandonó la sala durante la historia. Sus cuerpos son encontrados en distintas habitaciones de la mansión." },
                        new DialogueLine { speaker = "Narrador", text = "El grupo restante sale, pero el trauma es irreversible. La policía investiga las muertes." },
                        new DialogueLine { speaker = "Narrador", text = "La causa oficial: 'crisis nerviosa seguida de accidente.' Pero Keller sabe que no fue accidente — fue el aislamiento. La mansión amplifica el miedo de quien está solo." },
                        new DialogueLine { speaker = "Narrador", text = "Los sobrevivientes cargan con la culpa de no haber hablado más, de no haber prestado atención a las señales." },
                        new DialogueLine { speaker = "Narrador", text = "\"El silencio mata más lento que un cuchillo. Pero mata igual.\"", setFlag = "game.complete" }
                    }
                }
            }
        };
    }

    private DialogueConversation BuildFinalC()
    {
        return new DialogueConversation
        {
            id = "epilogue_final_c",
            nodes = new List<DialogueNode>
            {
                new DialogueNode
                {
                    id = "start",
                    endsConversation = true,
                    lines = new List<DialogueLine>
                    {
                        new DialogueLine { speaker = "Narrador", text = "FINAL C — El protagonista es el culpable." },
                        new DialogueLine { speaker = "Narrador", text = "Revelación: Franks Keller no es un detective. Es la sexta persona. El hombre de la foto con cinta negra." },
                        new DialogueLine { speaker = "Narrador", text = "Keller provocó el incendio del almacén del puerto. Simón presenció el crimen. Keller necesitaba recuperar la evidencia y eliminar testigos." },
                        new DialogueLine { speaker = "Simón", text = "Sabía que eras tú. Desde el principio." },
                        new DialogueLine { speaker = "Keller", text = "Entonces sabes por qué estoy aquí." },
                        new DialogueLine { speaker = "Simón", text = "La evidencia ya no importa. Hiciste copias de todo." },
                        new DialogueLine { speaker = "Keller", text = "No vine por la evidencia. Vine porque no puedo dejar testigos." },
                        new DialogueLine { speaker = "Narrador", text = "Keller deja a Simón atado. Sale de la mansión. Quema la evidencia. La policía encuentra 6 cuerpos días después. Caso archivado." },
                        new DialogueLine { speaker = "Narrador", text = "\"La mansión guarda sus secretos. Y los de quien la visita.\"", setFlag = "game.complete" }
                    }
                }
            }
        };
    }

    private DialogueConversation BuildChapter5Intro()
    {
        return new DialogueConversation
        {
            id = "chapter5_intro",
            nodes = new List<DialogueNode>
            {
                new DialogueNode
                {
                    id = "start",
                    lines = new List<DialogueLine>
                    {
                        new DialogueLine { speaker = "Narrador", text = "Capítulo 5 — La Noche. 10:00 PM." },
                        new DialogueLine { speaker = "Narrador", text = "El Ala Norte. Un pasillo largo y oscuro. Tres puertas idénticas." },
                        new DialogueLine { speaker = "Jugador", text = "La nota decía que no usara las luces. Tengo la llave desgastada... solo servirá una vez." },
                        new DialogueLine { speaker = "Jugador", text = "El mapa marcaba la segunda puerta. Pero... ¿y si es una trampa?" }
                    },
                    choices = new List<DialogueChoice>
                    {
                        new DialogueChoice { id = "door_1", text = "Primera puerta (la más cercana)", nextNodeId = "chose_door_1", setFlag = "chapter5.chose.door_empty" },
                        new DialogueChoice { id = "door_2", text = "Segunda puerta (la del mapa)", nextNodeId = "chose_door_2", setFlag = "chapter5.chose.door_simon" },
                        new DialogueChoice { id = "door_3", text = "Tercera puerta (al fondo)", nextNodeId = "chose_door_3", setFlag = "chapter5.chose.door_killer" }
                    }
                },
                new DialogueNode
                {
                    id = "chose_door_1",
                    endsConversation = true,
                    lines = new List<DialogueLine>
                    {
                        new DialogueLine { speaker = "Narrador", text = "Usas la llave en la primera puerta. El mecanismo gira con dificultad... y la llave se rompe.", setFlag = "OneTimeKeyUsed" },
                        new DialogueLine { speaker = "Jugador", text = "Está vacía. Solo telarañas y polvo. He perdido la llave para nada." },
                        new DialogueLine { speaker = "Narrador", text = "Sin la llave, no puedes abrir las otras puertas. Simón permanece cautivo.", setFlag = "chapter5.door_wasted" }
                    }
                },
                new DialogueNode
                {
                    id = "chose_door_2",
                    endsConversation = true,
                    lines = new List<DialogueLine>
                    {
                        new DialogueLine { speaker = "Narrador", text = "Usas la llave en la segunda puerta. El mecanismo gira... click.", setFlag = "OneTimeKeyUsed" },
                        new DialogueLine { speaker = "Narrador", text = "La puerta se abre. Dentro, una figura atada a una silla.", setFlag = "chapter5.found.simon" }
                    }
                },
                new DialogueNode
                {
                    id = "chose_door_3",
                    endsConversation = true,
                    lines = new List<DialogueLine>
                    {
                        new DialogueLine { speaker = "Narrador", text = "Usas la llave en la tercera puerta. El mecanismo gira con dificultad... y la llave se rompe.", setFlag = "OneTimeKeyUsed" },
                        new DialogueLine { speaker = "Jugador", text = "Una habitación con una cama y un ventanal grande. Parece una sala de operaciones improvisada." },
                        new DialogueLine { speaker = "Jugador", text = "Alguien ha estado viviendo aquí. El asesino. Pero no está ahora.", setFlag = "chapter5.found.killer_room" }
                    }
                }
            }
        };
    }

    private DialogueConversation BuildSimonRescue()
    {
        return new DialogueConversation
        {
            id = "chapter5_simon_rescue",
            nodes = new List<DialogueNode>
            {
                new DialogueNode
                {
                    id = "start",
                    endsConversation = true,
                    lines = new List<DialogueLine>
                    {
                        new DialogueLine { speaker = "Simón", text = "...agua." },
                        new DialogueLine { speaker = "Jugador", text = "Simón. Estás vivo. Te voy a sacar de aquí." },
                        new DialogueLine { speaker = "Simón", text = "Sabía... que alguien vendría. ¿Los demás...?" },
                        new DialogueLine { speaker = "Narrador", text = "Simón está débil pero consciente. Lo desatas con cuidado.", setFlag = "chapter5.simon.rescued" },
                        new DialogueLine { speaker = "Jugador", text = "Vamos. Hay que salir de esta mansión." }
                    }
                }
            }
        };
    }

    private DialogueConversation BuildDoorChoiceEmpty()
    {
        return new DialogueConversation
        {
            id = "chapter5_empty_room",
            nodes = new List<DialogueNode>
            {
                new DialogueNode
                {
                    id = "start",
                    endsConversation = true,
                    lines = new List<DialogueLine>
                    {
                        new DialogueLine { speaker = "Narrador", text = "La habitación está completamente vacía. Telarañas cubren las esquinas." },
                        new DialogueLine { speaker = "Jugador", text = "Nada. He desperdiciado la única oportunidad. La llave se rompió." },
                        new DialogueLine { speaker = "Narrador", text = "Sin forma de abrir las otras puertas, debes volver con el grupo. Simón permanece cautivo." }
                    }
                }
            }
        };
    }

    private DialogueConversation BuildDoorChoiceKiller()
    {
        return new DialogueConversation
        {
            id = "chapter5_killer_bunker",
            nodes = new List<DialogueNode>
            {
                new DialogueNode
                {
                    id = "start",
                    endsConversation = true,
                    lines = new List<DialogueLine>
                    {
                        new DialogueLine { speaker = "Narrador", text = "Una cama deshecha, instrumental médico, y un ventanal que da al jardín trasero." },
                        new DialogueLine { speaker = "Jugador", text = "Alguien ha estado viviendo aquí. Hay restos de comida reciente y notas con nombres... los nuestros." },
                        new DialogueLine { speaker = "Jugador", text = "Este es el escondite del secuestrador. Pero no está aquí ahora. La llave se rompió... no puedo abrir más puertas.", setFlag = "chapter5.found.killer_evidence" }
                    }
                }
            }
        };
    }
}
