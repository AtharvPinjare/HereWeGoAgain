using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CorruptedTextAnomaly : AnomalyBase
{
    [System.Serializable]
    public class TextObject
    {
        public TextMeshPro textComponent;
        public string originalText;
        public string corruptedText;
        [System.NonSerialized] public bool isCorrupted;
    }

    [SerializeField] private TextObject[] textObjects;
    [SerializeField] private float corruptionSpreadInterval = 8.0f;
    [SerializeField] private float jumpscareDelay = 3.0f;
    [SerializeField] private Image jumpscareOverlay;
    [SerializeField] private Text jumpscareText;
    [SerializeField] private string[] jumpscareMessages = { "HELP", "RUN", "YOU SAW NOTHING", "TOO LATE", "DYING" };

    private Coroutine corruptionCoroutine = null;
    private bool isFailStateTriggered = false;
    private bool greenPressFailActive = false;

    public override void Activate()
    {
        if (data == null)
        {
            return;
        }

        data.isActive = true;
        data.isResolved = false;
        isFailStateTriggered = false;
        greenPressFailActive = false;

        ResetAllText();
        HideJumpscare();
        StopCorruptionCoroutine();
        corruptionCoroutine = StartCoroutine(CorruptionSpread());
    }

    public override void Resolve()
    {
        if (data == null)
        {
            return;
        }

        data.isActive = false;
        data.isResolved = true;
        StopCorruptionCoroutine();
        HideJumpscare();
        ResetAllText();
        EventBus.OnAnomalyResolved?.Invoke(data.id);
    }

    public override void TriggerFailState()
    {
        if (isFailStateTriggered)
        {
            return;
        }

        isFailStateTriggered = true;

        if (data != null)
        {
            data.isActive = false;
        }

        StopCorruptionCoroutine();
        HideJumpscare();
        ResetAllText();

        if (data != null)
        {
            EventBus.OnAnomalyFailState?.Invoke(data.id);
        }

        EventBus.OnInputLocked?.Invoke();
        StartCoroutine(BlackoutAndReset(0.3f));
    }

    public override void ResetAnomaly()
    {
        StopCorruptionCoroutine();
        StopAllCoroutines();
        isFailStateTriggered = false;
        greenPressFailActive = false;
        HideJumpscare();
        ResetAllText();

        if (data != null)
        {
            data.isActive = false;
            data.isResolved = false;
        }
    }

    public void TriggerGreenFail()
    {
        if (isFailStateTriggered || greenPressFailActive)
        {
            return;
        }

        greenPressFailActive = true;
        isFailStateTriggered = true;

        if (data != null)
        {
            data.isActive = false;
            EventBus.OnAnomalyFailState?.Invoke(data.id);
        }

        StopCorruptionCoroutine();
        EventBus.OnInputLocked?.Invoke();
        ResetAllText();
        HideJumpscare();
        StartCoroutine(BlackoutAndReset(0.3f));
    }

    private IEnumerator CorruptionSpread()
    {
        if (textObjects == null || textObjects.Length == 0)
        {
            corruptionCoroutine = null;
            yield break;
        }

        for (int i = 0; i < textObjects.Length; i++)
        {
            if (data == null || !data.isActive)
            {
                corruptionCoroutine = null;
                yield break;
            }

            if (textObjects[i] == null)
            {
                continue;
            }

            CorruptTextObject(i);
            yield return new WaitForSeconds(corruptionSpreadInterval);
        }

        if (data == null || !data.isActive)
        {
            corruptionCoroutine = null;
            yield break;
        }

        yield return new WaitForSeconds(jumpscareDelay);

        if (data == null || !data.isActive)
        {
            corruptionCoroutine = null;
            yield break;
        }

        ShowJumpscare();
        yield return new WaitForSeconds(0.5f);
        corruptionCoroutine = null;
        TriggerFailState();
    }

    private void CorruptTextObject(int index)
    {
        if (textObjects == null || index < 0 || index >= textObjects.Length)
        {
            return;
        }

        TextObject obj = textObjects[index];
        if (obj == null || obj.textComponent == null)
        {
            return;
        }

        obj.textComponent.text = obj.corruptedText;
        obj.isCorrupted = true;
    }

    private void ResetAllText()
    {
        if (textObjects == null)
        {
            return;
        }

        for (int i = 0; i < textObjects.Length; i++)
        {
            TextObject textObj = textObjects[i];
            if (textObj == null || textObj.textComponent == null)
            {
                continue;
            }

            textObj.textComponent.text = textObj.originalText;
            textObj.isCorrupted = false;
        }
    }

    private void ShowJumpscare()
    {
        if (jumpscareOverlay != null)
        {
            jumpscareOverlay.gameObject.SetActive(true);
        }

        if (jumpscareText != null &&
            jumpscareMessages != null &&
            jumpscareMessages.Length > 0)
        {
            int randIndex = Random.Range(0, jumpscareMessages.Length);
            jumpscareText.text = jumpscareMessages[randIndex];
        }
    }

    private void HideJumpscare()
    {
        if (jumpscareOverlay != null)
        {
            jumpscareOverlay.gameObject.SetActive(false);
        }
    }

    private void StopCorruptionCoroutine()
    {
        if (corruptionCoroutine != null)
        {
            StopCoroutine(corruptionCoroutine);
            corruptionCoroutine = null;
        }
    }
}
