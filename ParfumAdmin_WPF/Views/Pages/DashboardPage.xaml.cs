using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using ParfumAdmin_WPF.ViewModels;

namespace ParfumAdmin_WPF.Views.Pages
{
    public partial class DashboardPage : Page
    {
        private readonly DashboardViewModel _viewModel;
        private readonly DispatcherTimer _realtimeTimer;
        private Window? _hostWindow;

        public DashboardPage(DashboardViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;

            // 30s-enként frissíti az "Aktív most" számlálót, hogy élőnek hasson
            // a dashboard anélkül, hogy a teljes adathalmazt újratöltené.
            _realtimeTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(30),
            };
            _realtimeTimer.Tick += async (_, _) => await _viewModel.RefreshActiveSessionsAsync();

            Unloaded += OnUnloaded;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.LoadDataAsync();

            // Pause the heartbeat while the host window is minimized — no
            // point hitting the realtime endpoint when the dashboard isn't
            // visible on screen. This both saves bandwidth and keeps the
            // log clean when the admin steps away.
            _hostWindow = Window.GetWindow(this);
            if (_hostWindow != null)
            {
                _hostWindow.StateChanged += OnHostStateChanged;
            }

            // Start only if the window is currently visible.
            if (_hostWindow?.WindowState != WindowState.Minimized)
            {
                _realtimeTimer.Start();
            }
        }

        private void OnHostStateChanged(object? sender, EventArgs e)
        {
            if (_hostWindow == null) return;

            if (_hostWindow.WindowState == WindowState.Minimized)
            {
                _realtimeTimer.Stop();
            }
            else
            {
                _realtimeTimer.Start();
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _realtimeTimer.Stop();
            if (_hostWindow != null)
            {
                _hostWindow.StateChanged -= OnHostStateChanged;
                _hostWindow = null;
            }
        }

        private void StatBox_Click(object sender, RoutedEventArgs e)
        {
            // Any stat box click navigates to the detailed analytics view.
            if (Window.GetWindow(this) is MainWindow main)
                main.NavigateToPage("Analytics");
        }

        // Háromfázisú rendezés: Növekvő → Csökkentő → Nincs rendezés.
        // Ugyanaz a minta, mint a ProductsPage / OrdersPage / CouponsPage tábláin —
        // ha a felhasználó harmadszor kattint, letörli a rendezést.
        private void DataGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            if (e.Column.SortDirection != ListSortDirection.Descending) return;
            e.Handled = true;
            e.Column.SortDirection = null;
            CollectionViewSource.GetDefaultView(((DataGrid)sender).ItemsSource)
                                ?.SortDescriptions.Clear();
        }
    }
}
