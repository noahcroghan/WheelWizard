using Avalonia.Controls;
using Avalonia.Media;
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

    private static readonly Dictionary<MiiSkinColor, (Color, Color)> SkinColors = new()
    {
        [MiiSkinColor.Light] = (Color.FromRgb(255, 242, 226), Color.FromRgb(224, 194, 154)),
        [MiiSkinColor.Yellow] = (Color.FromRgb(255, 208, 132), Color.FromRgb(229, 167, 80)),
        [MiiSkinColor.Red] = (Color.FromRgb(222, 154, 91), Color.FromRgb(224, 118, 48)),
        [MiiSkinColor.Pink] = (Color.FromRgb(255, 203, 195), Color.FromRgb(255, 173, 158)),
        [MiiSkinColor.DarkBrown] = (Color.FromRgb(194, 106, 33), Color.FromRgb(154, 69, 18)),
        [MiiSkinColor.Brown] = (Color.FromRgb(145, 98, 40), Color.FromRgb(58, 43, 11)),
    };

    private void PopulateValues()
    {
        foreach (var feature in Enum.GetNames(typeof(MiiFacialFeature)))
        {
            FacialFeatureBox.Items.Add(feature);
            if (feature == Editor.Mii.MiiFacial.FacialFeature.ToString())
                FacialFeatureBox.SelectedItem = feature;
        }
        CreateSkinColorButtons();
        CreateHeadShapeButtons();
    }

    private void CreateHeadShapeButtons()
    {
        var color1 = new SolidColorBrush(ViewUtils.Colors.Neutral50); // Skin Color
        var color2 = new SolidColorBrush(ViewUtils.Colors.Neutral300); // Skin border Color
        var color3 = new SolidColorBrush(Colors.Transparent); // Face features
        SetButtons(
            "MiiFace",
            8,
            HeadTypesGrid,
            (index, button) =>
            {
                button.IsChecked = index == (int)Editor.Mii.MiiFacial.FaceShape;
                button.Color1 = color1;
                button.Color2 = color2;
                button.Color3 = color3;
                button.Click += (_, _) => SetFaceType(index);
            }
        );
    }

    private void CreateSkinColorButtons()
    {
        SetButtons(
            "Color",
            6,
            SkinColorGrid,
            (index, button) =>
            {
                button.IsChecked = index == (int)Editor.Mii.MiiFacial.SkinColor;
                button.Color1 = new SolidColorBrush(SkinColors[(MiiSkinColor)index].Item1);
                button.Color2 = new SolidColorBrush(SkinColors[(MiiSkinColor)index].Item2);
                button.Click += (_, _) => SetSkinColor(index);
            }
        );
    }

    private void SetFaceType(int index)
    {
        var current = Editor.Mii.MiiFacial;
        //face shape is an enum so when comparing
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
            return;

        Editor.Mii.MiiFacial = result.Value;
        Editor.RefreshImage();
    }

    private void SetSkinColor(int index)
    {
        var current = Editor.Mii.MiiFacial;
        //face shape is an enum so when comparing
        if (index == (int)current.FaceShape)
            return; // No change
        var result = MiiFacialFeatures.Create(
            current.FaceShape,
            (MiiSkinColor)index,
            current.FacialFeature,
            current.MingleOff,
            current.Downloaded
        );
        if (result.IsFailure)
            return;

        Editor.Mii.MiiFacial = result.Value;
        Editor.RefreshImage();
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
    }
}
