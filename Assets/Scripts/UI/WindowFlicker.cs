using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WindowFlicker : MonoBehaviour
{
    [SerializeField] private float minInterval = 10f;
    [SerializeField] private float maxInterval = 20f;
    [SerializeField] private float flickerDuration = 0.3f;
    [SerializeField] private float brightnessDelta = 0.06f;

    private Image targetImage;
    private Color baseColor;

    private void Awake()
    {
        targetImage = GetComponent<Image>();
        if (targetImage != null)
        {
            baseColor = targetImage.color;
        }
    }

    private void OnEnable()
    {
        StartCoroutine(FlickerLoop());
    }

    private IEnumerator FlickerLoop()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(Random.Range(minInterval, maxInterval));

            if (targetImage == null)
            {
                continue;
            }

            targetImage.color = new Color(
                Mathf.Clamp01(baseColor.r + brightnessDelta),
                Mathf.Clamp01(baseColor.g + brightnessDelta),
                Mathf.Clamp01(baseColor.b + brightnessDelta),
                baseColor.a);

            yield return new WaitForSecondsRealtime(flickerDuration);
            targetImage.color = baseColor;
        }
    }
}
