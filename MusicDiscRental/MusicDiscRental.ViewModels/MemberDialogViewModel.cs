using MusicDiscRental.Models;
using MusicDiscRental.ViewModels;
using System;

namespace MusicDiscRental.ViewModels
{
    public class MemberDialogViewModel : ViewModelBase
    {
        private string _dialogTitle;
        private Member _member;

        public string DialogTitle
        {
            get => _dialogTitle;
            set => SetProperty(ref _dialogTitle, value);
        }

        public Member Member
        {
            get => _member;
            set => SetProperty(ref _member, value);
        }

        public MemberDialogViewModel(string title, Member member)
        {
            DialogTitle = title;

            // Create a copy of the member to avoid modifying the original
            Member = new Member
            {
                MemberId = member.MemberId,
                Name = member.Name,
                PhoneNumber = member.PhoneNumber,
                JoinDate = member.JoinDate
            };
        }

        public Member GetUpdatedMember()
        {
            // Validate and clean text values
            Member.Name = Member.Name?.Trim() ?? string.Empty;
            Member.PhoneNumber = Member.PhoneNumber?.Trim() ?? string.Empty;

            return Member;
        }
    }
}