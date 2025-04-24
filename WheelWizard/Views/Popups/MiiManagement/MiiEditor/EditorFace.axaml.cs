using Avalonia.Controls;
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
        foreach (var shape in Enum.GetNames(typeof(MiiFaceShape)))
        {
            HeadShapeBox.Items.Add(shape);
            if (shape == Editor.Mii.MiiFacial.FaceShape.ToString())
                HeadShapeBox.SelectedItem = shape;
        }

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
    }

    private void HeadShapeBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var value = HeadShapeBox.SelectedItem;
        if (value is null)
            return;

        var selectedShape = (MiiFaceShape)Enum.Parse(typeof(MiiFaceShape), value.ToString()!);
        var currentFacial = Editor.Mii.MiiFacial;

        if (selectedShape == currentFacial.FaceShape)
            return; // No change

        var result = MiiFacialFeatures.Create(
            selectedShape,
            currentFacial.SkinColor,
            currentFacial.FacialFeature,
            currentFacial.MingleOff,
            currentFacial.Downloaded
        );

        if (result.IsFailure)
        {
            // Handle error, maybe show a snackbar?
            return;
        }

        Editor.Mii.MiiFacial = result.Value;
        Editor.Mii.ClearImages();
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
