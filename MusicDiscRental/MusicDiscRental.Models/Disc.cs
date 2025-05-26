using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MusicDiscRental.Models
{
    public class Disc : INotifyPropertyChanged
    {
        private int _discId;
        private string _title;
        private string _artist;
        private string _genre;
        private int _totalStock;
        private int _availableStock;

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

        public string Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Artist
        {
            get => _artist;
            set
            {
                if (_artist != value)
                {
                    _artist = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Genre
        {
            get => _genre;
            set
            {
                if (_genre != value)
                {
                    _genre = value;
                    OnPropertyChanged();
                }
            }
        }

        public int TotalStock
        {
            get => _totalStock;
            set
            {
                if (_totalStock != value)
                {
                    _totalStock = value;
                    OnPropertyChanged();
                }
            }
        }

        public int AvailableStock
        {
            get => _availableStock;
            set
            {
                if (_availableStock != value)
                {
                    _availableStock = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsAvailable => AvailableStock > 0;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}