using UnityEngine;

public class BackgroundScaler : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null || spriteRenderer.sprite == null)
        {
            Debug.LogWarning("BackgroundScaler necesita un SpriteRenderer con sprite asignado.");
            return;
        }

        FitToScreen();
    }

    private void Update()
    {
        // Reajustar si cambia el tamaño de pantalla
        FitToScreen();
    }

    private void FitToScreen()
    {
        if (spriteRenderer == null || spriteRenderer.sprite == null || Camera.main == null)
            return;

        transform.localScale = Vector3.one;

        float spriteWidth = spriteRenderer.sprite.bounds.size.x;
        float spriteHeight = spriteRenderer.sprite.bounds.size.y;

        float cameraHeight = Camera.main.orthographicSize * 2f;
        float cameraWidth = cameraHeight * Camera.main.aspect;

        float scaleX = cameraWidth / spriteWidth;
        float scaleY = cameraHeight / spriteHeight;

        // Usar el mayor para que siempre cubra toda la pantalla
        float scale = Mathf.Max(scaleX, scaleY);

        transform.localScale = new Vector3(scale, scale, 1f);
    }
}
