using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Builder de contenido narrativo para el Capítulo 4: Sótano / Ala Norte.
/// Inyecta conversaciones en la DialogueLibrary usando AddConversations (no destructivo).
///
/// FLUJO NARRATIVO:
/// 1. Jugador entra al sótano tras descubrir la puerta bajo la alfombra y tener la llave.
/// 2. El ambiente es lúgubre, la música debería reflejar misterio y tensión.
/// 3. Encuentra el Diario Final de Simón y descubre el "Proyecto".
/// 4. Solo, el protagonista debe decidir qué hacer con este descubrimiento.
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

        List<DialogueConversation> chapter4 = new List<DialogueConversation>
        {
            // --- INTRODUCCIÓN AL CAPÍTULO 4 (SÓTANO) ---
            new DialogueConversation
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
                            new DialogueLine { speaker = "Jugador", text = "Así que este es el famoso sótano... el Ala Norte de la que hablaba la carta de Simón. Está oscuro y hace mucho frío." },
                            new DialogueLine { speaker = "Narrador", text = "El aire está viciado. Aquí abajo se esconde su secreto. Debo ser cuidadoso, nadie sabe que estoy aquí.", anxietyDelta = 10f, setFlag = "chapter4.intro.seen" }
                        }
                    }
                }
            },

            // --- HOTSPOTS DEL SÓTANO ---
            new DialogueConversation
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
                            new DialogueLine { speaker = "Jugador", text = "Simón estaba desarrollando algo peligroso... por eso fingió su muerte o se escondió.", addInventoryItemId = "diario_final" }
                        }
                    }
                }
            },

            new DialogueConversation
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
            },

            // --- DECISIÓN FINAL / CULMINACIÓN DEL CAPÍTULO 4 ---
            new DialogueConversation
            {
                id = "chapter4_decision",
                nodes = new List<DialogueNode>
                {
                    new DialogueNode
                    {
                        id = "start",
                        lines = new List<DialogueLine>
                        {
                            new DialogueLine { speaker = "Jugador", text = "Tengo el diario y he visto la maquinaria. Sé lo que estaba haciendo Simón." },
                            new DialogueLine { speaker = "Jugador", text = "¿Debería destruir todo esto para proteger su secreto, o llevar las pruebas a la policía?" }
                        },
                        choices = new List<DialogueChoice>
                        {
                            new DialogueChoice { id = "choice_destruir", text = "Destruir el laboratorio (Proteger el secreto)", nextNodeId = "end_destruir", setFlag = "chap4_final_destruir_laboratorio" },
                            new DialogueChoice { id = "choice_policia", text = "Llevar pruebas a la policía (Revelar la verdad)", nextNodeId = "end_policia", setFlag = "chap4_final_revelar_policia" }
                        }
                    },
                    new DialogueNode
                    {
                        id = "end_destruir",
                        endsConversation = true,
                        lines = new List<DialogueLine>
                        {
                            new DialogueLine { speaker = "Jugador", text = "Simón sabía que esto era peligroso. Si lo destruyo, nadie más correrá riesgo. Es lo correcto." },
                            new DialogueLine { speaker = "Narrador", text = "Has decidido proteger el secreto." },
                            new DialogueLine { speaker = "Jugador", text = "La decisión está tomada. Es hora de salir de esta mansión." }
                        }
                    },
                    new DialogueNode
                    {
                        id = "end_policia",
                        endsConversation = true,
                        lines = new List<DialogueLine>
                        {
                            new DialogueLine { speaker = "Jugador", text = "No puedo ocultar algo tan grande. Las autoridades deben saber qué ocurrió aquí realmente." },
                            new DialogueLine { speaker = "Narrador", text = "Has decidido revelar la verdad." },
                            new DialogueLine { speaker = "Jugador", text = "La decisión está tomada. Es hora de salir de esta mansión." }
                        }
                    }
                }
            }
        };

        // Inyectamos las conversaciones en la biblioteca general sin borrar lo anterior
        targetLibrary.AddConversations(chapter4);
        Debug.Log("Chapter4Builder: Inyectadas conversaciones del Capítulo 4 (Sótano) exitosamente.");
    }
}
