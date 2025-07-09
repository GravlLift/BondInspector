using System.Windows.Controls;

namespace BondInspector
{
    /// <summary>
    /// Interaction logic for BondInspector.xaml
    /// </summary>
    public partial class BondInspectorView : UserControl
    {
        public BondInspectorView()
        {
            InitializeComponent();
            ViewModel = new ViewModel();
            DataContext = ViewModel;
        }

        public ViewModel ViewModel { get; }
    }
}
