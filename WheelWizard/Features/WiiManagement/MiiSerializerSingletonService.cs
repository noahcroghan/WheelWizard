namespace WheelWizard.WiiManagement;

/// <summary>
/// Provides serialization and deserialization for full Mii data.
/// </summary>
public interface IMiiSerializerSingletonService
{
    /// <summary>
    /// Gets the Mii serializer instance.
    /// </summary>
    MiiSerializer MiiSerializer { get; }
}

public class MiiSerializerSingletonService : IMiiSerializerSingletonService
{
    public MiiSerializer MiiSerializer { get; } = new MiiSerializer();
}

