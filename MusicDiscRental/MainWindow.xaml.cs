using MusicDiscRental.ViewModels;
using System.Windows;

namespace MusicDiscRental
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}