using System.Windows.Media;
using Node.Editor.Attributes;
using Node.Graph;
using Node.Graph.Attributes;
using Node.Localize;
using Node.ValueTypes;
using Vortice.Direct2D1;
using Vortice.Direct2D1.Effects;

namespace Node.Nodes.Effect.Composition;

[Node(typeof(CompositionCategory), nameof(TextNode.AlphaNode), nameof(TextNode.AlphaNodeDescription), typeof(TextNode))]
public class Alpha : NodeLogic
{
    private ID2D1Image? _effectOutput;
    private Opacity? _opacityEffect;

    [InputPort(nameof(TextNode.InputImagePortLabel), nameof(TextNode.AlphaInputDescription), typeof(TextNode))]
    [PortColorSetting(nameof(Colors.CornflowerBlue))]
    public ImageWrapper? InputImage
    {
        get => GetInput<ImageWrapper>();
        set => SetInput(value);
    }

    [InputPort(nameof(TextNode.OpacityLabel), nameof(TextNode.OpacityDescription), typeof(TextNode))]
    [NumberPortControl(Min = 0f, Max = 100f, Default = 100f, Unit = "%")]
    [PortColorSetting(nameof(Colors.DarkOrange))]
    public float Opacity
    {
        get => GetInput<float>();
        set => SetInput(value);
    }

    [OutputPort(nameof(TextNode.OutputImagePortLabel), nameof(TextNode.AlphaOutputDescription), typeof(TextNode))]
    [PortColorSetting(nameof(Colors.MediumPurple))]
    public ImageWrapper? Output
    {
        get => GetOutput<ImageWrapper>();
        set => SetOutput(value);
    }

    protected override Task Calculate()
    {
        if (EvaluationContext is null) return Task.FromException(new NullReferenceException(nameof(EvaluationContext)));
        if (InputImage?.Image is null || InputImage.Image.NativePointer == nint.Zero)
            return Task.FromException(new NullReferenceException(nameof(InputImage)));

        _opacityEffect ??= new Opacity(EvaluationContext.Devices.DeviceContext);
        _opacityEffect.Value = Opacity / 100f;
        _opacityEffect.SetInput(0, InputImage.Image, true);
        _effectOutput?.Dispose();
        _effectOutput = _opacityEffect.Output;
        Output = new ImageWrapper { Image = _effectOutput };

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _effectOutput?.Dispose();
        _effectOutput = null;
        _opacityEffect?.Dispose();
        _opacityEffect = null;
        base.Dispose();
    }
}