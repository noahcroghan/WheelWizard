using Avalonia.Controls;
using Avalonia.Media;
using WheelWizard.Views.Components;
using WheelWizard.WiiManagement.Domain.Mii;

namespace WheelWizard.Views.Popups.MiiManagement.MiiEditor;

public partial class EditorFace : MiiEditorBaseControl
{
    public EditorFace(MiiEditorWindow ew)
        : base(ew)
    {
        InitializeComponent();
        PopulateValues();
    }

    private void PopulateValues()
    {
        foreach (var color in Enum.GetNames(typeof(MiiSkinColor)))
        {
            SkinColorBox.Items.Add(color);
            if (color == Editor.Mii.MiiFacial.SkinColor.ToString())
                SkinColorBox.SelectedItem = color;
        }

        foreach (var feature in Enum.GetNames(typeof(MiiFacialFeature)))
        {
            FacialFeatureBox.Items.Add(feature);
            if (feature == Editor.Mii.MiiFacial.FacialFeature.ToString())
                FacialFeatureBox.SelectedItem = feature;
        }
        CreateHeadShapeButtons();
    }

    private void CreateHeadShapeButtons()
    {
        var color1 = new SolidColorBrush(ViewUtils.Colors.Neutral50); // Skin Color
        var color2 = new SolidColorBrush(ViewUtils.Colors.Neutral300); // Skin border Color
        var color3 = new SolidColorBrush(ViewUtils.Colors.Neutral950); // Hair Color
        var color4 = new SolidColorBrush(ViewUtils.Colors.Danger800); // Hat main color
        var color5 = new SolidColorBrush(ViewUtils.Colors.Danger900); // Hat accent color
        var selectedColor3 = new SolidColorBrush(ViewUtils.Colors.Neutral700); // Hair Color - Selected
        SetButtons(
            "MiiFace",
            7,
            HeadTypesGrid,
            (index, button) =>
            {
                button.IsChecked = index == (int)Editor.Mii.MiiFacial.FaceShape;
                button.Color1 = color1;
                button.Color2 = color2;
                button.Color3 = color3;
                button.Color4 = color4;
                button.Color5 = color5;
                button.Click += (_, _) => SetFaceType(index);
                button.SelectedColor3 = selectedColor3;
            }
        );
    }

    private void SetFaceType(int index)
    {
        if (Editor?.Mii?.MiiEyebrows == null)
            return;

        var current = Editor.Mii.MiiFacial;
        //faceshape is an enum so when comparing
        if (index == (int)current.FaceShape)
            return; // No change
        var result = MiiFacialFeatures.Create(
            (MiiFaceShape)index,
            current.SkinColor,
            current.FacialFeature,
            current.MingleOff,
            current.Downloaded
        );
        if (result.IsFailure)
        {
            ViewUtils.ShowSnackbar($"Error updating Face Shape: {result.Error.Message}");
            // Reset the button state
            foreach (var child in HeadTypesGrid.Children)
            {
                if (child is MultiIconRadioButton button && button.IsChecked == true)
                {
                    button.IsChecked = false;
                }
            }
            return;
        }
        Editor.Mii.MiiFacial = result.Value;
        Editor.Mii.ClearImages();
        // Update the selected button
        foreach (var child in HeadTypesGrid.Children)
        {
            if (child is MultiIconRadioButton button && button.IsChecked == true)
            {
                button.IsChecked = false;
            }
        }
    }

    private void SkinColorBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var value = SkinColorBox.SelectedItem;
        if (value is null)
            return;

        var selectedColor = (MiiSkinColor)Enum.Parse(typeof(MiiSkinColor), value.ToString()!);
        var currentFacial = Editor.Mii.MiiFacial;

        if (selectedColor == currentFacial.SkinColor)
            return; // No change

        var result = MiiFacialFeatures.Create(
            currentFacial.FaceShape,
            selectedColor,
            currentFacial.FacialFeature,
            currentFacial.MingleOff,
            currentFacial.Downloaded
        );

        if (result.IsFailure)
        {
            return;
        }

        Editor.Mii.MiiFacial = result.Value;
        Editor.Mii.ClearImages();
    }

    private void FacialFeatureBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var value = FacialFeatureBox.SelectedItem;
        if (value is null)
            return;

        var selectedFeature = (MiiFacialFeature)Enum.Parse(typeof(MiiFacialFeature), value.ToString()!);
        var currentFacial = Editor.Mii.MiiFacial;

        if (selectedFeature == currentFacial.FacialFeature)
            return; // No change

        var result = MiiFacialFeatures.Create(
            currentFacial.FaceShape,
            currentFacial.SkinColor,
            selectedFeature,
            currentFacial.MingleOff,
            currentFacial.Downloaded
        );

        if (result.IsFailure)
        {
            ViewUtils.ShowSnackbar($"Error updating Facial Feature: {result.Error.Message}");
            return;
        }

        Editor.Mii.MiiFacial = result.Value;
        Editor.Mii.ClearImages();
    }
}
