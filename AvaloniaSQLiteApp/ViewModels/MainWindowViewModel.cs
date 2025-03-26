using AvaloniaSQLiteApp.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace AvaloniaSQLiteApp.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, INotifyPropertyChanged
    {
        private ObservableCollection<Mode> _modes = [];
        private ObservableCollection<Step> _steps = [];

        public ObservableCollection<Mode> Modes
        {
            get => _modes;
            set
            {
                if (_modes != value)
                {
                    _modes = value;
                    OnPropertyChanged(nameof(Modes));
                }
            }
        }

        public ObservableCollection<Step> Steps
        {
            get => _steps;
            set
            {
                if (_steps != value)
                {
                    _steps = value;
                    OnPropertyChanged(nameof(Steps));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected new virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
