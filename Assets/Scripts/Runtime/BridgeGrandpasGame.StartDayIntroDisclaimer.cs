using UnityEngine;
using UnityEngine.UI;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private const float StartDayIntroDisclaimerFadeInSeconds = 0.55f;
    private const float StartDayIntroDisclaimerFadeOutSeconds = 0.70f;

    private RectTransform startDayIntroDisclaimerRoot;
    private CanvasGroup startDayIntroDisclaimerGroup;

    private void SetupStartDayIntroDisclaimer(Transform parent)
    {
        startDayIntroDisclaimerRoot = CreatePanel("Start Disclaimer Root", parent, new Color(0f, 0f, 0f, 0f));
        startDayIntroDisclaimerRoot.anchorMin = Vector2.zero;
        startDayIntroDisclaimerRoot.anchorMax = Vector2.one;
        startDayIntroDisclaimerRoot.offsetMin = Vector2.zero;
        startDayIntroDisclaimerRoot.offsetMax = Vector2.zero;
        startDayIntroDisclaimerRoot.GetComponent<Image>().raycastTarget = false;
        startDayIntroDisclaimerGroup = startDayIntroDisclaimerRoot.gameObject.AddComponent<CanvasGroup>();
        startDayIntroDisclaimerGroup.interactable = false;
        startDayIntroDisclaimerGroup.blocksRaycasts = false;

        RectTransform panel = CreatePanel("Start Disclaimer Panel", startDayIntroDisclaimerRoot, new Color(0.015f, 0.014f, 0.013f, 0.92f));
        panel.anchorMin = new Vector2(0.5f, 0.5f);
        panel.anchorMax = new Vector2(0.5f, 0.5f);
        panel.pivot = new Vector2(0.5f, 0.5f);
        panel.anchoredPosition = new Vector2(0f, 0f);
        panel.sizeDelta = new Vector2(1120f, 640f);
        panel.GetComponent<Image>().raycastTarget = false;

        Outline outline = panel.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.88f, 0.12f, 0.08f, 0.72f);
        outline.effectDistance = new Vector2(2f, -2f);

        Text age = CreateText("Disclaimer 18+", panel, 44, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(1f, 0.18f, 0.12f));
        age.text = "18+";
        age.rectTransform.anchorMin = new Vector2(0f, 1f);
        age.rectTransform.anchorMax = new Vector2(0f, 1f);
        age.rectTransform.pivot = new Vector2(0f, 1f);
        age.rectTransform.anchoredPosition = new Vector2(44f, -34f);
        age.rectTransform.sizeDelta = new Vector2(128f, 74f);

        Text title = CreateText("Disclaimer Title", panel, 30, FontStyle.Bold, TextAnchor.UpperLeft, Color.white);
        title.text = "ПРЕДУПРЕЖДЕНИЕ О СОДЕРЖАНИИ";
        title.rectTransform.anchorMin = new Vector2(0f, 1f);
        title.rectTransform.anchorMax = new Vector2(1f, 1f);
        title.rectTransform.offsetMin = new Vector2(188f, -82f);
        title.rectTransform.offsetMax = new Vector2(-44f, -34f);

        RectTransform line = CreatePanel("Disclaimer Divider", panel, new Color(1f, 1f, 1f, 0.22f));
        line.anchorMin = new Vector2(0f, 1f);
        line.anchorMax = new Vector2(1f, 1f);
        line.pivot = new Vector2(0.5f, 1f);
        line.anchoredPosition = new Vector2(0f, -104f);
        line.sizeDelta = new Vector2(-88f, 2f);
        line.GetComponent<Image>().raycastTarget = false;

        Text body = CreateText("Disclaimer Body", panel, 22, FontStyle.Normal, TextAnchor.UpperLeft, new Color(0.92f, 0.91f, 0.86f));
        body.text =
            "Эта игра предназначена только для совершеннолетней аудитории.\n\n" +
            "Игра содержит психологический хоррор, тревожные образы, темы навязчивого наблюдения, " +
            "резкие звуковые эффекты, мигающие световые эффекты, интенсивные визуальные глитчи, " +
            "искажение изображения, внезапные переходы и сцены сильного эмоционального напряжения.\n\n" +
            "Игра не рекомендуется людям с фоточувствительной эпилепсией, повышенной чувствительностью " +
            "к вспышкам света, паническим реакциям, сильной тревожностью или высокой впечатлительностью.\n\n" +
            "Если во время игры вы почувствуете головокружение, тошноту, нарушение зрения, судороги, " +
            "дезориентацию, сердцебиение или выраженный дискомфорт, немедленно прекратите игру и отдохните.";
        body.rectTransform.anchorMin = new Vector2(0f, 0f);
        body.rectTransform.anchorMax = new Vector2(1f, 1f);
        body.rectTransform.offsetMin = new Vector2(58f, 94f);
        body.rectTransform.offsetMax = new Vector2(-58f, -132f);

        Text footer = CreateText("Disclaimer Footer", panel, 17, FontStyle.Italic, TextAnchor.LowerLeft, new Color(0.72f, 0.72f, 0.68f));
        footer.text = "Продолжая игру, вы подтверждаете, что ознакомились с предупреждением.";
        footer.rectTransform.anchorMin = new Vector2(0f, 0f);
        footer.rectTransform.anchorMax = new Vector2(1f, 0f);
        footer.rectTransform.offsetMin = new Vector2(58f, 34f);
        footer.rectTransform.offsetMax = new Vector2(-58f, 86f);

        startDayIntroDisclaimerRoot.gameObject.SetActive(false);
    }

    private void ResetStartDayIntroDisclaimer()
    {
        if (startDayIntroDisclaimerRoot != null)
        {
            startDayIntroDisclaimerRoot.gameObject.SetActive(true);
        }

        if (startDayIntroDisclaimerGroup != null)
        {
            startDayIntroDisclaimerGroup.alpha = 0f;
        }
    }

    private void UpdateStartDayIntroDisclaimer(float elapsed, bool titlePhase)
    {
        if (startDayIntroDisclaimerRoot == null || startDayIntroDisclaimerGroup == null)
        {
            return;
        }

        bool visible = !titlePhase && elapsed < StartDayIntroDarkHold;
        startDayIntroDisclaimerRoot.gameObject.SetActive(visible);
        if (!visible)
        {
            startDayIntroDisclaimerGroup.alpha = 0f;
            return;
        }

        float fadeIn = Mathf.Clamp01(elapsed / StartDayIntroDisclaimerFadeInSeconds);
        float fadeOut = Mathf.Clamp01((StartDayIntroDarkHold - elapsed) / Mathf.Max(0.01f, StartDayIntroDisclaimerFadeOutSeconds));
        startDayIntroDisclaimerGroup.alpha = Mathf.SmoothStep(0f, 1f, fadeIn) * Mathf.SmoothStep(0f, 1f, fadeOut);
    }

    private void HideStartDayIntroDisclaimer()
    {
        if (startDayIntroDisclaimerRoot != null)
        {
            startDayIntroDisclaimerRoot.gameObject.SetActive(false);
        }

        if (startDayIntroDisclaimerGroup != null)
        {
            startDayIntroDisclaimerGroup.alpha = 0f;
        }
    }
}
