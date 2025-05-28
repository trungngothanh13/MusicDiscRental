using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MusicDiscRental.Models
{
    public class Member : INotifyPropertyChanged
    {
        private int _memberId;
        private string _name;
        private string _phoneNumber;
        private DateTime _joinDate;

        public int MemberId
        {
            get => _memberId;
            set
            {
                if (_memberId != value)
                {
                    _memberId = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        public string PhoneNumber
        {
            get => _phoneNumber;
            set
            {
                if (_phoneNumber != value)
                {
                    _phoneNumber = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime JoinDate
        {
            get => _joinDate;
            set
            {
                if (_joinDate != value)
                {
                    _joinDate = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}