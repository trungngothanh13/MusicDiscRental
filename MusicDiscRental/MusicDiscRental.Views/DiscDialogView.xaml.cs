using System.Windows;

namespace MusicDiscRental.Views
{
    public partial class DiscDialogView : Window
    {
        public DiscDialogView()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}