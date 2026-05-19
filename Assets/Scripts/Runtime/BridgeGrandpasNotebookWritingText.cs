using UnityEngine;
using UnityEngine.UI;
using System;

public sealed class BridgeGrandpasNotebookWritingText : MonoBehaviour
{
    private Text target;
    private string fullText;
    private float startAt;
    private float charsPerSecond;
    private Action onComplete;
    private bool playing;

    public void Play(Text text, string content, float delay, float speed, Action completed)
    {
        target = text;
        fullText = content;
        charsPerSecond = Mathf.Max(1f, speed);
        startAt = Time.unscaledTime + Mathf.Max(0f, delay);
        onComplete = completed;
        playing = true;
        if (target != null)
        {
            target.supportRichText = true;
            target.text = "";
        }
    }

    private void Update()
    {
        if (!playing || target == null)
        {
            enabled = false;
            return;
        }

        float elapsed = Time.unscaledTime - startAt;
        if (elapsed < 0f)
        {
            target.text = "";
            return;
        }

        int visible = Mathf.Clamp(Mathf.FloorToInt(elapsed * charsPerSecond), 0, fullText.Length);
        if (visible >= fullText.Length)
        {
            target.text = fullText;
            playing = false;
            onComplete?.Invoke();
            onComplete = null;
            enabled = false;
            return;
        }

        string cursor = Mathf.Repeat(Time.unscaledTime * 3f, 1f) > 0.5f ? "<color=#5a3215>|</color>" : "";
        target.text = fullText.Substring(0, visible) + cursor;
    }
}
