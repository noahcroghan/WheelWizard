namespace WheelWizard.Views.Popups.MiiCreatorTabs;

public interface IValidatableMiiPage
{
    // Indicates if the data on this specific page is currently valid.
    bool IsPageValid { get; }

    //Allows the main window to trigger final validation before saving.
    void ValidatePage();

    // Allows the page to perform actions just before the Mii is saved.
    // e.g., Final conversion from intermediate properties to the Mii object if needed.
    void PrepareForSave();
}
