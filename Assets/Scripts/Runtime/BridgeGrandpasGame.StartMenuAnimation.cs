using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif
using UnityEngine.UI;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private const float StartMenuLoadingDuration = 1.45f;

    private RectTransform startMenuBackgroundRect;
    private RectTransform startMenuShadeRect;
    private RectTransform startMenuContentRoot;
    private RectTransform startMenuTitleRect;
    private RectTransform startMenuSubtitleRect;
    private RectTransform startMenuButtonsRect;
    private RectTransform startMenuLoadingRoot;
    private Image startMenuLoadingFill;
    private Text startMenuLoadingText;
    private CanvasGroup startMenuContentGroup;
    private CanvasGroup startMenuButtonsGroup;
    private RectTransform startMenuFireGlowRect;
    private RectTransform startMenuFireCoreRect;
    private Vector2 startMenuCursorParallax;
    private bool startMenuLoading;
    private bool startMenuLoadSavedGame;
    private float startMenuLoadingStartedAt;
    private readonly List<MenuParticle> menuParticles = new List<MenuParticle>();

    private sealed class MenuParticle
    {
        public RectTransform Rect;
        public Image Image;
        public Vector2 Position;
        public float Speed;
        public float Drift;
        public float Phase;
    }

    private void CreateMenuAnimationLayers(Transform parent)
    {
        startMenuFireGlowRect = CreateMenuGlow(parent, "Menu Fire Glow", new Color(1f, 0.45f, 0.12f, 0.95f), new Vector2(620f, 420f));
        startMenuFireGlowRect.anchoredPosition = new Vector2(0f, -18f);
        startMenuFireCoreRect = CreateMenuGlow(parent, "Menu Fire Core", new Color(1f, 0.78f, 0.24f, 0.92f), new Vector2(210f, 155f));
        startMenuFireCoreRect.anchoredPosition = new Vector2(0f, -28f);
        CreateMenuParticles(parent);
    }

    private RectTransform CreateMenuGlow(Transform parent, string name, Color color, Vector2 size)
    {
        GameObject glowObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
        glowObject.transform.SetParent(parent, false);
        RectTransform rect = glowObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;

        RawImage image = glowObject.GetComponent<RawImage>();
        image.texture = CreateRadialGlowTexture(color);
        image.color = Color.white;
        image.raycastTarget = false;
        return rect;
    }

    private Texture2D CreateRadialGlowTexture(Color color)
    {
        const int size = 96;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x + 0.5f) / size - 0.5f;
                float dy = (y + 0.5f) / size - 0.5f;
                float distance = Mathf.Sqrt(dx * dx + dy * dy) * 2f;
                float alpha = Mathf.Pow(Mathf.Clamp01(1f - distance), 2.4f) * color.a;
                texture.SetPixel(x, y, new Color(color.r, color.g, color.b, alpha));
            }
        }

        texture.Apply();
        return texture;
    }

    private void CreateMenuParticles(Transform parent)
    {
        menuParticles.Clear();
        System.Random rng = new System.Random(311);
        for (int i = 0; i < 22; i++)
        {
            AddMenuParticle(parent, rng);
        }
    }

    private void AddMenuParticle(Transform parent, System.Random rng)
    {
        RectTransform rect = CreatePanel("Menu Spark", parent, new Color(1f, 0.55f, 0.16f, 0.70f));
        rect.GetComponent<Image>().raycastTarget = false;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(3f + (float)rng.NextDouble() * 5f, 3f + (float)rng.NextDouble() * 5f);

        MenuParticle particle = new MenuParticle();
        particle.Rect = rect;
        particle.Image = rect.GetComponent<Image>();
        particle.Position = new Vector2(-820f + (float)rng.NextDouble() * 1640f, -430f + (float)rng.NextDouble() * 860f);
        particle.Speed = 18f + (float)rng.NextDouble() * 42f;
        particle.Drift = -18f + (float)rng.NextDouble() * 36f;
        particle.Phase = (float)rng.NextDouble() * 100f;
        menuParticles.Add(particle);
    }

    private void UpdateStartMenuAnimation(float deltaTime)
    {
        if (startMenuCanvas == null || !startMenuCanvas.gameObject.activeSelf)
        {
            return;
        }

        float t = Time.unscaledTime;
        UpdateMenuCursorParallax(deltaTime);
        AnimateMenuBackground(t);
        AnimateMenuFire(t);
        AnimateMenuParticles(deltaTime, t);
        AnimateMenuContent(t);
        UpdateStartMenuLoading(t);
    }

    private void UpdateMenuCursorParallax(float deltaTime)
    {
        Vector2 target = ReadMenuCursorParallaxTarget();
        float lerp = 1f - Mathf.Exp(-deltaTime * 5.5f);
        startMenuCursorParallax = Vector2.Lerp(startMenuCursorParallax, target, lerp);
    }

    private Vector2 ReadMenuCursorParallaxTarget()
    {
        if (Screen.width <= 0 || Screen.height <= 0)
        {
            return Vector2.zero;
        }

        Vector2 cursor;
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current == null)
        {
            return Vector2.zero;
        }

        cursor = Mouse.current.position.ReadValue();
#else
        cursor = Input.mousePosition;
