# Refactorización del Sistema Narrativo - Resumen de Cambios

## 📋 Resumen General

Se ha completado una refactorización comprehensiva del flujo narrativo del Capítulo 2 y 3, priorizando coherencia espacial, lógica narrativa robusta y validaciones centralizadas. Todos los cambios mantienen coherencia con el script Python original (`la_mansion_de_simon.py`).

---

## ✅ Cambios Realizados

### 1. **Creación del Sistema Centralizado de Validación de Contexto Narrativo**

**Archivo:** `Assets/Scripts/State/RoomContextValidator.cs` (NUEVO)

**Responsabilidades:**
- Validar si el jugador está en la misma habitación que un NPC
- Validar si pueden ocurrir diálogos de NPC basándose en la ubicación
- Validar si pueden ocurrir decisiones grupales
- Centralizar toda lógica de restricción espacial

**Métodos principales:**
- `CanNpcDialogue(npcId, chapterId)` - Retorna TRUE solo si el jugador está en la sala de NPCs
- `CanGroupDecision(chapterId)` - Retorna TRUE solo si el jugador está en la sala de NPCs
- `IsPlayerInNpcRoom()` - Verifica ubicación actual del jugador

**Validaciones implementadas:**
```
CanNpcDialogue = PlayerInNpcRoom AND NpcInCurrentRoom AND ValidChapter
CanGroupDecision = PlayerInNpcRoom AND ValidChapter
```

---

### 2. **Refactorización del Panel de Decisiones del Capítulo 2**

**Archivo:** `Assets/Scripts/Narrative/Chapter2Builder.cs`

**Cambio: De UNA decisión a DOS decisiones secuenciales**

#### Decisión 1: PRIMERA DECISIÓN (cuando se habla con los 5 NPCs)
- **ID:** `chapter2_initial_decision`
- **Trigger:** Después de hablar con los 5 NPCs en la sala de NPCs
- **Validación:** `GetNpcTalkCount() >= 5` AND `IsPlayerInNpcRoom()`
- **Opciones:**
  1. "Hablar sinceramente sobre Simon" → flag: `chapter2.choice.talk_sincere`
  2. "Separarse" → flag: `chapter2.choice.separate`

#### Decisión 2: SEGUNDA DECISIÓN (después de obtener el libro)
- **ID:** `chapter2_book_decision`
- **Trigger:** Jugador interactúa con Ben habiendo obtenido el libro de contabilidad
- **Validación:**
  ```
  HasAccountingBook == TRUE
  AND PlayerInNpcRoom == TRUE
  AND InteractingWithBen == TRUE
  ```
- **Opciones:**
  1. "Confrontar" → flag: `chapter2.choice.confront_ben`
  2. "Subir a buscar la habitación de Simon" → flag: `chapter2.choice.search_bedroom`

---

### 3. **Nuevo Flujo de Interacción del Libro de Contabilidad**

**Archivos modificados:**
- `Assets/Scripts/Map/NpcInteractable.cs`
- `Assets/Scripts/State/ChapterFlowController.cs`

**Cambios:**

1. **En NpcInteractable.cs:**
   - Agregada validación de habitación: `RoomContextValidator.CanNpcDialogue()` antes de permitir diálogos
   - Agregado caso especial para Ben en Cap 2: Si tiene el libro de contabilidad, dispara directamente `chapter2_book_decision`
   - Habilitada la restricción espacial que estaba comentada

2. **En ChapterFlowController.cs:**
   - Separados los IDs de conversación en dos: `chapter2InitialDecisionConversationId` y `chapter2BookDecisionConversationId`
   - Agregado `LaunchChapter2InitialDecision()` que se dispara cuando se han hablado con los 5 NPCs
   - Actualizada lógica de `CheckAutoDecisions()` para disparar la primera decisión basándose en `GetNpcTalkCount() >= 5`
   - Actualizado `OnConversationEnded()` para manejar ambas decisiones del Cap 2

---

### 4. **Re-habilitación de Validación Espacial**

**Archivo:** `Assets/Scripts/Map/NpcInteractable.cs`

**Cambio:** Se re-habilitó la validación de habitación que estaba comentada con "TODO"

**Código anterior:**
```csharp
// Restricción espacial deshabilitada temporalmente
// TODO: Reimplementar cuando el sistema de salas esté estable
// Los NPCs solo deberían responder en la sala donde están físicamente
```

**Código nuevo:**
```csharp
// VALIDACIÓN ESPACIAL: El jugador debe estar en la misma habitación que el NPC
if (RoomContextValidator.Instance != null)
{
    if (!RoomContextValidator.Instance.CanNpcDialogue(npcId, StoryState.Instance.CurrentChapterId))
    {
        Debug.LogWarning($"[NPC] {npcId} no puede dialogar: jugador no está en la habitación correcta.");
        return false;
    }
}
```

---

### 5. **Refactorización del Capítulo 3**

**Archivo:** `Assets/Scripts/Narrative/Chapter3Builder.cs`

**Cambios:**

1. **Decisión mejorada con mejor narrativa:**
   - Las opciones ahora tienen contexto más rico y feedback narrativo mejorado
   - Cambio de flag `chapter3.completed` a flags más específicos: `chapter3.archivador_abierto`, `chapter3.ala_norte_explorada`, `chapter3.group_confronted`

2. **Mejoras narrativas:**
   - Mejor descripción del flujo de cada opción
   - Feedback más contextual cuando se elige cada ruta
   - La narrativa ahora refleja que el jugador investiga solo

