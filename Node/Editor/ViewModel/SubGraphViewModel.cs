using System.Windows.Input;
using Node.Graph;
using Node.Graph.Events;

namespace Node.Editor.ViewModel;

public sealed class SubGraphViewModel : IDisposable
{
    private readonly EventHandler<CommittedEventArgs> _committedHandler;
    private readonly NodeGraph _graph;
    private readonly EventHandler<GraphChangedEventArgs> _graphChangedHandler;
    private bool _isDisposed;

    public SubGraphViewModel(
        string name,
        string label,
        string description,
        NodeGraph graph)
    {
        Name = name;
        Label = label;
        Description = description;
        _graph = graph;
        _graphChangedHandler = (_, args) => OnGraphChangedCommand?.Execute(args);
        _committedHandler = (_, _) => OnGraphCommitedCommand?.Execute(null);
        _graph.GraphChanged += _graphChangedHandler;
        _graph.Committed += _committedHandler;
    }

    public string Name { get; }
    public string Label { get; }
    public string Description { get; }

    public ICommand? OpenSubGraphCommand { get; init; }
    public ICommand? OnGraphChangedCommand { get; init; }
    public ICommand? OnGraphCommitedCommand { get; init; }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        _graph.GraphChanged -= _graphChangedHandler;
        _graph.Committed -= _committedHandler;
    }
}