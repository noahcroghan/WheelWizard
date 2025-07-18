using Microsoft.Xna.Framework;

namespace WheelWizard.Rendering3D.Domain;

/// <summary>
/// Interface for controlling 3D scene lighting
/// </summary>
public interface I3DLighting
{
    /// <summary>
    /// Ambient light color (affects all objects equally)
    /// </summary>
    Color AmbientColor { get; set; }

    /// <summary>
    /// Directional light color (like sunlight)
    /// </summary>
    Color DirectionalColor { get; set; }

    /// <summary>
    /// Direction of the directional light
    /// </summary>
    Vector3 DirectionalDirection { get; set; }

    /// <summary>
    /// Whether lighting is enabled
    /// </summary>
    bool LightingEnabled { get; set; }

    /// <summary>
    /// Sets up basic three-point lighting
    /// </summary>
    /// <param name="keyLightColor">Main light color</param>
    /// <param name="keyLightDirection">Main light direction</param>
    /// <param name="fillLightColor">Fill light color (optional)</param>
    /// <param name="ambientColor">Ambient light color (optional)</param>
    void SetupThreePointLighting(Color keyLightColor, Vector3 keyLightDirection, Color? fillLightColor = null, Color? ambientColor = null);

    /// <summary>
    /// Sets up simple directional lighting (like outdoor sunlight)
    /// </summary>
    /// <param name="sunColor">Sun light color</param>
    /// <param name="sunDirection">Sun light direction</param>
    /// <param name="ambientColor">Ambient light color (optional)</param>
    void SetupSunLighting(Color sunColor, Vector3 sunDirection, Color? ambientColor = null);

    /// <summary>
    /// Disables all lighting (objects will use their vertex colors)
    /// </summary>
    void DisableLighting();
}
