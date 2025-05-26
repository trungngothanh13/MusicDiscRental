using System.ComponentModel;
using System.Runtime.CompilerServices;
using MusicDiscRental.Models;
using MusicDiscRental.Database.Repositories;

namespace MusicDiscRental.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        // Add this method to refresh data when the view becomes active
        public virtual void OnActivated() { }
    }
}