using UnityEngine;

public class MenuCursorController : MonoBehaviour
{
    [SerializeField] private Vector2 hotspot = new Vector2(2f, 2f);

    private Texture2D cursorTexture;

    private void Start()
    {
        cursorTexture = BuildCursorTexture();
        Cursor.SetCursor(cursorTexture, hotspot, CursorMode.Auto);
        Cursor.visible = true;
    }

    private Texture2D BuildCursorTexture()
    {
        const int size = 32;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        Color clear = new Color(0f, 0f, 0f, 0f);
        Color white = Color.white;
        Color glow = new Color32(179, 18, 23, 90);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                texture.SetPixel(x, y, clear);
            }
        }

        for (int i = 0; i < 14; i++)
        {
            for (int j = 0; j <= i / 2; j++)
            {
                int px = 3 + j;
                int py = size - 5 - i;
                if (px >= 0 && px < size && py >= 0 && py < size)
                {
                    texture.SetPixel(px, py, glow);
                    if (px + 1 < size)
                    {
                        texture.SetPixel(px + 1, py, glow);
                    }
                }
            }
        }

        for (int i = 0; i < 12; i++)
        {
            int py = size - 6 - i;
            texture.SetPixel(3, py, white);
            if (i < 6)
            {
                texture.SetPixel(4, py, white);
            }
        }

        for (int i = 0; i < 6; i++)
        {
            texture.SetPixel(3 + i, size - 6 - i, white);
        }

        texture.Apply();
        return texture;
    }
}
