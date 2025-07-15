using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using WheelWizard.Views.Popups.Base;

namespace WheelWizard.Views.Popups;

public partial class Test3DWindow : PopupContent
{
    public Test3DWindow()
        : base(allowClose: true, allowParentInteraction: true, isTopMost: true, title: "3D Test Window")
    {
        InitializeComponent();
        // No event subscription needed; rendering is handled in MyOpenGlControl
    }
}

public class MyOpenGlControl : OpenGlControlBase
{
    protected override void OnOpenGlRender(GlInterface gl, int fb)
    {
        gl.ClearColor(1.2f, 0.3f, 0.3f, 1.0f);
        gl.Clear(0x4100); // GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT
    }
}
