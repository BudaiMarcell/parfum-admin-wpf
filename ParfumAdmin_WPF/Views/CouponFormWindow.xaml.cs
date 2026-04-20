using System.Windows;
using ParfumAdmin_WPF.ViewModels;

namespace ParfumAdmin_WPF.Views
{
    public partial class CouponFormWindow : Window
    {
        private readonly CouponFormViewModel _viewModel;

        public CouponFormWindow(CouponFormViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;

            _viewModel.OnRequestClose += HandleRequestClose;
        }

        private void HandleRequestClose(bool success)
        {
            DialogResult = success;
            Close();
        }

        protected override void OnClosed(System.EventArgs e)
        {
            _viewModel.OnRequestClose -= HandleRequestClose;
            base.OnClosed(e);
        }
    }
}
