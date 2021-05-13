using SrcToTextConverter.Helpers;
using SrcToTextConverter.Model;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;

namespace SrcToTextConverter.ViewModel
{
    class AddFilterViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<FilterModel> _filters;
        private string _name;
        private string _extensions;
        private int _codePage;

        public AddFilterViewModel(ObservableCollection<FilterModel> filters)
        {
            _filters = filters;

            // Default values;
            Name = string.Empty;
            Extensions = string.Empty;
            CodePage = 1251;
        }

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
        public string Extensions
        {
            get => _extensions;
            set
            {
                _extensions = value;
                OnPropertyChanged(nameof(Extensions));
            }
        }
        public int CodePage
        {
            get => _codePage;
            set
            {
                try
                {
                    Encoding.GetEncoding(value);
                    _codePage = value;
                    OnPropertyChanged(nameof(CodePage));
                }
                catch (Exception)
                {
                    MessageBox.Show("Code Page not supported", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
        }

        public RelayCommand<Window> OkCommand => new RelayCommand<Window>(AddFilter);

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        private void AddFilter(Window window)
        {
            FilterModel filter = new FilterModel(Name, Extensions.Split(" "), CodePage);

            _filters.Add(filter);

            if (window != null)
            {
                window.Close();
            }
        }
    }
}
