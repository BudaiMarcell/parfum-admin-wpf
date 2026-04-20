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
    public partial class CouponsPage : Page
    {
        private readonly CouponsViewModel _viewModel;

        public CouponsPage(CouponsViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;

            _viewModel.OnAddCouponRequested  += OpenAddCouponForm;
            _viewModel.OnEditCouponRequested += OpenEditCouponForm;
            _viewModel.ConfirmDelete = ConfirmDeleteCoupon;

            this.PreviewMouseLeftButtonDown += Page_PreviewMouseLeftButtonDown;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.LoadCouponsAsync();
        }

        private bool ConfirmDeleteCoupon(Coupon c)
        {
            var dialog = new ConfirmDialog(
                "Kupon törlése",
                $"Biztosan törlöd a(z) \"{c.CouponCode}\" kupont?")
            {
                Owner = Window.GetWindow(this)
            };
            return dialog.ShowDialog() == true;
        }

        private void OpenAddCouponForm()         => _ = ShowCouponFormAsync(null);
        private void OpenEditCouponForm(Coupon c) => _ = ShowCouponFormAsync(c);

        private async System.Threading.Tasks.Task ShowCouponFormAsync(Coupon? editingCoupon)
        {
            var window = App.ServiceProvider.GetRequiredService<CouponFormWindow>();
            var formVm = (CouponFormViewModel)window.DataContext;

            if (editingCoupon != null) formVm.InitializeForEdit(editingCoupon);
            else                       formVm.InitializeForAdd();

            window.Owner = Window.GetWindow(this);
            if (window.ShowDialog() == true)
                await _viewModel.LoadCouponsAsync();
        }

        private void Page_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var src = e.OriginalSource as DependencyObject;
            // Gombokra / sorra kattintva NE töröljük a kijelölést.
            if (FindParent<System.Windows.Controls.Primitives.ButtonBase>(src) != null) return;
            if (FindParent<DataGridRow>(src) == null)
                _viewModel.SelectedCoupon = null;
        }

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
