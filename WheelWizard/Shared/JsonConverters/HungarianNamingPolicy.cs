using System.Text.Json;

namespace WheelWizard.Shared.JsonConverters;

public class HungarianNamingPolicy : JsonNamingPolicy
{
    public override string ConvertName(string name)
    {
        if (name.StartsWith("Count"))
        {
            return "_s" + name;
        }
        else if (name.EndsWith("Data"))
        {
            return "_a" + name;
        }
        else
        {
            return "_s" + name;
        }
    }
}
