using System.Windows;
using System.Windows.Input;

namespace ParfumAdmin_WPF.Views
{
    public partial class ConfirmDialog : Window
    {
        public ConfirmDialog(string title, string message)
        {
            InitializeComponent();
            TitleText.Text    = title;
            MessageText.Text  = message;
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        // Lehetővé teszi az ablak húzását a Border fogásával.
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
    }
}