#endif
        float x = Mathf.Clamp(((cursor.x / Screen.width) - 0.5f) * 2f, -1f, 1f);
        float y = Mathf.Clamp(((cursor.y / Screen.height) - 0.5f) * 2f, -1f, 1f);
        return new Vector2(x, y);
    }

    private void AnimateMenuBackground(float t)
    {
        if (startMenuBackgroundRect == null)
        {
            return;
        }

        float zoom = 1.035f + Mathf.Sin(t * 0.17f) * 0.012f;
        startMenuBackgroundRect.localScale = new Vector3(zoom, zoom, 1f);
        Vector2 drift = new Vector2(Mathf.Sin(t * 0.09f) * 18f, Mathf.Cos(t * 0.11f) * 12f);
        startMenuBackgroundRect.anchoredPosition = drift - startMenuCursorParallax * 28f;
    }

    private void AnimateMenuFire(float t)
    {
        float a = Mathf.PerlinNoise(t * 2.4f, 0.31f);
        float b = Mathf.PerlinNoise(6.7f, t * 4.8f);
        if (startMenuFireGlowRect != null)
        {
            startMenuFireGlowRect.localScale = new Vector3(0.92f + a * 0.24f, 0.86f + b * 0.22f, 1f);
            startMenuFireGlowRect.anchoredPosition = new Vector2((a - 0.5f) * 34f, -22f + (b - 0.5f) * 24f) + startMenuCursorParallax * 18f;
        }

        if (startMenuFireCoreRect != null)
        {
            startMenuFireCoreRect.localScale = new Vector3(0.82f + b * 0.38f, 0.78f + a * 0.32f, 1f);
            startMenuFireCoreRect.anchoredPosition = new Vector2((b - 0.5f) * 20f, -34f + (a - 0.5f) * 18f) + startMenuCursorParallax * 24f;
        }
    }

    private void AnimateMenuParticles(float deltaTime, float t)
    {
        for (int i = 0; i < menuParticles.Count; i++)
        {
            MenuParticle particle = menuParticles[i];
            particle.Position.y += particle.Speed * deltaTime;
            particle.Position.x += (Mathf.Sin(t * 2.1f + particle.Phase) * 16f + particle.Drift) * deltaTime;
            if (particle.Position.y > 460f)
            {
                particle.Position.y = -360f;
                particle.Position.x = -220f + Mathf.Repeat(particle.Phase * 91f + t * 33f, 440f);
            }

            particle.Image.color = new Color(1f, 0.55f, 0.16f, 0.28f + Mathf.PerlinNoise(t * 5f, particle.Phase) * 0.55f);
            particle.Rect.anchoredPosition = particle.Position + startMenuCursorParallax * 24f;
        }
    }

    private void AnimateMenuContent(float t)
    {
        float breathe = Mathf.Sin(t * 1.15f) * 0.008f;
        if (startMenuContentRoot != null)
        {
            Vector2 idle = new Vector2(Mathf.Sin(t * 0.23f) * 7f, Mathf.Cos(t * 0.19f) * 5f);
            startMenuContentRoot.anchoredPosition = idle + startMenuCursorParallax * 10f;
        }

        if (startMenuTitleRect != null)
        {
            startMenuTitleRect.localScale = new Vector3(1f + breathe, 1f + breathe, 1f);
        }

        if (startMenuSubtitleRect != null)
        {
            startMenuSubtitleRect.localScale = new Vector3(1f + breathe * 0.6f, 1f + breathe * 0.6f, 1f);
        }

        if (startMenuButtonsRect != null)
        {
            startMenuButtonsRect.anchoredPosition = new Vector2(startMenuCursorParallax.x * 3f, 46f + Mathf.Sin(t * 0.72f) * 2f + startMenuCursorParallax.y * 2f);
        }

        if (startMenuButtonsGroup != null)
        {
            float targetAlpha = startMenuLoading ? 0.36f : 1f;
            startMenuButtonsGroup.alpha = Mathf.Lerp(startMenuButtonsGroup.alpha, targetAlpha, Time.unscaledDeltaTime * 9f);
        }

        if (startMenuContentGroup != null)
        {
            startMenuContentGroup.alpha = Mathf.Lerp(startMenuContentGroup.alpha, 1f, Time.unscaledDeltaTime * 4.5f);
        }
    }

    private void UpdateStartMenuLoading(float t)
    {
        if (!startMenuLoading)
        {
            return;
        }

        float raw = Mathf.Clamp01((Time.unscaledTime - startMenuLoadingStartedAt) / StartMenuLoadingDuration);
        float eased = 1f - Mathf.Pow(1f - raw, 2.7f);
        if (startMenuLoadingFill != null)
        {
            startMenuLoadingFill.fillAmount = eased;
        }

        if (startMenuLoadingText != null)
        {
            int percent = Mathf.RoundToInt(eased * 100f);
            string dots = new string('.', 1 + Mathf.FloorToInt(t * 4f) % 3);
            string action = startMenuLoadSavedGame ? "Читаем старые записи" : "Готовим место под мостом";
            startMenuLoadingText.text = action + dots + " " + percent + "%";
        }

        if (startMenuLoadingRoot != null)
        {
            float pulse = 1f + Mathf.Sin(t * 8f) * 0.012f;
            startMenuLoadingRoot.localScale = new Vector3(pulse, pulse, 1f);
        }

        if (raw >= 1f)
        {
            startMenuLoading = false;
            StartNewGame();
        }
    }
}
