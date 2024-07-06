using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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

        private void Button_Click(object sender, RoutedEventArgs e) {
            if (editorInfo is null)
                throw new InvalidOperationException("EditorInfo is not set.");
            if (ItemProperties is null)
                throw new InvalidOperationException("ItemProperties is not set.");
            BeginEdit?.Invoke(this, EventArgs.Empty);

            var window = new NodeEditor
            {
                Owner = Window.GetWindow(this),
            };
            window.Show();

            EndEdit?.Invoke(this, EventArgs.Empty);
        }

        public void SetEditorInfo(IEditorInfo info)
        {
            editorInfo = info;
        }
    }
}
