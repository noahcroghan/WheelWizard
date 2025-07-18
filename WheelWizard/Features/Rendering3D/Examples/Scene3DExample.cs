using Microsoft.Xna.Framework;
using WheelWizard.Rendering3D.Domain;

namespace WheelWizard.Rendering3D.Examples;

/// <summary>
/// Example demonstrating how to use the high-level 3D scene system
/// </summary>
public static class Scene3DExample
{
    /// <summary>
    /// Sets up a basic 3D scene with multiple objects and camera movement
    /// </summary>
    /// <param name="scene">The 3D scene to configure</param>
    public static void SetupBasicScene(I3DScene scene)
    {
        // Clear any existing objects
        scene.ClearScene();

        // Add various 3D objects to the scene
        var cube = scene.AddObject("rotating-cube", SceneObjectType.Cube, position: new Vector3(-2, 0, 0), scale: new Vector3(1.5f));

        var sphere = scene.AddObject("bouncing-sphere", SceneObjectType.Sphere, position: new Vector3(2, 0, 0));

        var pyramid = scene.AddObject("spinning-pyramid", SceneObjectType.Pyramid, position: new Vector3(0, -2, 0));

        var cylinder = scene.AddObject(
            "tall-cylinder",
            SceneObjectType.Cylinder,
            position: new Vector3(0, 2, 0),
            scale: new Vector3(0.8f, 2.0f, 0.8f)
        );

        // Set up camera for a nice view
        scene.Camera.MoveTo(new Vector3(0, 3, 8));
        scene.Camera.LookAt(Vector3.Zero);

        // Set up nice lighting
        scene.Lighting.SetupSunLighting(Color.White, Vector3.Normalize(new Vector3(-1, -1, -1)), new Color(0.3f, 0.3f, 0.4f));

        // Start some animations
        cube.AnimateToRotation(new Vector3(0, MathHelper.TwoPi, 0), 4.0f);
        sphere.AnimateToPosition(new Vector3(2, 2, 0), 2.0f);
    }

    /// <summary>
    /// Demonstrates camera movement and orbiting
    /// </summary>
    /// <param name="scene">The 3D scene</param>
    public static void DemonstrateCameraMovement(I3DScene scene)
    {
        // Move camera to different positions
        scene.Camera.MoveTo(new Vector3(5, 5, 5));
        scene.Camera.LookAt(Vector3.Zero);

        // Orbit around the origin
        scene.Camera.OrbitAround(Vector3.Zero, 10.0f, MathHelper.PiOver4, Single.Pi / 6);

        // Animate camera to a new position
        scene.Camera.AnimateToPosition(new Vector3(0, 10, 0), 3.0f);
        scene.Camera.AnimateToLookAt(Vector3.Zero, 3.0f);
    }

    /// <summary>
    /// Shows how to manipulate objects in real-time
    /// </summary>
    /// <param name="scene">The 3D scene</param>
    /// <param name="gameTime">Current game time</param>
    public static void UpdateObjectsRealTime(I3DScene scene, GameTime gameTime)
    {
        var cube = scene.GetObject("rotating-cube");
        if (cube != null)
        {
            // Continuously rotate the cube
            cube.Rotate(new Vector3(0, (float)gameTime.ElapsedGameTime.TotalSeconds, 0));
        }

        var sphere = scene.GetObject("bouncing-sphere");
        if (sphere != null)
        {
            // Make the sphere bounce up and down
            var time = (float)gameTime.TotalGameTime.TotalSeconds;
            sphere.Position = new Vector3(2, MathF.Sin(time * 2) * 2, 0);
        }

        var pyramid = scene.GetObject("spinning-pyramid");
        if (pyramid != null)
        {
            // Spin the pyramid on multiple axes
            pyramid.Rotate(
                new Vector3((float)gameTime.ElapsedGameTime.TotalSeconds * 0.5f, (float)gameTime.ElapsedGameTime.TotalSeconds, 0)
            );
        }
    }

    /// <summary>
    /// Demonstrates different lighting setups
    /// </summary>
    /// <param name="scene">The 3D scene</param>
    /// <param name="lightingType">Type of lighting to apply</param>
    public static void SetupLighting(I3DScene scene, LightingType lightingType)
    {
        switch (lightingType)
        {
            case LightingType.Outdoor:
                scene.Lighting.SetupSunLighting(
                    Color.LightYellow,
                    Vector3.Normalize(new Vector3(-0.5f, -1, -0.5f)),
                    new Color(0.4f, 0.4f, 0.6f)
                );
                break;

            case LightingType.Studio:
                scene.Lighting.SetupThreePointLighting(
                    Color.White,
                    Vector3.Normalize(new Vector3(-1, -1, -1)),
                    Color.LightBlue,
                    new Color(0.2f, 0.2f, 0.2f)
                );
                break;

            case LightingType.Dramatic:
                scene.Lighting.SetupSunLighting(Color.Orange, Vector3.Normalize(new Vector3(-2, -1, 0)), new Color(0.1f, 0.1f, 0.2f));
                break;

            case LightingType.None:
                scene.Lighting.DisableLighting();
                break;
        }
    }

    /// <summary>
    /// Example of loading and positioning a 3D model (when model loading is implemented)
    /// </summary>
    /// <param name="scene">The 3D scene</param>
    /// <param name="modelPath">Path to the 3D model file</param>
    public static void LoadAndPositionModel(I3DScene scene, string modelPath)
    {
        // Add a 3D model to the scene
        var model = scene.AddModel(
            "my-model",
            modelPath,
            position: new Vector3(0, 0, 0),
            rotation: new Vector3(0, MathHelper.PiOver2, 0),
            scale: new Vector3(2.0f)
        );

        // Position camera to view the model
        scene.Camera.MoveTo(new Vector3(5, 2, 5));
        scene.Camera.LookAt(model.Position);

        // Animate the model
        model.AnimateToRotation(new Vector3(0, MathHelper.TwoPi, 0), 5.0f);
    }
}

/// <summary>
/// Different lighting presets for the scene
/// </summary>
public enum LightingType
{
    Outdoor,
    Studio,
    Dramatic,
    None,
}
