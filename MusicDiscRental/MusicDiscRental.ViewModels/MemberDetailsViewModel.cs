using MusicDiscRental.Common;
using MusicDiscRental.Database.Repositories;
using MusicDiscRental.Models;
using MusicDiscRental.Views;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MusicDiscRental.ViewModels
{
    public class MemberDetailsViewModel : ViewModelBase
    {
        private readonly MemberRepository _memberRepository;
        private ObservableCollection<Member> _members;
        private Member _selectedMember;
        private string _searchText;
        private bool _isLoading;
        private string _statusMessage;

        public MemberDetailsViewModel(MemberRepository memberRepository)
        {
            _memberRepository = memberRepository;
            Members = new ObservableCollection<Member>();

            // Commands
            AddMemberCommand = new RelayCommand(_ => AddMember(), _ => CanAddMember());
            EditMemberCommand = new RelayCommand(_ => EditMember(), _ => CanEditMember());
            DeleteMemberCommand = new RelayCommand(_ => DeleteMember(), _ => CanDeleteMember());
            SearchCommand = new RelayCommand(_ => Search());
            ClearSearchCommand = new RelayCommand(_ => ClearSearch());

            // Initial load
            LoadMembersAsync();
        }

        public ObservableCollection<Member> Members
        {
            get => _members;
            set => SetProperty(ref _members, value);
        }

        public Member SelectedMember
        {
            get => _selectedMember;
            set
            {
                SetProperty(ref _selectedMember, value);
                ((RelayCommand)EditMemberCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DeleteMemberCommand).RaiseCanExecuteChanged();
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

        public ICommand AddMemberCommand { get; }
        public ICommand EditMemberCommand { get; }
        public ICommand DeleteMemberCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand ClearSearchCommand { get; }

        private async void LoadMembersAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Loading members...";

                var members = await _memberRepository.GetAllAsync();
                Members.Clear();

                foreach (var member in members)
                {
                    Members.Add(member);
                }

                StatusMessage = $"Loaded {Members.Count} members";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading members: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void AddMember()
        {
            try
            {
                StatusMessage = "Adding new member...";

                // Create a new member with default values
                var newMember = new Member
                {
                    Name = "",
                    PhoneNumber = "",
                    JoinDate = DateTime.Now
                };

                // Show dialog to get user input
                var dialogViewModel = new MemberDialogViewModel("Add New Member", newMember);
                var dialog = new MemberDialogView { DataContext = dialogViewModel };

                bool? result = dialog.ShowDialog();

                if (result == true)
                {
                    // Get the updated member from dialog
                    var memberToAdd = dialogViewModel.GetUpdatedMember();

                    // Validate input
                    if (string.IsNullOrWhiteSpace(memberToAdd.Name))
                    {
                        StatusMessage = "Error: Member name is required.";
                        return;
                    }

                    var success = await _memberRepository.AddAsync(memberToAdd);

                    if (success)
                    {
                        StatusMessage = "Member added successfully";
                        LoadMembersAsync();
                    }
                    else
                    {
                        StatusMessage = "Failed to add member";
                    }
                }
                else
                {
                    StatusMessage = "Member addition canceled";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error adding member: {ex.Message}";
            }
        }

        private async void EditMember()
        {
            if (SelectedMember == null)
                return;

            try
            {
                StatusMessage = "Editing member...";

                // Show dialog to get user input
                var dialogViewModel = new MemberDialogViewModel("Edit Member", SelectedMember);
                var dialog = new MemberDialogView { DataContext = dialogViewModel };

                bool? result = dialog.ShowDialog();

                if (result == true)
                {
                    // Get the updated member
                    var updatedMember = dialogViewModel.GetUpdatedMember();

                    // Validate input
                    if (string.IsNullOrWhiteSpace(updatedMember.Name))
                    {
                        StatusMessage = "Error: Member name is required.";
                        return;
                    }

                    var success = await _memberRepository.UpdateAsync(updatedMember);

                    if (success)
                    {
                        StatusMessage = "Member updated successfully";
                        LoadMembersAsync();
                    }
                    else
                    {
                        StatusMessage = "Failed to update member";
                    }
                }
                else
                {
                    StatusMessage = "Member update canceled";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error updating member: {ex.Message}";
            }
        }

        private async void DeleteMember()
        {
            if (SelectedMember == null)
                return;

            try
            {
                // Show confirmation dialog
                var result = MessageBox.Show(
                    $"Are you sure you want to delete member '{SelectedMember.Name}'?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    StatusMessage = "Deleting member...";

                    var success = await _memberRepository.DeleteAsync(SelectedMember.MemberId);

                    if (success)
                    {
                        StatusMessage = "Member deleted successfully";
                        Members.Remove(SelectedMember);
                        SelectedMember = null;
                    }
                    else
                    {
                        StatusMessage = "Failed to delete member";
                    }
                }
                else
                {
                    StatusMessage = "Member deletion canceled";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error deleting member: {ex.Message}";
            }
        }

        private void Search()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                LoadMembersAsync();
                return;
            }

            // Simple search filtering
            var searchText = SearchText.ToLower();
            var filteredMembers = new ObservableCollection<Member>();

            foreach (var member in Members)
            {
                if (member.Name.ToLower().Contains(searchText) ||
                    member.PhoneNumber?.ToLower().Contains(searchText) == true)
                {
                    filteredMembers.Add(member);
                }
            }

            Members = filteredMembers;
            StatusMessage = $"Found {Members.Count} matching members";
        }

        private void ClearSearch()
        {
            SearchText = string.Empty;
            LoadMembersAsync();
        }

        public override void OnActivated()
        {
            base.OnActivated();
            LoadMembersAsync();
        }

        private bool CanAddMember()
        {
            return true;
        }

        private bool CanEditMember()
        {
            return SelectedMember != null;
        }

        private bool CanDeleteMember()
        {
            return SelectedMember != null;
        }
    }
}