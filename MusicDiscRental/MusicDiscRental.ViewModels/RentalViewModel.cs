using MusicDiscRental.Common;
using MusicDiscRental.Database.Repositories;
using MusicDiscRental.Models;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MusicDiscRental.ViewModels
{
    public class RentalViewModel : ViewModelBase
    {
        private readonly RentalRepository _rentalRepository;
        private readonly DiscRepository _discRepository;
        private readonly MemberRepository _memberRepository;

        private ObservableCollection<Rental> _rentals;
        private ObservableCollection<Disc> _availableDiscs;
        private ObservableCollection<Member> _members;

        private Rental _selectedRental;
        private Disc _selectedDisc;
        private Member _selectedMember;

        private bool _isLoading;
        private bool _showOnlyActive;
        private string _statusMessage;

        public RentalViewModel(
            RentalRepository rentalRepository,
            DiscRepository discRepository,
            MemberRepository memberRepository)
        {
            _rentalRepository = rentalRepository;
            _discRepository = discRepository;
            _memberRepository = memberRepository;

            Rentals = new ObservableCollection<Rental>();
            AvailableDiscs = new ObservableCollection<Disc>();
            Members = new ObservableCollection<Member>();

            // Commands
            LoadRentalsCommand = new RelayCommand(_ => LoadRentalsAsync());
            LoadDiscsCommand = new RelayCommand(_ => LoadDiscsAsync());
            LoadMembersCommand = new RelayCommand(_ => LoadMembersAsync());
            RentDiscCommand = new RelayCommand(_ => RentDisc(), _ => CanRentDisc());
            ReturnDiscCommand = new RelayCommand(_ => ReturnDisc(), _ => CanReturnDisc());
            ShowActiveOnlyCommand = new RelayCommand(_ => ToggleActiveOnly());

            // Initial load
            LoadRentalsAsync();
            LoadDiscsAsync();
            LoadMembersAsync();
        }

        public ObservableCollection<Rental> Rentals
        {
            get => _rentals;
            set => SetProperty(ref _rentals, value);
        }

        public ObservableCollection<Disc> AvailableDiscs
        {
            get => _availableDiscs;
            set => SetProperty(ref _availableDiscs, value);
        }

        public ObservableCollection<Member> Members
        {
            get => _members;
            set => SetProperty(ref _members, value);
        }

        public Rental SelectedRental
        {
            get => _selectedRental;
            set
            {
                SetProperty(ref _selectedRental, value);
                ((RelayCommand)ReturnDiscCommand).RaiseCanExecuteChanged();
            }
        }

        public Disc SelectedDisc
        {
            get => _selectedDisc;
            set
            {
                SetProperty(ref _selectedDisc, value);
                ((RelayCommand)RentDiscCommand).RaiseCanExecuteChanged();
            }
        }

        public Member SelectedMember
        {
            get => _selectedMember;
            set
            {
                SetProperty(ref _selectedMember, value);
                ((RelayCommand)RentDiscCommand).RaiseCanExecuteChanged();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public bool ShowOnlyActive
        {
            get => _showOnlyActive;
            set
            {
                SetProperty(ref _showOnlyActive, value);
                LoadRentalsAsync();
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public ICommand LoadRentalsCommand { get; }
        public ICommand LoadDiscsCommand { get; }
        public ICommand LoadMembersCommand { get; }
        public ICommand RentDiscCommand { get; }
        public ICommand ReturnDiscCommand { get; }
        public ICommand ShowActiveOnlyCommand { get; }

        private async void LoadRentalsAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Loading rentals...";

                var rentals = ShowOnlyActive
                    ? await _rentalRepository.GetActiveRentalsAsync()
                    : await _rentalRepository.GetAllAsync();

                Rentals.Clear();

                foreach (var rental in rentals)
                {
                    Rentals.Add(rental);
                }

                StatusMessage = $"Loaded {Rentals.Count} rentals";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading rentals: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void LoadDiscsAsync()
        {
            try
            {
                var discs = await _discRepository.GetAllAsync();
                AvailableDiscs.Clear();

                foreach (var disc in discs)
                {
                    if (disc.AvailableStock > 0)
                    {
                        AvailableDiscs.Add(disc);
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading discs: {ex.Message}";
            }
        }

        private async void LoadMembersAsync()
        {
            try
            {
                var members = await _memberRepository.GetAllAsync();
                Members.Clear();

                foreach (var member in members)
                {
                    Members.Add(member);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading members: {ex.Message}";
            }
        }

        private async void RentDisc()
        {
            if (SelectedDisc == null || SelectedMember == null)
                return;

            try
            {
                StatusMessage = "Renting disc...";

                var success = await _rentalRepository.RentDiscAsync(
                    SelectedMember.MemberId, SelectedDisc.DiscId);

                if (success)
                {
                    StatusMessage = "Disc rented successfully";

                    // Refresh data
                    LoadRentalsAsync();
                    LoadDiscsAsync();

                    // Clear selections
                    SelectedDisc = null;
                    SelectedMember = null;
                }
                else
                {
                    StatusMessage = "Failed to rent disc";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error renting disc: {ex.Message}";
            }
        }

        private async void ReturnDisc()
        {
            if (SelectedRental == null)
                return;

            try
            {
                StatusMessage = "Returning disc...";

                var success = await _rentalRepository.ReturnDiscAsync(SelectedRental.RentalId);

                if (success)
                {
                    StatusMessage = "Disc returned successfully";

                    // Refresh data
                    LoadRentalsAsync();
                    LoadDiscsAsync();

                    // Clear selection
                    SelectedRental = null;
                }
                else
                {
                    StatusMessage = "Failed to return disc";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error returning disc: {ex.Message}";
            }
        }

        public override void OnActivated()
        {
            base.OnActivated();

            // Reload all data when the view becomes active
            LoadRentalsAsync();
            LoadDiscsAsync();
            LoadMembersAsync();
        }

        private void ToggleActiveOnly()
        {
            ShowOnlyActive = !ShowOnlyActive;
        }

        private bool CanRentDisc()
        {
            return SelectedDisc != null && SelectedMember != null;
        }

        private bool CanReturnDisc()
        {
            return SelectedRental != null && !SelectedRental.IsReturned;
        }
    }
}