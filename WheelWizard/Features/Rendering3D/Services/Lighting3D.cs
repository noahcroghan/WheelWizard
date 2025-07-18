using Microsoft.Xna.Framework;
using WheelWizard.Rendering3D.Domain;

namespace WheelWizard.Rendering3D.Services;

/// <summary>
/// Implementation of 3D lighting control
/// </summary>
public class Lighting3D : I3DLighting
{
    public Color AmbientColor { get; set; } = new Color(0.2f, 0.2f, 0.2f);
    public Color DirectionalColor { get; set; } = Color.White;
    public Vector3 DirectionalDirection { get; set; } = Vector3.Normalize(new Vector3(-1, -1, -1));
    public bool LightingEnabled { get; set; } = true;

    public void SetupThreePointLighting(
        Color keyLightColor,
        Vector3 keyLightDirection,
        Color? fillLightColor = null,
        Color? ambientColor = null
    )
    {
        // Key light (main light)
        DirectionalColor = keyLightColor;
        DirectionalDirection = Vector3.Normalize(keyLightDirection);

        // Fill light would require additional light support in BasicEffect
        // For now, we'll adjust ambient to simulate fill light
        var fillColor = fillLightColor ?? new Color(0.3f, 0.3f, 0.4f);
        var ambient = ambientColor ?? new Color(0.1f, 0.1f, 0.1f);

        // Blend fill light into ambient
        AmbientColor = new Color(
            Math.Min(1.0f, ambient.R / 255.0f + fillColor.R / 255.0f * 0.3f),
            Math.Min(1.0f, ambient.G / 255.0f + fillColor.G / 255.0f * 0.3f),
            Math.Min(1.0f, ambient.B / 255.0f + fillColor.B / 255.0f * 0.3f)
        );

        LightingEnabled = true;
    }

    public void SetupSunLighting(Color sunColor, Vector3 sunDirection, Color? ambientColor = null)
    {
        DirectionalColor = sunColor;
        DirectionalDirection = Vector3.Normalize(sunDirection);
        AmbientColor = ambientColor ?? new Color(0.3f, 0.3f, 0.4f); // Slightly blue ambient for outdoor feel
        LightingEnabled = true;
    }

    public void DisableLighting()
    {
        LightingEnabled = false;
        AmbientColor = Color.White; // Full brightness when lighting is disabled
        DirectionalColor = Color.Transparent;
    }
}
