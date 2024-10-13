using System.Windows;
using System.Windows.Controls;
using YukkuriMovieMaker.Commons;

namespace NodeVideoEffects
{
    /// <summary>
    /// Interaction logic for ManageNodes.xaml
    /// </summary>
    public partial class OpenNodeEditorButton : UserControl, IPropertyEditorControl2
    {
        public event EventHandler? BeginEdit;
        public event EventHandler? EndEdit;
        IEditorInfo? editorInfo;

        public ItemProperty[]? ItemProperties { get; set; }
        public OpenNodeEditorButton()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (ItemProperties is null)
                throw new InvalidOperationException("ItemProperties is not set.");
            if (((NodeVideoEffectsPlugin)ItemProperties[0].Item).window != null)
                return;
            BeginEdit?.Invoke(this, EventArgs.Empty);

            var window = ((NodeVideoEffectsPlugin)ItemProperties[0].Item).window = new NodeEditor
            {
                Owner = Window.GetWindow(this),
                Nodes = ((NodeVideoEffectsPlugin)ItemProperties[0].Item).Nodes.Select(item => item with { }).ToList(),
                ItemID = ((NodeVideoEffectsPlugin)ItemProperties[0].Item).ID
            };
            var parentWindow = Window.GetWindow(this);
            window.CommandBindings.AddRange(parentWindow.CommandBindings);
            window.Show();

            window.NodesUpdated += (s, e) =>
            {
                BeginEdit?.Invoke(this, EventArgs.Empty);
                ((NodeVideoEffectsPlugin)ItemProperties[0].Item).EditorNodes = window.Nodes;
                EndEdit?.Invoke(this, EventArgs.Empty);
            };

            window.Closed += (s, e) => ((NodeVideoEffectsPlugin)ItemProperties[0].Item).window = null;

            EndEdit?.Invoke(this, EventArgs.Empty);
        }

        public void SetEditorInfo(IEditorInfo info)
        {
            editorInfo = info;
        }
    }
}
