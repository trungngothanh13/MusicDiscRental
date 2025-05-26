using MusicDiscRental.Common;
using MusicDiscRental.Database.Repositories;
using MusicDiscRental.Models;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MusicDiscRental.Views;

namespace MusicDiscRental.ViewModels
{
    public class DiscViewModel : ViewModelBase
    {
        private readonly DiscRepository _discRepository;
        private ObservableCollection<Disc> _discs;
        private Disc _selectedDisc;
        private string _searchText;
        private bool _isLoading;
        private string _statusMessage;

        public DiscViewModel(DiscRepository discRepository)
        {
            _discRepository = discRepository;
            Discs = new ObservableCollection<Disc>();

            // Commands
            LoadDiscsCommand = new RelayCommand(_ => LoadDiscsAsync());
            AddDiscCommand = new RelayCommand(_ => AddDisc(), _ => CanAddDisc());
            UpdateDiscCommand = new RelayCommand(_ => UpdateDisc(), _ => CanUpdateDisc());
            DeleteDiscCommand = new RelayCommand(_ => DeleteDisc(), _ => CanDeleteDisc());
            SearchCommand = new RelayCommand(_ => Search());
            ClearSearchCommand = new RelayCommand(_ => ClearSearch());

            // Initial load
            LoadDiscsAsync();
        }

        public ObservableCollection<Disc> Discs
        {
            get => _discs;
            set => SetProperty(ref _discs, value);
        }

        public Disc SelectedDisc
        {
            get => _selectedDisc;
            set
            {
                SetProperty(ref _selectedDisc, value);
                ((RelayCommand)UpdateDiscCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DeleteDiscCommand).RaiseCanExecuteChanged();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public ICommand LoadDiscsCommand { get; }
        public ICommand AddDiscCommand { get; }
        public ICommand UpdateDiscCommand { get; }
        public ICommand DeleteDiscCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand ClearSearchCommand { get; }

        private async void LoadDiscsAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Loading discs...";

                var discs = await _discRepository.GetAllAsync();
                Discs.Clear();

                foreach (var disc in discs)
                {
                    Discs.Add(disc);
                }

                StatusMessage = $"Loaded {Discs.Count} discs";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading discs: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void AddDisc()
        {
            try
            {
                StatusMessage = "Adding new disc...";

                // Create a new disc with default values
                var newDisc = new Disc
                {
                    Title = "",
                    Artist = "",
                    Genre = "",
                    TotalStock = 1,
                    AvailableStock = 1
                };

                // Show dialog to get user input
                var dialogViewModel = new DiscDialogViewModel("Add New Disc", newDisc);
                var dialog = new DiscDialogView { DataContext = dialogViewModel };

                bool? result = dialog.ShowDialog();

                if (result == true)
                {
                    // Get the updated disc from dialog
                    var discToAdd = dialogViewModel.GetUpdatedDisc();

                    // Validate input
                    if (string.IsNullOrWhiteSpace(discToAdd.Title) || string.IsNullOrWhiteSpace(discToAdd.Artist))
                    {
                        StatusMessage = "Error: Title and Artist are required.";
                        return;
                    }

                    var success = await _discRepository.AddAsync(discToAdd);

                    if (success)
                    {
                        StatusMessage = "Disc added successfully";
                        LoadDiscsAsync();
                    }
                    else
                    {
                        StatusMessage = "Failed to add disc";
                    }
                }
                else
                {
                    StatusMessage = "Disc addition canceled";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error adding disc: {ex.Message}";
            }
        }

        private async void UpdateDisc()
        {
            if (SelectedDisc == null)
                return;

            try
            {
                StatusMessage = "Updating disc...";

                // Show dialog to get user input
                var dialogViewModel = new DiscDialogViewModel("Edit Disc", SelectedDisc);
                var dialog = new DiscDialogView { DataContext = dialogViewModel };

                bool? result = dialog.ShowDialog();

                if (result == true)
                {
                    // Get the updated disc
                    var updatedDisc = dialogViewModel.GetUpdatedDisc();

                    // Validate input
                    if (string.IsNullOrWhiteSpace(updatedDisc.Title) || string.IsNullOrWhiteSpace(updatedDisc.Artist))
                    {
                        StatusMessage = "Error: Title and Artist are required.";
                        return;
                    }

                    var success = await _discRepository.UpdateAsync(updatedDisc);

                    if (success)
                    {
                        StatusMessage = "Disc updated successfully";
                        LoadDiscsAsync();
                    }
                    else
                    {
                        StatusMessage = "Failed to update disc";
                    }
                }
                else
                {
                    StatusMessage = "Disc update canceled";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error updating disc: {ex.Message}";
            }
        }

        private async void DeleteDisc()
        {
            if (SelectedDisc == null)
                return;

            try
            {
                // Show confirmation dialog
                var result = MessageBox.Show(
                    $"Are you sure you want to delete the disc '{SelectedDisc.Title}'?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    StatusMessage = "Deleting disc...";

                    try
                    {
                        var success = await _discRepository.DeleteAsync(SelectedDisc.DiscId);

                        if (success)
                        {
                            StatusMessage = "Disc deleted successfully";
                            Discs.Remove(SelectedDisc);
                            SelectedDisc = null;
                        }
                        else
                        {
                            StatusMessage = "Failed to delete disc";
                        }
                    }
                    catch (InvalidOperationException ex)
                    {
                        // This catches the specific exception for discs that are referenced in rentals
                        MessageBox.Show(ex.Message, "Cannot Delete", MessageBoxButton.OK, MessageBoxImage.Warning);
                        StatusMessage = "Cannot delete: Disc is referenced in rental records";
                    }
                }
                else
                {
                    StatusMessage = "Disc deletion canceled";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error deleting disc: {ex.Message}";
            }
        }
        private void Search()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                LoadDiscsAsync();
                return;
            }

            // Simple search filtering (in a real app, this would be more sophisticated)
            var searchText = SearchText.ToLower();
            var filteredDiscs = new ObservableCollection<Disc>();

            foreach (var disc in Discs)
            {
                if (disc.Title.ToLower().Contains(searchText) ||
                    disc.Artist.ToLower().Contains(searchText) ||
                    disc.Genre?.ToLower().Contains(searchText) == true)
                {
                    filteredDiscs.Add(disc);
                }
            }

            Discs = filteredDiscs;
            StatusMessage = $"Found {Discs.Count} matching discs";
        }

        private void ClearSearch()
        {
            SearchText = string.Empty;
            LoadDiscsAsync();
        }

        private bool CanAddDisc()
        {
            return true; // You might add conditions here
        }

        private bool CanUpdateDisc()
        {
            return SelectedDisc != null;
        }

        private bool CanDeleteDisc()
        {
            return SelectedDisc != null;
        }
    }
}