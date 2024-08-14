using NodeVideoEffects.Type;
using System.Collections.Immutable;
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
            if (editorInfo is null)
                throw new InvalidOperationException("EditorInfo is not set.");
            if (ItemProperties is null)
                throw new InvalidOperationException("ItemProperties is not set.");
            BeginEdit?.Invoke(this, EventArgs.Empty);

            var window = new NodeEditor
            {
                Owner = Window.GetWindow(this),
                Nodes = ((NodeVideoEffectsPlugin)ItemProperties[0].Item).Nodes
            };
            window.Show();

            window.NodesUpdated += (s, e) =>
            {
                BeginEdit?.Invoke(this, EventArgs.Empty);
                ((NodeVideoEffectsPlugin)ItemProperties[0].Item).Nodes = window.Nodes;
            };

            EndEdit?.Invoke(this, EventArgs.Empty);
        }

        public void SetEditorInfo(IEditorInfo info)
        {
            editorInfo = info;
        }
    }
}
