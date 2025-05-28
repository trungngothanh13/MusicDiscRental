using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MusicDiscRental.Models
{
    public class Rental : INotifyPropertyChanged
    {
        private int _rentalId;
        private int _discId;
        private int _memberId;
        private DateTime _rentalDate;
        private DateTime? _returnDate;

        // Navigation properties
        private Disc _disc;
        private Member _member;

        public int RentalId
        {
            get => _rentalId;
            set
            {
                if (_rentalId != value)
                {
                    _rentalId = value;
                    OnPropertyChanged();
                }
            }
        }

        public int DiscId
        {
            get => _discId;
            set
            {
                if (_discId != value)
                {
                    _discId = value;
                    OnPropertyChanged();
                }
            }
        }

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

        public DateTime RentalDate
        {
            get => _rentalDate;
            set
            {
                if (_rentalDate != value)
                {
                    _rentalDate = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DaysRented));
                }
            }
        }

        public DateTime? ReturnDate
        {
            get => _returnDate;
            set
            {
                if (_returnDate != value)
                {
                    _returnDate = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsReturned));
                    OnPropertyChanged(nameof(Status));
                    OnPropertyChanged(nameof(DaysRented));
                }
            }
        }

        public Disc Disc
        {
            get => _disc;
            set
            {
                if (_disc != value)
                {
                    _disc = value;
                    OnPropertyChanged();
                }
            }
        }

        public Member Member
        {
            get => _member;
            set
            {
                if (_member != value)
                {
                    _member = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsReturned => ReturnDate.HasValue;

        public string Status => IsReturned ? "Returned" : "Currently Rented";

        public int DaysRented
        {
            get
            {
                var endDate = ReturnDate ?? DateTime.Now;
                return (int)(endDate - RentalDate).TotalDays;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}