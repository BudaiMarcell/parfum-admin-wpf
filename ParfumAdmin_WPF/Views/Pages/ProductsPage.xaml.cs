using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Extensions.DependencyInjection;
using ParfumAdmin_WPF.Models;
using ParfumAdmin_WPF.ViewModels;

namespace ParfumAdmin_WPF.Views.Pages
{
    public partial class ProductsPage : Page
    {
        private readonly ProductsViewModel _viewModel;

        public ProductsPage(ProductsViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;

            _viewModel.OnAddProductRequested  += OpenAddProductForm;
            _viewModel.OnEditProductRequested += OpenEditProductForm;
            _viewModel.ConfirmDelete = ConfirmDeleteProduct;
            _viewModel.ConfirmBulkDelete = ConfirmBulkDeleteProducts;

            // Kattintás bárhova az oldalon (DataGriden kívül is) megszünteti a kijelölést.
            this.PreviewMouseLeftButtonDown += Page_PreviewMouseLeftButtonDown;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.LoadProductsAsync();
        }

        private async void IsActive_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox cb && cb.DataContext is Product product)
                await _viewModel.ToggleActiveAsync(product);
        }

        private bool ConfirmDeleteProduct(Product p)
        {
            var dialog = new ConfirmDialog(
                "Termék törlése",
                $"Biztosan törlöd a(z) \"{p.Name}\" terméket?")
            {
                Owner = Window.GetWindow(this)
            };
            return dialog.ShowDialog() == true;
        }

        private bool ConfirmBulkDeleteProducts(int count)
        {
            var dialog = new ConfirmDialog(
                "Tömeges törlés",
                $"Biztosan törlöd a kijelölt {count} terméket?")
            {
                Owner = Window.GetWindow(this)
            };
            return dialog.ShowDialog() == true;
        }

        // A DataGrid SelectedItems nem bindolható, ezért kézzel szinkronizáljuk.
        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _viewModel.SelectedProducts.Clear();
            foreach (var item in ProductsGrid.SelectedItems)
                if (item is Product p) _viewModel.SelectedProducts.Add(p);
        }

        private void OpenAddProductForm()  => _ = ShowProductFormAsync(null);
        private void OpenEditProductForm(Product p) => _ = ShowProductFormAsync(p);

        private async System.Threading.Tasks.Task ShowProductFormAsync(Product? editingProduct)
        {
            var window = App.ServiceProvider.GetRequiredService<ProductFormWindow>();
            var formVm = (ProductFormViewModel)window.DataContext;

            if (editingProduct != null) formVm.InitializeForEdit(editingProduct);
            else                        formVm.InitializeForAdd();

            window.Owner = Window.GetWindow(this);
            if (window.ShowDialog() == true)
                await _viewModel.LoadProductsAsync();
        }

        // Ha a kattintás nem DataGridRow-n történt, töröljük a kijelölést.
        private void Page_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var src = e.OriginalSource as DependencyObject;
            if (FindParent<System.Windows.Controls.Primitives.ButtonBase>(src) != null) return;
            if (FindParent<DataGridRow>(src) == null)
                ProductsGrid.UnselectAll();
        }

        // Háromfázisú rendezés: Növekvő → Csökkentő → Nincs rendezés
        private void DataGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            if (e.Column.SortDirection != ListSortDirection.Descending) return;
            e.Handled = true;
            e.Column.SortDirection = null;
            CollectionViewSource.GetDefaultView(((DataGrid)sender).ItemsSource)
                                ?.SortDescriptions.Clear();
        }

        private static T? FindParent<T>(DependencyObject? child) where T : DependencyObject
        {
            while (child != null)
            {
                if (child is T t) return t;
                child = VisualTreeHelper.GetParent(child);
            }
            return null;
        }
    }
}
