using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "Scene_01";
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 1f;

    private RawImage menuSnowImage;
    private Texture2D menuSnowTexture;

    private void Awake()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        EnsureMenuFogOrder();
        EnsureMenuSnow();

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = false;
        }

        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);
        }
    }

    private void Start()
    {
        if (canvasGroup != null)
        {
            StartCoroutine(FadeCanvasIn());
        }
    }

    private void Update()
    {
        if (menuSnowImage == null)
        {
            return;
        }

        Rect uvRect = menuSnowImage.uvRect;
        uvRect.x = Mathf.Repeat(Time.unscaledTime * 0.0025f, 1f);
        uvRect.y = Mathf.Repeat(Time.unscaledTime * 0.01f, 1f);
        menuSnowImage.uvRect = uvRect;
    }

    public void OnStartPressed()
    {
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);
        }
        Debug.Log("Starting game...");
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnQuitPressed()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void OnOptionsPressed()
    {
        if (optionsPanel == null)
        {
            return;
        }

        optionsPanel.SetActive(!optionsPanel.activeSelf);
    }

    public void OnReturnToMenuPressed()
    {
        SceneManager.LoadScene("MainMenu");
    }

    private IEnumerator FadeCanvasIn()
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    private void EnsureMenuFogOrder()
    {
        Transform background = transform.Find("BackgroundImage");
        Transform fog = transform.Find("MenuFog");

        if (background == null || fog == null)
        {
            return;
        }

        fog.SetSiblingIndex(background.GetSiblingIndex() + 1);
    }

    private void EnsureMenuSnow()
    {
        Transform existingSnow = transform.Find("MenuSnow");
        if (existingSnow != null)
        {
            menuSnowImage = existingSnow.GetComponent<RawImage>();
            return;
        }

        GameObject menuSnow = new GameObject("MenuSnow", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
        menuSnow.transform.SetParent(transform, false);
        menuSnow.transform.SetSiblingIndex(1);

        RectTransform snowRect = menuSnow.GetComponent<RectTransform>();
        if (snowRect != null)
        {
            snowRect.anchorMin = Vector2.zero;
            snowRect.anchorMax = Vector2.one;
            snowRect.offsetMin = Vector2.zero;
            snowRect.offsetMax = Vector2.zero;
            snowRect.anchoredPosition = Vector2.zero;
        }

        menuSnowImage = menuSnow.GetComponent<RawImage>();
        if (menuSnowImage == null)
        {
            return;
        }

        menuSnowTexture = BuildSnowTexture();
        menuSnowImage.texture = menuSnowTexture;
        menuSnowImage.color = new Color(1f, 1f, 1f, 0.16f);
        menuSnowImage.raycastTarget = false;
        menuSnowImage.uvRect = new Rect(0f, 0f, 2f, 2f);
    }

    private Texture2D BuildSnowTexture()
    {
        const int size = 256;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Repeat;
        texture.filterMode = FilterMode.Bilinear;

        Color clear = new Color(0f, 0f, 0f, 0f);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                texture.SetPixel(x, y, clear);
            }
        }

        Random.State originalState = Random.state;
        Random.InitState(1337);

        for (int i = 0; i < 320; i++)
        {
            int x = Random.Range(0, size);
            int y = Random.Range(0, size);
            float alpha = Random.Range(0.2f, 0.85f);
            texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
        }

        Random.state = originalState;
        texture.Apply();
        return texture;
    }
    
}
