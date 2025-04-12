using WheelWizard.GameBanana.Domain;

namespace WheelWizard.Views.Popups.ModManagement;

public class ModListItem
{
    public required GameBananaModPreview Mod { get; set; }
    public string ImageUrl => Mod.PreviewMedia != null ? Mod.PreviewMedia.Images[0].BaseUrl + "/" + Mod.PreviewMedia.Images[0].File : "";
}
