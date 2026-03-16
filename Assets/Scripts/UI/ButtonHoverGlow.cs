using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverGlow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private static readonly int UnderlayColorId = Shader.PropertyToID("_UnderlayColor");
    private static readonly int UnderlayOffsetXId = Shader.PropertyToID("_UnderlayOffsetX");
    private static readonly int UnderlayOffsetYId = Shader.PropertyToID("_UnderlayOffsetY");
    private static readonly int UnderlayDilateId = Shader.PropertyToID("_UnderlayDilate");
    private static readonly int UnderlaySoftnessId = Shader.PropertyToID("_UnderlaySoftness");

    [SerializeField] private float hoverScale = 1.05f;
    [SerializeField] private float animationTime = 0.15f;
    [SerializeField] private Color glowColor = new Color32(179, 18, 23, 0);
    [SerializeField] private float glowAlpha = 0.35f;

    private RectTransform rectTransform;
    private TMP_Text label;
    private Material runtimeMaterial;
    private Vector3 defaultScale = Vector3.one;
    private Coroutine animationRoutine;
    private Canvas parentCanvas;
    private bool isHovered;

    private void Awake()
    {
        rectTransform = transform as RectTransform;
        label = GetComponentInChildren<TMP_Text>(true);
        parentCanvas = GetComponentInParent<Canvas>();

        if (rectTransform != null)
        {
            defaultScale = rectTransform.localScale;
        }

        if (label != null)
        {
            runtimeMaterial = new Material(label.fontSharedMaterial);
            runtimeMaterial.EnableKeyword("UNDERLAY_ON");
            runtimeMaterial.SetFloat(UnderlayOffsetXId, 0f);
            runtimeMaterial.SetFloat(UnderlayOffsetYId, -0.15f);
            runtimeMaterial.SetFloat(UnderlayDilateId, 0.3f);
            runtimeMaterial.SetFloat(UnderlaySoftnessId, 0.5f);
            label.fontMaterial = runtimeMaterial;
            SetGlow(0f);
        }
    }

    private void Update()
    {
        if (rectTransform == null)
        {
            return;
        }

        Camera eventCamera = null;
        if (parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            eventCamera = parentCanvas.worldCamera;
        }

        bool shouldHover = RectTransformUtility.RectangleContainsScreenPoint(
            rectTransform,
            Input.mousePosition,
            eventCamera);

        if (shouldHover != isHovered)
        {
            SetHovered(shouldHover);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetHovered(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetHovered(false);
    }

    private void SetHovered(bool hovered)
    {
        isHovered = hovered;
        StartHoverAnimation(hovered ? defaultScale * hoverScale : defaultScale, hovered ? glowAlpha : 0f);
    }

    private void StartHoverAnimation(Vector3 targetScale, float targetGlowAlpha)
    {
        if (animationRoutine != null)
        {
            StopCoroutine(animationRoutine);
        }

        animationRoutine = StartCoroutine(Animate(targetScale, targetGlowAlpha));
    }

    private IEnumerator Animate(Vector3 targetScale, float targetGlowAlpha)
    {
        Vector3 startingScale = rectTransform != null ? rectTransform.localScale : defaultScale;
        float startingGlowAlpha = runtimeMaterial != null ? runtimeMaterial.GetColor(UnderlayColorId).a : 0f;
        float elapsed = 0f;

        while (elapsed < animationTime)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / animationTime);

            if (rectTransform != null)
            {
                rectTransform.localScale = Vector3.Lerp(startingScale, targetScale, t);
            }

            SetGlow(Mathf.Lerp(startingGlowAlpha, targetGlowAlpha, t));
            yield return null;
        }

        if (rectTransform != null)
        {
            rectTransform.localScale = targetScale;
        }

        SetGlow(targetGlowAlpha);
        animationRoutine = null;
    }

    private void SetGlow(float alpha)
    {
        if (runtimeMaterial == null)
        {
            return;
        }

        Color color = glowColor;
        color.a = alpha;
        runtimeMaterial.SetColor(UnderlayColorId, color);
    }
}
