using MusicDiscRental.Models;
using System;

namespace MusicDiscRental.ViewModels
{
    public class DiscDialogViewModel : ViewModelBase
    {
        private string _dialogTitle;
        private Disc _disc;

        public string DialogTitle
        {
            get => _dialogTitle;
            set => SetProperty(ref _dialogTitle, value);
        }

        public Disc Disc
        {
            get => _disc;
            set => SetProperty(ref _disc, value);
        }

        public DiscDialogViewModel(string title, Disc disc)
        {
            DialogTitle = title;

            // Create a copy of the disc to avoid modifying the original
            Disc = new Disc
            {
                DiscId = disc.DiscId,
                Title = disc.Title,
                Artist = disc.Artist,
                Genre = disc.Genre,
                TotalStock = disc.TotalStock,
                AvailableStock = disc.AvailableStock
            };
        }

        public Disc GetUpdatedDisc()
        {
            // Validate numeric values
            if (Disc.TotalStock <= 0)
                Disc.TotalStock = 1;

            if (Disc.AvailableStock < 0)
                Disc.AvailableStock = 0;

            if (Disc.AvailableStock > Disc.TotalStock)
                Disc.AvailableStock = Disc.TotalStock;

            // Validate and clean text values
            Disc.Title = Disc.Title?.Trim() ?? string.Empty;
            Disc.Artist = Disc.Artist?.Trim() ?? string.Empty;
            Disc.Genre = string.IsNullOrWhiteSpace(Disc.Genre) ? null : Disc.Genre.Trim();

            return Disc;
        }
    }
}