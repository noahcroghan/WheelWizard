using Microsoft.Xna.Framework;

namespace WheelWizard.Rendering3D.Domain;

/// <summary>
/// High-level interface for managing 3D scenes with easy object manipulation
/// </summary>
public interface I3DScene
{
    /// <summary>
    /// Adds a 3D object to the scene
    /// </summary>
    /// <param name="objectId">Unique identifier for the object</param>
    /// <param name="objectType">Type of object to create</param>
    /// <param name="position">Initial position</param>
    /// <param name="rotation">Initial rotation</param>
    /// <param name="scale">Initial scale</param>
    /// <returns>The created scene object</returns>
    I3DSceneObject AddObject(
        string objectId,
        SceneObjectType objectType,
        Vector3? position = null,
        Vector3? rotation = null,
        Vector3? scale = null
    );

    /// <summary>
    /// Adds a 3D model from file to the scene
    /// </summary>
    /// <param name="objectId">Unique identifier for the object</param>
    /// <param name="modelPath">Path to the model file</param>
    /// <param name="position">Initial position</param>
    /// <param name="rotation">Initial rotation</param>
    /// <param name="scale">Initial scale</param>
    /// <returns>The created scene object</returns>
    I3DSceneObject AddModel(string objectId, string modelPath, Vector3? position = null, Vector3? rotation = null, Vector3? scale = null);

    /// <summary>
    /// Gets a scene object by its ID
    /// </summary>
    /// <param name="objectId">The object ID</param>
    /// <returns>The scene object or null if not found</returns>
    I3DSceneObject? GetObject(string objectId);

    /// <summary>
    /// Removes an object from the scene
    /// </summary>
    /// <param name="objectId">The object ID to remove</param>
    /// <returns>True if removed, false if not found</returns>
    bool RemoveObject(string objectId);

    /// <summary>
    /// Clears all objects from the scene
    /// </summary>
    void ClearScene();

    /// <summary>
    /// Gets all objects in the scene
    /// </summary>
    IReadOnlyList<I3DSceneObject> Objects { get; }

    /// <summary>
    /// Camera controller for the scene
    /// </summary>
    I3DCamera Camera { get; }

    /// <summary>
    /// Lighting controller for the scene
    /// </summary>
    I3DLighting Lighting { get; }

    /// <summary>
    /// Updates the scene (called automatically by renderer)
    /// </summary>
    /// <param name="gameTime">Game time information</param>
    void Update(GameTime gameTime);

    /// <summary>
    /// Renders the scene (called automatically by renderer)
    /// </summary>
    /// <param name="gameTime">Game time information</param>
    void Draw(GameTime gameTime);
}
