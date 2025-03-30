using System.Text.RegularExpressions;
using WheelWizard.WiiManagement.Domain.Enums;

namespace WheelWizard.WiiManagement;

public static class FullMiiExtentions
{
    // public static OperationResult UpdateMiiName(this FullMii mii, string newName)
    // {
    //     newName = Regex.Replace(newName.Trim(), @"\s+", " ");
    //     if (newName.Length < 3 || newName.Length > 10)
    //     {
    //         return OperationResult.Fail<FullMii>("Mii name must be between 3 and 10 characters.");
    //     }
    //     mii.Name = new MiiName(newName);
    //     return OperationResult.Ok();
    // }
}
