using Node.Editor.View;
using Node.Editor.ViewModel;
using Node.Localize;
using YukkuriMovieMaker.Plugin;

namespace Node.Editor;

public class NodeEditor : IToolPlugin
{
    public NodeEditor()
    {
        ControlRegistrations.Initialize();
    }

    public string Name => TextUi.Node;
    public Type ViewModelType => typeof(NodeEditorViewModel);
    public Type ViewType => typeof(NodeEditorView);
    public bool AllowMultipleInstances => true;
}