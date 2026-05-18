using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Builder de contenido narrativo para el Capítulo 4: Sótano / Sala de Seguridad.
/// Inyecta conversaciones en la DialogueLibrary usando AddConversations (no destructivo).
///
/// FLUJO NARRATIVO (basado en flujoHistoria.txt):
/// 1. Jugador entra al sótano tras descubrir la trampilla bajo la alfombra y tener la llave.
/// 2. Encuentra la sala de seguridad con código 4-7-2-9.
/// 3. Hotspots: Mirilla central, Maletín negro (relicario Lucas), Cilindros, Nota roja, Mapa, Caja puzzle (llave de un solo uso).
/// 4. Sistema de entregas NPC: el jugador puede entregar objetos a sus dueños.
/// 5. Decisión: ir al Ala Norte a rescatar a Simón o quedarse.
/// </summary>
public class Chapter4Builder : MonoBehaviour
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
            Debug.LogWarning("Chapter4Builder no encontró DialogueLibrary.");
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
            return;
        }

        List<DialogueConversation> chapter4 = BuildAllConversations();
        targetLibrary.AddConversations(chapter4);
        Debug.Log("Chapter4Builder: Inyectadas conversaciones del Capítulo 4 (Sótano) exitosamente.");
    }

    private List<DialogueConversation> BuildAllConversations()
    {
        List<DialogueConversation> conversations = new List<DialogueConversation>();
        conversations.Add(BuildIntro());
        conversations.Add(BuildHotspotMirillaCentral());
        conversations.Add(BuildHotspotMaletinNegro());
        conversations.Add(BuildHotspotCilindros());
        conversations.Add(BuildHotspotNotaRoja());
        conversations.Add(BuildHotspotMapa());
        conversations.Add(BuildHotspotCajaPuzzleLlave());
        conversations.Add(BuildHotspotDiarioFinal());
        conversations.Add(BuildHotspotMaquinaria());
        conversations.Add(BuildNpcDeliveryPrompt());
        conversations.Add(BuildDeliverBen());
        conversations.Add(BuildDeliverLisa());
        conversations.Add(BuildDeliverRobert());
        conversations.Add(BuildDeliverAna());
        conversations.Add(BuildDeliverLucas());
        conversations.Add(BuildDeliverWrongItem());
        conversations.Add(BuildDecision());
        return conversations;
    }

    private DialogueConversation BuildIntro()
    {
        return new DialogueConversation
        {
            id = "chapter4_intro",
            nodes = new List<DialogueNode>
            {
                new DialogueNode
                {
                    id = "start",
                    endsConversation = true,
                    lines = new List<DialogueLine>
                    {
                        new DialogueLine { speaker = "Narrador", text = "Capítulo 4 — El Sótano. 8:00 PM." },
                        new DialogueLine { speaker = "Jugador", text = "Así que esta es la sala de seguridad... el código 4-7-2-9 funcionó. Está oscuro y hace mucho frío." },
                        new DialogueLine { speaker = "Narrador", text = "El aire está viciado. Aquí abajo Simón guardaba sus secretos más peligrosos. Debo ser cuidadoso.", anxietyDelta = 10f, setFlag = "chapter4.intro.seen" }
                    }
                }
            }
        };
    }

    private DialogueConversation BuildHotspotMirillaCentral()
    {
        return new DialogueConversation
        {
            id = "chapter4_hotspot_mirilla_central",
            nodes = new List<DialogueNode>
            {
                new DialogueNode
                {
                    id = "start",
                    endsConversation = true,
                    lines = new List<DialogueLine>
                    {
                        new DialogueLine { speaker = "Narrador", text = "Hay una mirilla en la pared. Parece dar a otra habitación... al Ala Norte." },
                        new DialogueLine { speaker = "Jugador", text = "(Mirando a través) Veo una figura humana sentada, inmóvil, en una habitación oscura. ¿Será...?" },
                        new DialogueLine { speaker = "Jugador", text = "Simón. Tiene que ser él. Está vivo... pero parece atado a una silla. Dios mío.", anxietyDelta = 15f, setFlag = "chapter4.mirilla.seen" },
                        new DialogueLine { speaker = "Narrador", text = "La silueta no se mueve. Pero respira. Hay que encontrar la forma de llegar hasta allí." }
                    }
                }
            }
        };
    }

    private DialogueConversation BuildHotspotMaletinNegro()
    {
        return new DialogueConversation
        {
            id = "chapter4_hotspot_maletin_negro",
            nodes = new List<DialogueNode>
            {
                new DialogueNode
                {
                    id = "start",
                    endsConversation = true,
                    lines = new List<DialogueLine>
                    {
                        new DialogueLine { speaker = "Narrador", text = "Un maletín negro de cuero, cerrado con un broche simple. Está debajo de la mesa de trabajo." },
                        new DialogueLine { speaker = "Jugador", text = "(Abriéndolo) Dentro hay un relicario de plata con una inscripción: 'Para Lucas. Siempre.'" },
                        new DialogueLine { speaker = "Jugador", text = "Este es el relicario que Lucas vino a buscar. Simón lo guardaba aquí abajo, protegido.", addInventoryItemId = "relicario_lucas", setFlag = "chapter4.maletin.opened" },
                        new DialogueLine { speaker = "Narrador", text = "El relicario brilla tenuemente bajo la luz del sótano. Lucas estará aliviado de recuperarlo." }
                    }
                }
            }
        };
    }

    private DialogueConversation BuildHotspotCilindros()
    {
        return new DialogueConversation
        {
            id = "chapter4_hotspot_cilindros",
            nodes = new List<DialogueNode>
            {
                new DialogueNode
                {
                    id = "start",
                    endsConversation = true,
                    lines = new List<DialogueLine>
                    {
                        new DialogueLine { speaker = "Narrador", text = "Hay cilindros de fonógrafo en un estante. Uno tiene una etiqueta: 'Testimonio final'." },
                        new DialogueLine { speaker = "Jugador", text = "(Reproduciendo) La voz de Simón: 'No voy a callar más. Aunque eso me cueste la vida.'" },
                        new DialogueLine { speaker = "Jugador", text = "Simón grabó su testimonio antes de que lo atraparan. Esto es una prueba.", setFlag = "chapter4.cilindros.heard" }
                    }
                }
            }
        };
    }

    private DialogueConversation BuildHotspotNotaRoja()
    {
        return new DialogueConversation
        {
            id = "chapter4_hotspot_nota_roja",
            nodes = new List<DialogueNode>
            {
                new DialogueNode
                {
                    id = "start",
                    endsConversation = true,
                    lines = new List<DialogueLine>
                    {
                        new DialogueLine { speaker = "Narrador", text = "Una nota escrita con tinta roja, pegada en la pared junto a la puerta norte." },
                        new DialogueLine { speaker = "Jugador", text = "(Leyendo) 'Si encuentras esto, ya saben que estás aquí. No uses las luces del pasillo norte.'" },
                        new DialogueLine { speaker = "Jugador", text = "Simón dejó esta advertencia. Quien lo retuvo vigila el Ala Norte.", anxietyDelta = 5f, setFlag = "chapter4.nota_roja.read" }
                    }
                }
            }
        };
    }

    private DialogueConversation BuildHotspotMapa()
    {
        return new DialogueConversation
        {
            id = "chapter4_hotspot_mapa",
            nodes = new List<DialogueNode>
            {
                new DialogueNode
                {
                    id = "start",
                    endsConversation = true,
                    lines = new List<DialogueLine>
                    {
                        new DialogueLine { speaker = "Narrador", text = "Un mapa del Ala Norte clavado en la pared. Tiene marcas recientes." },
                        new DialogueLine { speaker = "Jugador", text = "Hay una X roja en la segunda puerta del pasillo. 'Aquí está.' Alguien marcó exactamente dónde retienen a Simón." },
                        new DialogueLine { speaker = "Jugador", text = "La segunda puerta. Debo recordarlo. Si entro por la puerta equivocada...", setFlag = "chapter4.mapa.seen" }
                    }
                }
            }
        };
    }

    private DialogueConversation BuildHotspotCajaPuzzleLlave()
    {
        return new DialogueConversation
        {
            id = "chapter4_hotspot_caja_puzzle_llave",
            nodes = new List<DialogueNode>
            {
                new DialogueNode
                {
                    id = "start",
                    endsConversation = true,
                    lines = new List<DialogueLine>
                    {
                        new DialogueLine { speaker = "Narrador", text = "Una caja de madera con un mecanismo de engranajes. Parece un puzzle." },
                        new DialogueLine { speaker = "Jugador", text = "(Resolviendo el mecanismo) Los engranajes encajan... la caja se abre." },
                        new DialogueLine { speaker = "Jugador", text = "Dentro hay una llave vieja y desgastada. El metal está tan corroído que parece que solo resistirá un giro más.", addInventoryItemId = "SingleUseKey", setFlag = "chapter4.single_use_key.found" },
                        new DialogueLine { speaker = "Narrador", text = "Esta llave abre una de las puertas del Ala Norte. Pero solo una. Debo elegir bien dónde usarla." }
                    }
                }
            }
        };
    }

    private DialogueConversation BuildHotspotDiarioFinal()
    {
        return new DialogueConversation
        {
            id = "chapter4_hotspot_diario_final",
            nodes = new List<DialogueNode>
            {
                new DialogueNode
                {
                    id = "start",
                    endsConversation = true,
                    lines = new List<DialogueLine>
                    {
                        new DialogueLine { speaker = "Narrador", text = "Hay un diario sobre esta vieja mesa. La cubierta está desgastada... 'Diario de Investigación'." },
                        new DialogueLine { speaker = "Jugador", text = "(Leyendo) 'He logrado aislar el compuesto, pero los efectos secundarios son devastadores. No puedo permitir que salga a la luz.'" },
                        new DialogueLine { speaker = "Jugador", text = "Simón estaba investigando algo peligroso... por eso alguien lo retuvo aquí.", addInventoryItemId = "diario_final", setFlag = "chapter4.diario.found" }
                    }
                }
            }
        };
    }

    private DialogueConversation BuildHotspotMaquinaria()
    {
        return new DialogueConversation
        {
            id = "chapter4_hotspot_maquinaria",
            nodes = new List<DialogueNode>
            {
                new DialogueNode
                {
                    id = "start",
                    endsConversation = true,
                    lines = new List<DialogueLine>
                    {
                        new DialogueLine { speaker = "Narrador", text = "Es una especie de centrífuga o generador... Está apagada, pero aún emite un leve zumbido." },
                        new DialogueLine { speaker = "Jugador", text = "Hay tubos de ensayo rotos alrededor. Simón tuvo prisa en abandonar este lugar. No debo tocar nada sin saber qué es.", anxietyDelta = 5f }
                    }
                }
            }
        };
    }

    private DialogueConversation BuildNpcDeliveryPrompt()
    {
        return new DialogueConversation
        {
            id = "chapter4_npc_delivery_prompt",
            nodes = new List<DialogueNode>
            {
                new DialogueNode
                {
                    id = "start",
                    lines = new List<DialogueLine>
                    {
                        new DialogueLine { speaker = "Jugador", text = "Creo que ya encontré varias de las cosas que estaban buscando... quizá debería hablar con ellos sobre esto." }
                    },
                    choices = new List<DialogueChoice>
                    {
                        new DialogueChoice { id = "choice_deliver_yes", text = "Sí, hablar con ellos", nextNodeId = "deliver_yes", setFlag = "chapter4.npc_delivery.activated" },
                        new DialogueChoice { id = "choice_deliver_no", text = "No todavía", nextNodeId = "deliver_no" }
                    }
                },
                new DialogueNode
                {
                    id = "deliver_yes",
                    endsConversation = true,
                    lines = new List<DialogueLine>
                    {
                        new DialogueLine { speaker = "Narrador", text = "Ahora puedes seleccionar un objeto del inventario y entregárselo al NPC correspondiente usando el botón de interacción.", setFlag = "NpcDeliveryMode" }
                    }
                },
                new DialogueNode
                {
                    id = "deliver_no",
                    endsConversation = true,
                    lines = new List<DialogueLine>
                    {
                        new DialogueLine { speaker = "Jugador", text = "Mejor sigo explorando un poco más. Aún hay cosas que investigar." }
                    }
                }
            }
        };
    }

    private DialogueConversation BuildDeliverBen()
    {
        return new DialogueConversation
        {
            id = "chapter4_deliver_ben_libro_contabilidad",
            nodes = new List<DialogueNode>
            {
                new DialogueNode
                {
                    id = "start",
                    lines = new List<DialogueLine>
                    {
                        new DialogueLine { speaker = "Ben", text = "¿Eso es...? El libro de contabilidad. Dios mío, pensé que nunca lo volvería a ver." },
                        new DialogueLine { speaker = "Jugador", text = "Lo encontré en el estudio de Simón. Sé lo que contiene, Ben." },
                        new DialogueLine { speaker = "Ben", text = "Mira... cometí errores. Pero puedo arreglarlo si me das la oportunidad." }
                    },
                    choices = new List<DialogueChoice>
                    {
                        new DialogueChoice { id = "choice_give", text = "Entregar el libro", nextNodeId = "give", setFlag = "npc.delivery.ben.completed" },
                        new DialogueChoice { id = "choice_keep", text = "No, entregar después", nextNodeId = "keep", setFlag = "PlayerRefusedNpc.ben" }
                    }
                },
                new DialogueNode
                {
                    id = "give",
                    endsConversation = true,
                    lines = new List<DialogueLine>
                    {
                        new DialogueLine { speaker = "Narrador", text = "Has entregado el libro de contabilidad a Ben. Su ansiedad disminuye notablemente.", removeInventoryItemId = "libro_contabilidad", setFlag = "ending.ben.item_delivered" }
                    }
                },
                new DialogueNode
                {
                    id = "keep",
                    endsConversation = true,
                    lines = new List<DialogueLine>
                    {
                        new DialogueLine { speaker = "Ben", text = "Entiendo... supongo que no confías en mí. No te culpo." }
                    }
                }
            }
        };
    }

    private DialogueConversation BuildDeliverLisa()
    {
        return new DialogueConversation
        {
            id = "chapter4_deliver_lisa_carpeta_evidencia",
            nodes = new List<DialogueNode>
            {
                new DialogueNode
                {
                    id = "start",
                    lines = new List<DialogueLine>
                    {
                        new DialogueLine { speaker = "Lisa", text = "¿Esa carpeta...? Son las fotografías y documentos del incendio del puerto. ¿Dónde las encontraste?" },
                        new DialogueLine { speaker = "Jugador", text = "Estaban debajo de la cama de Simón. Él las guardaba como prueba." },
                        new DialogueLine { speaker = "Lisa", text = "Con esto puedo publicar la investigación completa. La verdad tiene que salir a la luz." }
                    },
                    choices = new List<DialogueChoice>
                    {
                        new DialogueChoice { id = "choice_give", text = "Entregar la carpeta", nextNodeId = "give", setFlag = "npc.delivery.lisa.completed" },
                        new DialogueChoice { id = "choice_keep", text = "No, entregar después", nextNodeId = "keep", setFlag = "PlayerRefusedNpc.lisa" }
                    }
                },
                new DialogueNode
                {
                    id = "give",
                    endsConversation = true,
                    lines = new List<DialogueLine>
                    {
                        new DialogueLine { speaker = "Narrador", text = "Has entregado la carpeta con evidencia a Lisa. Su determinación se renueva.", removeInventoryItemId = "carpeta_evidencia", setFlag = "ending.lisa.item_delivered" }
                    }
                },
                new DialogueNode
                {
                    id = "keep",
                    endsConversation = true,
                    lines = new List<DialogueLine>
                    {
                        new DialogueLine { speaker = "Lisa", text = "Está bien. Pero esas pruebas son importantes. No las pierdas." }
                    }
                }
            }
        };
    }

    private DialogueConversation BuildDeliverRobert()
    {
        return new DialogueConversation
        {
            id = "chapter4_deliver_robert_carta_padre",
            nodes = new List<DialogueNode>
            {
                new DialogueNode
                {
                    id = "start",
                    lines = new List<DialogueLine>
                    {
                        new DialogueLine { speaker = "Robert", text = "Esa carta... la reconozco. Es la letra de nuestro padre." },
                        new DialogueLine { speaker = "Jugador", text = "La encontré en un compartimento oculto detrás de una pintura en la galería." },
                        new DialogueLine { speaker = "Robert", text = "Simón y yo... somos hermanos. Nunca se lo dije a nadie. Esta carta lo prueba." }
                    },
                    choices = new List<DialogueChoice>
                    {
                        new DialogueChoice { id = "choice_give", text = "Entregar la carta", nextNodeId = "give", setFlag = "npc.delivery.robert.completed" },
                        new DialogueChoice { id = "choice_keep", text = "No, entregar después", nextNodeId = "keep", setFlag = "PlayerRefusedNpc.robert" }
                    }
                },
                new DialogueNode
                {
                    id = "give",
                    endsConversation = true,
                    lines = new List<DialogueLine>
                    {
                        new DialogueLine { speaker = "Narrador", text = "Has entregado la carta del padre a Robert. Sus ojos se humedecen.", removeInventoryItemId = "carta_padre", setFlag = "ending.robert.item_delivered" }
                    }
                },
                new DialogueNode
                {
                    id = "keep",
                    endsConversation = true,
                    lines = new List<DialogueLine>
                    {
                        new DialogueLine { speaker = "Robert", text = "Entiendo. Quizás no es el momento. Pero esa carta... significa mucho para mí." }
                    }
                }
            }
        };
    }

    private DialogueConversation BuildDeliverAna()
    {
        return new DialogueConversation
        {
            id = "chapter4_deliver_ana_estuche_joyas",
            nodes = new List<DialogueNode>
            {
                new DialogueNode
                {
                    id = "start",
                    lines = new List<DialogueLine>
                    {
                        new DialogueLine { speaker = "Ana", text = "¿Es ese...? El estuche de joyas familiares. Pensé que se habían perdido para siempre." },
                        new DialogueLine { speaker = "Jugador", text = "Estaba en el archivador de la galería, cerrado con la llave pequeña." },
                        new DialogueLine { speaker = "Ana", text = "Las usé como garantía sin permiso de Simón. Fue un error terrible. Necesito devolverlas." }
                    },
                    choices = new List<DialogueChoice>
                    {
                        new DialogueChoice { id = "choice_give", text = "Entregar el estuche", nextNodeId = "give", setFlag = "npc.delivery.ana.completed" },
                        new DialogueChoice { id = "choice_keep", text = "No, entregar después", nextNodeId = "keep", setFlag = "PlayerRefusedNpc.ana" }
                    }
                },
                new DialogueNode
                {
                    id = "give",
                    endsConversation = true,
                    lines = new List<DialogueLine>
                    {
                        new DialogueLine { speaker = "Narrador", text = "Has entregado el estuche de joyas a Ana. Suspira con alivio.", removeInventoryItemId = "estuche_joyas", setFlag = "ending.ana.item_delivered" }
                    }
                },
                new DialogueNode
                {
                    id = "keep",
                    endsConversation = true,
                    lines = new List<DialogueLine>
                    {
                        new DialogueLine { speaker = "Ana", text = "Está bien... pero por favor, no las pierdas. Son lo único que queda de esa familia." }
                    }
                }
            }
        };
    }

    private DialogueConversation BuildDeliverLucas()
    {
        return new DialogueConversation
        {
            id = "chapter4_deliver_lucas_relicario",
            nodes = new List<DialogueNode>
            {
                new DialogueNode
                {
                    id = "start",
                    lines = new List<DialogueLine>
                    {
                        new DialogueLine { speaker = "Lucas", text = "El relicario... 'Para Lucas. Siempre.' Simón me lo dio cuando era su ayudante." },
                        new DialogueLine { speaker = "Jugador", text = "Lo encontré en un maletín negro en la sala de seguridad del sótano." },
                        new DialogueLine { speaker = "Lucas", text = "Lo tomé sin permiso cuando me fui. Me arrepentí cada día. Necesitaba recuperarlo para devolverlo." }
                    },
                    choices = new List<DialogueChoice>
                    {
                        new DialogueChoice { id = "choice_give", text = "Entregar el relicario", nextNodeId = "give", setFlag = "npc.delivery.lucas.completed" },
                        new DialogueChoice { id = "choice_keep", text = "No, entregar después", nextNodeId = "keep", setFlag = "PlayerRefusedNpc.lucas" }
                    }
                },
                new DialogueNode
                {
                    id = "give",
                    endsConversation = true,
                    lines = new List<DialogueLine>
                    {
                        new DialogueLine { speaker = "Narrador", text = "Has entregado el relicario a Lucas. Lo aprieta contra su pecho.", removeInventoryItemId = "relicario_lucas", setFlag = "ending.lucas.item_delivered" }
                    }
                },
                new DialogueNode
                {
                    id = "keep",
                    endsConversation = true,
                    lines = new List<DialogueLine>
                    {
                        new DialogueLine { speaker = "Lucas", text = "Lo entiendo. Quizás no me lo merezco todavía." }
                    }
                }
            }
        };
    }

    private DialogueConversation BuildDeliverWrongItem()
    {
        return new DialogueConversation
        {
            id = "chapter4_deliver_wrong_item",
            nodes = new List<DialogueNode>
            {
                new DialogueNode
                {
                    id = "start",
                    endsConversation = true,
                    lines = new List<DialogueLine>
                    {
                        new DialogueLine { speaker = "Narrador", text = "El personaje mira el objeto con confusión." },
                        new DialogueLine { speaker = "NPC", text = "No creo que eso tenga relación conmigo... ¿Estás seguro de que es para mí?" }
                    }
                }
            }
        };
    }

    private DialogueConversation BuildDecision()
    {
        return new DialogueConversation
        {
            id = "chapter4_decision",
            nodes = new List<DialogueNode>
            {
                new DialogueNode
                {
                    id = "start",
                    lines = new List<DialogueLine>
                    {
                        new DialogueLine { speaker = "Jugador", text = "He visto a Simón por la mirilla. Está vivo pero retenido en el Ala Norte." },
                        new DialogueLine { speaker = "Jugador", text = "Tengo la llave desgastada. Solo servirá para una puerta. Debo decidir bien." }
                    },
                    choices = new List<DialogueChoice>
                    {
                        new DialogueChoice { id = "choice_rescue", text = "Ir al Ala Norte a rescatar a Simón", nextNodeId = "end_rescue", setFlag = "chapter4.decision.rescue_simon" },
                        new DialogueChoice { id = "choice_stay", text = "Quedarme con el grupo por ahora", nextNodeId = "end_stay", setFlag = "chapter4.decision.stay_group" }
                    }
                },
                new DialogueNode
                {
                    id = "end_rescue",
                    endsConversation = true,
                    lines = new List<DialogueLine>
                    {
                        new DialogueLine { speaker = "Jugador", text = "No puedo dejarlo ahí. Voy a sacarlo de esa habitación." },
                        new DialogueLine { speaker = "Narrador", text = "Has decidido rescatar a Simón. El Ala Norte te espera.", setFlag = "chapter4.complete" }
                    }
                },
                new DialogueNode
                {
                    id = "end_stay",
                    endsConversation = true,
                    lines = new List<DialogueLine>
                    {
                        new DialogueLine { speaker = "Jugador", text = "Es demasiado peligroso ir solo. Necesito pensar en otra forma." },
                        new DialogueLine { speaker = "Narrador", text = "Has decidido quedarte. Simón permanece cautivo.", setFlag = "chapter4.complete" }
                    }
                }
            }
        };
    }
}
