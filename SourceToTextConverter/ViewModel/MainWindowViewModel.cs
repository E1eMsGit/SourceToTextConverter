using SrcToTextConverter.Helpers;
using SrcToTextConverter.Model;
using SrcToTextConverter.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using WinForms = System.Windows.Forms;

namespace SrcToTextConverter.ViewModel
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly string _filesDirectory;

        private string _projectPathText;
        private int _progressBarValue;
        private int _progressBarMaxValue;
        private bool _progressBarIsIndeterminate;
        private int _filterSelectedIndex;
        private ObservableCollection<SourceFileModel> _sourceFiles;
        private ObservableCollection<FilterModel> _filters;
        private string _jsonFileName;

        public MainWindowViewModel()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            _filesDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Source to Text Converter\\";
            _jsonFileName = "filters.json";

            ProgressBarValue = 0;
            ProgressBarMaxValue = 100;
            ProgressBarIsIndeterminate = false;
            FilterSelectedIndex = 0;

            if (!Directory.Exists(_filesDirectory))
            {
                Directory.CreateDirectory(_filesDirectory);
            }      

            if (!File.Exists(_jsonFileName))
            {
                Filters = new ObservableCollection<FilterModel>()
                {
                    new FilterModel("C#", new string[] { ".sln", ".csproj", ".cs" }, 1200),
                    new FilterModel("C++", new string[] { ".brp", ".c", ".h", ".cpp", ".hpp", ".dfm" }, 1251),
                };

                WriteJson();
            }
            else
            {
                Filters = JsonSerializer.Deserialize<ObservableCollection<FilterModel>>(File.ReadAllText(_jsonFileName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<FilterModel> Filters 
        {
            get => _filters;
            set
            {
                _filters = value;
                OnPropertyChanged(nameof(Filters));
            }
        }
        
        public int FilterSelectedIndex 
        { 
            get => _filterSelectedIndex;
            set
            {
                _filterSelectedIndex = value;
                OnPropertyChanged(nameof(FilterSelectedIndex));
                if (ProjectPathText != null)
                {
                    UpdateSourceFiles();
                }               
            }
        }
        
        public ObservableCollection<SourceFileModel> SourceFiles {
            get => _sourceFiles;
            private set
            {
                _sourceFiles = value;
                OnPropertyChanged(nameof(SourceFiles));
            }
        }
        
        public string ProjectPathText
        {
            get => _projectPathText;
            set
            {
                _projectPathText = value;
                OnPropertyChanged(nameof(ProjectPathText));
            }
        }
        
        public int ProgressBarValue
        {
            get => _progressBarValue;
            set
            {
                _progressBarValue = value;
                OnPropertyChanged(nameof(ProgressBarValue));
            }
        }
        
        public int ProgressBarMaxValue
        {
            get => _progressBarMaxValue;
            set
            {
                _progressBarMaxValue = value;
                OnPropertyChanged(nameof(ProgressBarMaxValue));
            }
        }
        
        public bool ProgressBarIsIndeterminate 
        {
            get => _progressBarIsIndeterminate;
            set
            {
                _progressBarIsIndeterminate = value;
                OnPropertyChanged(nameof(ProgressBarIsIndeterminate));
            }
        }

        public RelayCommand ProjectPathEnterPressed => new RelayCommand(action => UpdateSourceFiles());

        public RelayCommand AddFilterCommand => new RelayCommand(action => {
            AddFilterViewModel addFilterViewModel = new AddFilterViewModel(Filters);
            AddFilterWindow addFilterWindow = new AddFilterWindow() { DataContext = addFilterViewModel, Owner = Application.Current.MainWindow };

            addFilterWindow.ShowDialog();
            WriteJson();
        });

        public RelayCommand RemoveFilterCommand => new RelayCommand(action => {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to remove this filter?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                Filters.Remove(Filters[FilterSelectedIndex]);
                FilterSelectedIndex++;
                WriteJson();
            }
        });

        public RelayCommand BrowseCommand => new RelayCommand(action => {
            using (var dialog = new WinForms.FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == WinForms.DialogResult.OK)
                {
                    ProjectPathText = dialog.SelectedPath;

                    UpdateSourceFiles();
                }
            }          
        });

        public RelayCommand CreateTextCommand => new RelayCommand(async action =>
        {
            await Task.Run(() =>
            {
                List<SourceFileModel> selectedSourceFiles = SourceFiles.Where(file => file.IsChecked == true).ToList();
                ProgressBarMaxValue = selectedSourceFiles.Count();
                StringBuilder content = new StringBuilder();
                string[] pathArray = ProjectPathText.Split('\\');
                string fileName = $"Source {pathArray[pathArray.Length - 1]}.txt";
                string filePath = Path.Combine(_filesDirectory, fileName);

                for (int i = 0; i < selectedSourceFiles.Count(); i++)
                {
                    content.AppendLine(new string('*', 40));
                    content.AppendLine($"Файл: {selectedSourceFiles[i].FullFileName}");
                    content.AppendLine();                     
                    content.AppendLine(File.ReadAllText(selectedSourceFiles[i].FullFileName, Filters[FilterSelectedIndex].GetEncoding()));
                    content.AppendLine();
                    content.AppendLine();
                    ProgressBarValue = i + 1;
                }

                ProgressBarValue = 0;

                File.WriteAllText(filePath, content.ToString());
                MessageBox.Show($"File saved to {_filesDirectory}", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                
                ProcessStartInfo processStartInfo = new ProcessStartInfo()
                {
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Normal,
                    FileName = "explorer",
                    Arguments = @"/n, /open, " + filePath,
                };
                Process process = new Process()
                {
                    StartInfo = processStartInfo,
                };
                process.Start();
            });        
        }, canExecute =>  SourceFiles != null && SourceFiles.Count > 0);

        private void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private async void UpdateSourceFiles()
        {
            Func<ObservableCollection<SourceFileModel>> func = () =>
            {
                ObservableCollection<SourceFileModel> sourceFileModels = new ObservableCollection<SourceFileModel>();
                
                if (Directory.Exists(ProjectPathText))
                { 
                    string[] files = Directory.GetFiles(ProjectPathText, "*", SearchOption.AllDirectories);

                    for (int i = 0; i < files.Length; i++)
                    {
                        for (int j = 0; j < Filters[FilterSelectedIndex].Extensions.Length; j++)
                        {
                            if (Path.GetExtension(files[i]) == Filters[FilterSelectedIndex].Extensions[j])
                            {
                                sourceFileModels.Add(new SourceFileModel(files[i]));                                
                            }
                        }
                    }
                }

                return sourceFileModels;
            };

            ProgressBarIsIndeterminate = true;
            
            try
            {          
                await Task.Run(() => func()).ContinueWith(t => { SourceFiles = t.Result; });             
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);                
            }
            
            ProgressBarIsIndeterminate = false;
        }

        private async void WriteJson()
        {
            using FileStream createStream = File.Create(_jsonFileName);
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            await JsonSerializer.SerializeAsync(createStream, Filters, options);          
        }
    }
}
