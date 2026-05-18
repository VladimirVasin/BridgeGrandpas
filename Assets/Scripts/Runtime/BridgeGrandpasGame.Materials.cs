using UnityEngine;

public sealed partial class BridgeGrandpasGame : MonoBehaviour
{
    private const string AsphaltAlbedoPath = "Textures/Asphalt/asphalt_square";
    private const string AsphaltNormalPath = "Textures/Asphalt/asphalt_square_nm";
    private const string AsphaltOcclusionPath = "Textures/Asphalt/asphalt_square_oc";

    private Material AsphaltMat(string key, Color tint, Vector2 tiling, float smoothness)
    {
        Material material = Mat(key, tint);
        Texture2D albedo = Resources.Load<Texture2D>(AsphaltAlbedoPath);
        if (albedo == null)
        {
            return material;
        }

        Texture2D normal = Resources.Load<Texture2D>(AsphaltNormalPath);
        Texture2D occlusion = Resources.Load<Texture2D>(AsphaltOcclusionPath);
        ApplyTexture(material, "_MainTex", albedo, tiling);
        ApplyTexture(material, "_BaseMap", albedo, tiling);
        ApplyColor(material, "_Color", tint);
        ApplyColor(material, "_BaseColor", tint);

        if (normal != null)
        {
            ApplyTexture(material, "_BumpMap", normal, tiling);
            SetFloat(material, "_BumpScale", 0.72f);
            material.EnableKeyword("_NORMALMAP");
        }

        if (occlusion != null)
        {
            ApplyTexture(material, "_OcclusionMap", occlusion, tiling);
            SetFloat(material, "_OcclusionStrength", 0.68f);
            material.EnableKeyword("_OCCLUSIONMAP");
        }

        SetFloat(material, "_Smoothness", smoothness);
        SetFloat(material, "_Glossiness", smoothness);
        return material;
    }

    private static void ApplyTexture(Material material, string property, Texture texture, Vector2 tiling)
    {
        if (!material.HasProperty(property))
        {
            return;
        }

        texture.wrapMode = TextureWrapMode.Repeat;
        material.SetTexture(property, texture);
        material.SetTextureScale(property, tiling);
    }

    private static void ApplyColor(Material material, string property, Color color)
    {
        if (material.HasProperty(property))
        {
            material.SetColor(property, color);
        }
    }

    private static void SetFloat(Material material, string property, float value)
    {
        if (material.HasProperty(property))
        {
            material.SetFloat(property, value);
        }
    }
}
