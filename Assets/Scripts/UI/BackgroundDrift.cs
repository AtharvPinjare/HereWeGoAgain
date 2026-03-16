using UnityEngine;

public class BackgroundDrift : MonoBehaviour
{
    [SerializeField] private float horizontalRange = 14f;
    [SerializeField] private float cycleDuration = 24f;

    private RectTransform rectTransform;
    private Vector2 basePosition;

    private void Awake()
    {
        rectTransform = transform as RectTransform;
        if (rectTransform != null)
        {
            basePosition = rectTransform.anchoredPosition;
        }
    }

    private void Update()
    {
        if (rectTransform == null || cycleDuration <= 0f)
        {
            return;
        }

        float phase = Time.unscaledTime / cycleDuration * Mathf.PI * 2f;
        rectTransform.anchoredPosition = basePosition + new Vector2(Mathf.Sin(phase) * horizontalRange, 0f);
    }
}
