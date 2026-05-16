INSTRUCCIONES - Cursores personalizados para CursorManager
============================================================

Esta carpeta debe contener dos texturas PNG:

    hand_cursor.png    — cursor de mano (hover sobre interactuables)
    default_cursor.png — cursor de flecha personalizado (cursor por defecto)

Requisitos para ambas texturas:
- Tamaño: 32x32 píxeles (recomendado)
- Formato: PNG con transparencia (fondo alfa = 0)

Configuración en Unity Inspector (para CADA textura):
1. Selecciona el PNG en el Project panel
2. En el Inspector:
   - Texture Type: Cursor
   - Max Size: 32
   - Clic en Apply

El sistema CursorManager carga ambas texturas automáticamente via Resources.Load.
CursorInitializer aplica el cursor por defecto desde el primer frame del juego.
