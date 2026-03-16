using UnityEngine;
using UnityEngine.UI;

public class FooterPulse : MonoBehaviour
{
    [SerializeField] private float minAlpha = 0.6f;
    [SerializeField] private float maxAlpha = 0.8f;
    [SerializeField] private float loopDuration = 2f;

    private Graphic targetGraphic;
    private Color baseColor;

    private void Awake()
    {
        targetGraphic = GetComponent<Graphic>();
        if (targetGraphic != null)
        {
            baseColor = targetGraphic.color;
        }
    }

    private void Update()
    {
        if (targetGraphic == null)
        {
            return;
        }

        float t = Mathf.PingPong(Time.unscaledTime / (loopDuration * 0.5f), 1f);
        Color color = baseColor;
        color.a = Mathf.Lerp(minAlpha, maxAlpha, t);
        targetGraphic.color = color;
    }
}
