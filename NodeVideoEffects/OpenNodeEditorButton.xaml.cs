using System.Windows;
using YukkuriMovieMaker.Commons;

namespace NodeVideoEffects
{
    /// <summary>
    /// Interaction logic for ManageNodes.xaml
    /// </summary>
    public partial class OpenNodeEditorButton : IPropertyEditorControl2
    {
        public event EventHandler? BeginEdit;
        public event EventHandler? EndEdit;

        public ItemProperty[]? ItemProperties { get; set; }
        public OpenNodeEditorButton()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (ItemProperties is null)
                throw new InvalidOperationException("ItemProperties is not set.");
            if (((NodeVideoEffectsPlugin)ItemProperties[0].Item).Window != null)
                return;
            BeginEdit?.Invoke(this, EventArgs.Empty);

            var window = ((NodeVideoEffectsPlugin)ItemProperties[0].Item).Window = new NodeEditor
            {
                Owner = Window.GetWindow(this),
                Nodes = ((NodeVideoEffectsPlugin)ItemProperties[0].Item).Nodes.Select(item => item.DeepCopy()).ToList(),
                ItemId = ((NodeVideoEffectsPlugin)ItemProperties[0].Item).Id
            };
            var parentWindow = Window.GetWindow(this);
            if (parentWindow != null) window.CommandBindings.AddRange(parentWindow.CommandBindings);
            window.Show();

            window.NodesUpdated += (_, _) =>
            {
                BeginEdit?.Invoke(this, EventArgs.Empty);
                ((NodeVideoEffectsPlugin)ItemProperties[0].Item).EditorNodes = window.Nodes;
                EndEdit?.Invoke(this, EventArgs.Empty);
            };

            window.Closed += (_, _) =>
            {
                if (ItemProperties != null)
                    ((NodeVideoEffectsPlugin)ItemProperties[0].Item).Window = null;
            };

            EndEdit?.Invoke(this, EventArgs.Empty);
        }

        public void SetEditorInfo(IEditorInfo info)
        {
        }
    }
}
