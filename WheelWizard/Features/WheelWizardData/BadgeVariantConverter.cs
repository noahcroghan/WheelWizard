using System.Text.Json;
using System.Text.Json.Serialization;
using WheelWizard.WheelWizardData.Domain;

namespace WheelWizard.WheelWizardData;

public class BadgeVariantConverter : JsonConverter<BadgeVariant>
{
    public override BadgeVariant Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String) 
            return BadgeVariant.None;
        
        var enumString = reader.GetString();
        if (Enum.TryParse(typeof(BadgeVariant), enumString, true, out var result))
            return (BadgeVariant)result;
        
        return BadgeVariant.None;
    }

    public override void Write(Utf8JsonWriter writer, BadgeVariant value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
