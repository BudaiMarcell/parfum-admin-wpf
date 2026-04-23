using System.Windows;
using System.Windows.Controls;
using ParfumAdmin_WPF.ViewModels;

namespace ParfumAdmin_WPF.Views.Pages
{
    public partial class AuditLogsPage : Page
    {
        private readonly AuditLogsViewModel _viewModel;

        public AuditLogsPage(AuditLogsViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.LoadLogsAsync();
        }
    }
}
