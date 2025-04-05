
using Avalonia;
using Avalonia.Controls;

namespace WheelWizard.Views.BehaviorComponent;

public partial class FeedbackTextBox : UserControl
{
    public FeedbackTextBox()
    {
        InitializeComponent();
    }

    public static readonly StyledProperty<TextBoxVariantType> VariantProperty =
        AvaloniaProperty.Register<FeedbackTextBox, TextBoxVariantType>(nameof(Variant), TextBoxVariantType.Default);
    
    public static readonly StyledProperty<string> ErrorMessageProperty =
        AvaloniaProperty.Register<FeedbackTextBox, string>(nameof(ErrorMessage));
    
    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<FeedbackTextBox, string>(nameof(Text));

    public static readonly StyledProperty<string> WatermarkProperty =
        AvaloniaProperty.Register<FeedbackTextBox, string>(nameof(Watermark), "Enter text here...");
    
    public static readonly StyledProperty<string> LabelProperty =
        AvaloniaProperty.Register<FeedbackTextBox, string>(nameof(Label));

    public static readonly StyledProperty<string> InfoTextProperty =
        AvaloniaProperty.Register<FeedbackTextBox, string>(nameof(InfoText));
    
    public enum TextBoxVariantType
    {
        Default,
        Dark
    }
    public TextBoxVariantType Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }
    
    public string ErrorMessage
    {
        get => GetValue(ErrorMessageProperty);
        set => SetValue(ErrorMessageProperty, value);
    }
    
    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
    
    public string Watermark
    {
        get => GetValue(WatermarkProperty);
        set => SetValue(WatermarkProperty, value);
    }
    
    public string Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }
    
    public string InfoText
    {
        get => GetValue(InfoTextProperty);
        set => SetValue(InfoTextProperty, value);
    }
}