---

### 6. **Eliminación de Lógica de Acompañantes**

**Búsqueda realizada:** Se confirmó que no hay lógica explícita de "companions", "followers", o "party" en el código.

**Cambios realizados:**
- Todos los diálogos de NPCs ahora están restringidos por habitación (validación centralizada en `RoomContextValidator`)
- Los NPCs solo pueden dialogar cuando están físicamente en la misma habitación que el jugador
- No hay cambios de NPC "siguiendo" al jugador entre habitaciones
- Los NPCs permanecen siempre en la sala de NPCs (lobby)

---

## 🔒 Restricciones Implementadas

### Restricción Espacial de Diálogos de NPC

**Regla obligatoria:** Los diálogos de NPC SOLO pueden ocurrir si:
```
1. El jugador está en la sala de NPCs ("lobby")
2. El NPC está presente físicamente en esa sala
3. El jugador interactúa explícitamente con ese NPC
```

**Si NO se cumple:**
- ❌ No se disparan diálogos automáticos
- ❌ No aparecen reacciones de NPCs
- ❌ No se activan decisiones grupales
- ❌ No hay conversaciones contextuales

---

## 📊 Flujo Narrativo Final

### Capítulo 2: El Estudio

```
1. Jugador entra al estudio con los 5 NPCs (Cap 2 intro)
   ↓
2. Jugador habla con cada NPC (VALIDACIÓN: debe estar en la sala)
   ↓
3. Después de hablar con los 5 → PRIMERA DECISIÓN (capítulo2_initial_decision)
   - Opción A: Hablar sinceramente sobre Simon
   - Opción B: Separarse
   ↓
4. Jugador obtiene el libro de contabilidad (interacción con hotspot)
   ↓
5. Jugador regresa a la sala de NPCs
   ↓
6. Jugador interactúa con Ben → SEGUNDA DECISIÓN (capítulo2_book_decision)
   - Opción A: Confrontar
   - Opción B: Subir a buscar habitación de Simon
   ↓
7. Transición a Cap 3
```

### Capítulo 3: Habitación de Simón

```
1. Jugador entra a la habitación de Simón (Cap 3 intro)
   ↓
2. Jugador interactúa con hotspots:
   - Vaso de agua
   - Mapa de pared
   - Carta inconclusa
   - Mesita de noche
   - Cama deshecha
   ↓
3. Obtiene la llave pequeña y descubre que Simón está vivo
   ↓
4. DECISIÓN DEL CAP 3 (capítulo3_decision)
   - Opción A: Volver al estudio con la llave
   - Opción B: Buscar el Ala Norte
   - Opción C: Mostrar carta al grupo
   ↓
5. Continuación según elección
```

---

## 🛡️ Validaciones Implementadas

### Cap 2 - Decisión Inicial
```
Trigger: GetNpcTalkCount() >= 5 AND IsPlayerInNpcRoom()
Validación: chapter2.initial_decision.shown != true
```

### Cap 2 - Decisión del Libro
```
Trigger: InteractionWithBen() AND HasItem("libro_contabilidad") AND IsPlayerInNpcRoom()
Validación: Automático cuando se interactúa con Ben teniendo el libro
```

### Cap 3 - Decisión
```
Validación: Requiere clue.habitacion.llave_pequena AND simon_vivo
Opciones: Basadas en items obtenidos (solo muestran si se tienen)
```

---

## 🔧 Archivos Modificados

| Archivo | Tipo | Cambio |
|---------|------|--------|
| `RoomContextValidator.cs` | ✨ NUEVO | Sistema centralizado de validación |
| `Chapter2Builder.cs` | 🔄 REFACTORIZADO | De 1 decisión a 2 decisiones |
| `Chapter3Builder.cs` | 🔄 REFACTORIZADO | Narrativa mejorada, mejor feedback |
| `NpcInteractable.cs` | 📝 MODIFICADO | Validación espacial + caso especial Ben |
| `ChapterFlowController.cs` | 📝 MODIFICADO | Lógica para 2 decisiones Cap 2 |

---

## ✨ Beneficios de la Refactorización

✅ **Coherencia Espacial:** Los NPCs no dialogan fuera de su habitación  
✅ **Lógica Narrativa Robusta:** Validaciones centralizadas y consistentes  
✅ **Experiencia Mejorada:** Decisiones contextuales y significativas  
✅ **Mantenibilidad:** Sistema centralizado fácil de extender  
✅ **Eliminación de Inconsistencias:** Los protagonistas investigan solos  
✅ **Progresión Controlada:** Decisiones no se disparan automáticamente  

---

## ⚠️ Notas Importantes

1. **RoomContextValidator debe estar en la escena:** Asegurar que RoomContextValidator está instanciado en la escena actual o en el prefab de GameManager.

2. **Nombres de IDs de conversación:** Los nuevos IDs de conversación son:
   - `chapter2_initial_decision` (NUEVO)
   - `chapter2_book_decision` (NUEVO - reemplaza viejo `chapter2_decision`)

3. **Validaciones automáticas:** Las decisiones ahora dependen de:
   - Ubicación del jugador (validada por RoomContextValidator)
   - Flags de progreso (validados por StoryState)
   - Items en inventario (validados por InventoryState)

---

## 📝 Estado Final

✅ **Sistema completamente refactorizado**  
✅ **Todos los cambios compilados sin errores**  
✅ **Validaciones centralizadas y robustas**  
✅ **Flujo narrativo coherente y consistente**  
✅ **Listo para testing en-game**
