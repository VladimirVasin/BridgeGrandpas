using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private const string ObservationCardArtResourcesPath = "ObservationCards";

    private readonly Dictionary<string, Sprite> observationCardArtCache = new Dictionary<string, Sprite>();

    private string ObservationCardCaption(ObservationCard card)
    {
        if (card == null || string.IsNullOrWhiteSpace(card.Label))
        {
            return "наблюдение";
        }

        string label = card.Label.Trim();
        return label.Length <= 34 ? label : label.Substring(0, 31) + "...";
    }

    private Sprite ObservationCardArtSprite(string label)
    {
        string key = ObservationCardArtKey(label);
        Sprite cached;
        if (observationCardArtCache.TryGetValue(key, out cached))
        {
            return cached;
        }

        Sprite sprite = LoadObservationCardArtByKey(key);
        string fileKey = ObservationCardArtFileKey(key);
        if (sprite == null && fileKey != key)
        {
            sprite = LoadObservationCardArtByKey(fileKey);
        }

        if (sprite == null && key != "default")
        {
            sprite = ObservationCardArtSprite("default");
        }

        observationCardArtCache[key] = sprite;
        return sprite;
    }

    private Sprite LoadObservationCardArtByKey(string key)
    {
        Sprite sprite = Resources.Load<Sprite>(ObservationCardArtResourcesPath + "/" + key);
        if (sprite == null)
        {
            Texture2D texture = Resources.Load<Texture2D>(ObservationCardArtResourcesPath + "/" + key);
            if (texture != null)
            {
                texture.filterMode = FilterMode.Point;
                sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
            }
        }

        return sprite;
    }

    private string ObservationCardArtFileKey(string key)
    {
        switch (key)
        {
            case "fire_barrel":
                return "1FireBarrel";
            case "dry_spot":
                return "2DrySpot";
            case "corrupted_account":
                return "685a2880-4327-4748-a5bd-d25a39679662";
            default:
                return key;
        }
    }

    private string ObservationCardArtKey(string label)
    {
        string text = string.IsNullOrWhiteSpace(label) ? "" : label.ToLowerInvariant();
        if (text.Contains("бочка"))
        {
            return "fire_barrel";
        }

        if (text.Contains("сухое"))
        {
            return "dry_spot";
        }

        if (text.Contains("учёт") || text.Contains("учет") || text.Contains("account") || text.Contains("observer"))
        {
            return "corrupted_account";
        }

        if (text.Contains("первые"))
        {
            return "first_grandpas";
        }

        if (text.Contains("планы"))
        {
            return "old_men_plans";
        }

        if (text.Contains("новый дед"))
        {
            return "new_grandpa";
        }

        if (text.Contains("комис"))
        {
            return "inspection";
        }

        if (text.Contains("версия"))
        {
            return "event_choice";
        }

        if (text.Contains("шорох"))
        {
            return "event";
        }

        if (text.Contains("радио"))
        {
            return "radio";
        }

        if (text.Contains("уход"))
        {
            return "expedition_start";
        }

        if (text.Contains("возвращ"))
        {
            return "expedition_return";
        }

        if (text.Contains("кубик"))
        {
            return "expedition_dice";
        }

        return "default";
    }

    private void CreateObservationCardPlaceholderArt(RectTransform parent, string seedText)
    {
        int seed = ObservationCardSeed(seedText);
        Color baseColor = Color.Lerp(new Color(0.16f, 0.12f, 0.09f, 1f), new Color(0.46f, 0.29f, 0.12f, 1f), ((seed >> 3) & 7) / 7f);
        Image background = parent.GetComponent<Image>();
        if (background != null)
        {
            background.color = baseColor;
        }

        for (int i = 0; i < 14; i++)
        {
            int value = seed + i * 97;
            RectTransform pixel = CreatePanel("Placeholder Pixel " + i, parent, ObservationCardPlaceholderColor(value));
            pixel.anchorMin = new Vector2(((value >> 2) & 15) / 16f, ((value >> 7) & 15) / 16f);
            pixel.anchorMax = pixel.anchorMin;
            pixel.pivot = new Vector2(0.5f, 0.5f);
            pixel.sizeDelta = new Vector2(10f + ((value >> 11) & 3) * 8f, 8f + ((value >> 13) & 3) * 7f);
            pixel.anchoredPosition = Vector2.zero;
            pixel.GetComponent<Image>().raycastTarget = false;
        }
    }

    private Color ObservationCardPlaceholderColor(int value)
    {
        int lane = Mathf.Abs(value) % 4;
        switch (lane)
        {
            case 0:
                return new Color(0.92f, 0.74f, 0.42f, 0.92f);
            case 1:
                return new Color(0.38f, 0.20f, 0.10f, 0.95f);
            case 2:
                return new Color(0.12f, 0.12f, 0.13f, 0.95f);
            default:
                return new Color(0.70f, 0.34f, 0.18f, 0.90f);
        }
    }

    private int ObservationCardSeed(string text)
    {
        int hash = 23;
        string value = string.IsNullOrEmpty(text) ? "default" : text;
        for (int i = 0; i < value.Length; i++)
        {
            hash = hash * 31 + value[i];
        }

        return Mathf.Abs(hash);
    }
}
