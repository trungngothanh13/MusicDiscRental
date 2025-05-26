using MusicDiscRental.Common;
using MusicDiscRental.Database.DBConnection;
using MusicDiscRental.Database.Repositories;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;
using System.Windows;
using System.Windows.Input;

namespace MusicDiscRental.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private ViewModelBase _currentViewModel;
        private string _title = "Music Disc Rental System";
        private string _connectionStatus;

        public MainViewModel()
        {
            // Choose which database service to use
            IDatabaseService dbService;

            try
            {
                // Create the Oracle service with the correct connection string
                var oracleDbService = new OracleDatabaseService();
                oracleDbService.SetConnectionString("User Id=C##MUSICADMIN;Password=music_password;Data Source=localhost:1521/orcl;");

                bool isConnected = oracleDbService.TestConnection();

                if (isConnected)
                {
                    dbService = oracleDbService;
                    ConnectionStatus = "Connected to Oracle Database";
                }
                else
                {
                    // Fall back to mock if connection fails
                    dbService = MockDatabaseService.Instance;
                    ConnectionStatus = "Using Mock Database (Connection failed)";
                }
            }
            catch (Exception ex)
            {
                // Fall back to mock if there's any error
                dbService = MockDatabaseService.Instance;
                ConnectionStatus = $"Using Mock Database";

                // Keep just one simple error message
                MessageBox.Show("Could not connect to Oracle database. Using mock data instead.",
                                "Connection Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
            }

            // Create repositories with the selected service
            var discRepository = new DiscRepository(dbService);
            var memberRepository = new MemberRepository(dbService);
            var rentalRepository = new RentalRepository(dbService);

            // Create view models
            DiscViewModel = new DiscViewModel(discRepository);
            RentalViewModel = new RentalViewModel(rentalRepository, discRepository, memberRepository);

            // Set initial view
            CurrentViewModel = RentalViewModel;

            // Commands
            ShowDiscsCommand = new RelayCommand(_ => ShowDiscs());
            ShowRentalsCommand = new RelayCommand(_ => ShowRentals());
        }

        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                if (_currentViewModel != value)
                {
                    _currentViewModel = value;
                    OnPropertyChanged();

                    // Notify the new view model that it's been activated
                    _currentViewModel?.OnActivated();
                }
            }
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string ConnectionStatus
        {
            get => _connectionStatus;
            set => SetProperty(ref _connectionStatus, value);
        }

        public DiscViewModel DiscViewModel { get; }
        public RentalViewModel RentalViewModel { get; }

        public ICommand ShowDiscsCommand { get; }
        public ICommand ShowRentalsCommand { get; }

        private void ShowDiscs()
        {
            CurrentViewModel = DiscViewModel;
        }

        private void ShowRentals()
        {
            CurrentViewModel = RentalViewModel; 
        }
    }
}