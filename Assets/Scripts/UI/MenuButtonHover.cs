using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private static readonly int UnderlayColorId = Shader.PropertyToID("_UnderlayColor");
    private static readonly int UnderlayOffsetXId = Shader.PropertyToID("_UnderlayOffsetX");
    private static readonly int UnderlayOffsetYId = Shader.PropertyToID("_UnderlayOffsetY");
    private static readonly int UnderlayDilateId = Shader.PropertyToID("_UnderlayDilate");
    private static readonly int UnderlaySoftnessId = Shader.PropertyToID("_UnderlaySoftness");

    [SerializeField] private float hoverScale = 1.05f;
    [SerializeField] private float animationDuration = 0.15f;
    [SerializeField] private Color glowColor = new Color32(179, 18, 23, 255);
    [SerializeField] [Range(0f, 1f)] private float glowAlpha = 0.85f;

    private RectTransform rectTransform;
    private TMP_Text label;
    private Vector3 defaultScale = Vector3.one;
    private Material runtimeMaterial;
    private Coroutine animationRoutine;

    private void Awake()
    {
        rectTransform = transform as RectTransform;
        label = GetComponentInChildren<TMP_Text>(true);

        if (rectTransform != null)
        {
            defaultScale = rectTransform.localScale;
        }

        if (label == null || label.fontSharedMaterial == null)
        {
            return;
        }

        runtimeMaterial = new Material(label.fontSharedMaterial);
        runtimeMaterial.EnableKeyword("UNDERLAY_ON");
        runtimeMaterial.SetFloat(UnderlayOffsetXId, 0f);
        runtimeMaterial.SetFloat(UnderlayOffsetYId, -0.2f);
        runtimeMaterial.SetFloat(UnderlayDilateId, 0.4f);
        runtimeMaterial.SetFloat(UnderlaySoftnessId, 0.45f);
        label.fontMaterial = runtimeMaterial;
        SetGlowAlpha(0f);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        StartAnimation(defaultScale * hoverScale, glowAlpha);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StartAnimation(defaultScale, 0f);
    }

    private void StartAnimation(Vector3 targetScale, float targetGlowAlpha)
    {
        if (animationRoutine != null)
        {
            StopCoroutine(animationRoutine);
        }

        animationRoutine = StartCoroutine(Animate(targetScale, targetGlowAlpha));
    }

    private IEnumerator Animate(Vector3 targetScale, float targetGlowAlpha)
    {
        Vector3 startScale = rectTransform != null ? rectTransform.localScale : defaultScale;
        float startGlowAlpha = runtimeMaterial != null ? runtimeMaterial.GetColor(UnderlayColorId).a : 0f;
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / animationDuration);

            if (rectTransform != null)
            {
                rectTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
            }

            SetGlowAlpha(Mathf.Lerp(startGlowAlpha, targetGlowAlpha, t));
            yield return null;
        }

        if (rectTransform != null)
        {
            rectTransform.localScale = targetScale;
        }

        SetGlowAlpha(targetGlowAlpha);
        animationRoutine = null;
    }

    private void SetGlowAlpha(float alpha)
    {
        if (runtimeMaterial == null)
        {
            return;
        }

        Color color = glowColor;
        color.a = alpha;
        runtimeMaterial.SetColor(UnderlayColorId, color);
    }

    private void OnDisable()
    {
        if (rectTransform != null)
        {
            rectTransform.localScale = defaultScale;
        }

        SetGlowAlpha(0f);
    }

    private void OnDestroy()
    {
        if (runtimeMaterial != null)
        {
            Destroy(runtimeMaterial);
        }
    }
}
