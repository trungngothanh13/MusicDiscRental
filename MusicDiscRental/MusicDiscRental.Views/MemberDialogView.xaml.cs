using System.Windows;

namespace MusicDiscRental.Views
{
    public partial class MemberDialogView : Window
    {
        public MemberDialogView()
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