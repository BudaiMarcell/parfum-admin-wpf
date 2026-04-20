using System.Windows;
using ParfumAdmin_WPF.ViewModels;

namespace ParfumAdmin_WPF.Views
{
    public partial class ProductFormWindow : Window
    {
        private readonly ProductFormViewModel _viewModel;

        public ProductFormWindow(ProductFormViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;

            _viewModel.OnRequestClose += HandleRequestClose;
            // A kategóriákat az ablak megnyitása után töltjük be (akkor is,
            // ha előtte InitializeForEdit-et hívtunk, hogy a kiválasztás beálljon).
            Loaded += async (_, _) => await _viewModel.LoadCategoriesAsync();
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
