# Guía: Crear el Sistema de Notificaciones

## Paso 1: Crear la estructura de UI en el Canvas

1. En la **Jerarquía**, click derecho en **Canvas** → **Create Empty**
2. Llámalo: `NotificationSystem`
3. Con `NotificationSystem` seleccionado:
   - En **Inspector** → **Add Component** → busca **NotificationPopup** → Agregalo

## Paso 2: Crear el Panel Popup dentro de NotificationSystem

1. Click derecho en **NotificationSystem** → **UI → Panel**
2. Llámalo: `PopupPanel`
3. Con `PopupPanel` seleccionado:
   - **Image** → Color: Negro con algo de transparencia (0, 0, 0, 0.8)
   - **Rect Transform**:
     - Ancho/Alto: Ajusta a tu preferencia (ej: 400x300)
     - Centra en pantalla

## Paso 3: Crear Contenido dentro de PopupPanel

### 3.1 Imagen (opcional)
1. Click derecho en **PopupPanel** → **UI → Image**
2. Llámalo: `IconImage`
3. Ajusta posición/tamaño en el panel (esquina superior)

### 3.2 Texto del Mensaje
1. Click derecho en **PopupPanel** → **UI → Text - TextMeshPro**
2. Llámalo: `MessageText`
3. Configura:
   - Texto: "Test Message"
   - Font Size: 28
   - Color: Blanco
   - Alineación: Centro
   - Posición: Centro del panel

### 3.3 Botón de Confirmación
1. Click derecho en **PopupPanel** → **UI → Button - TextMeshPro**
2. Llámalo: `ConfirmButton`
3. Configura:
   - Imagen: Color blanco
   - Posición: Abajo del panel
   - Texto botón: "OK"

## Paso 4: Conectar referencias en NotificationPopup

1. Selecciona **NotificationSystem**
2. En el **Inspector** busca el componente **NotificationPopup (Script)**
3. Asigna (drag & drop desde Hierarchy):
   - **Popup Panel**: Arrastra `PopupPanel`
   - **Notification Image**: Arrastra `IconImage`
   - **Message Text**: Arrastra `MessageText`
   - **Confirm Button**: Arrastra `ConfirmButton`
4. **Auto Close Delay**: Deja en 3

## Resultado
- Al recoger llaves → popup con mensaje
- Al intentar abrir puertas → popup con estado (bloqueada/abierta)
