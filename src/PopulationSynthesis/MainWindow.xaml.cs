/*
    Copyright 2021 Travel Modelling Group, Department of Civil Engineering, University of Toronto

    This file is part of V4.0PopulationSynthesis.

    V4.0PopulationSynthesis is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    V4.0PopulationSynthesis is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with V4.0PopulationSynthesis.  If not, see <http://www.gnu.org/licenses/>.
*/
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using Ookii.Dialogs.Wpf;

namespace PopulationSynthesis;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public class ConfigurationModel : INotifyPropertyChanged
    {
        string _populationForecastFile = String.Empty;
        string _zoneSystemFile = String.Empty;
        string _inputPopulationDirectory = String.Empty;
        string _outputDirectory = String.Empty;
        int _randomSeed = 12345;

        public string PopulationForecastFile
        {
            get => _populationForecastFile;
            set
            {
                _populationForecastFile = value;
                InvokePropertyChanged();
            }
        }

        public string ZoneSystemFile
        {
            get => _zoneSystemFile;
            set
            {
                _zoneSystemFile = value;
                InvokePropertyChanged();
            }
        }

        public string InputPopulationDirectory
        {
            get => _inputPopulationDirectory;
            set
            {
                _inputPopulationDirectory = value;
                InvokePropertyChanged();
            }
        }

        public string OutputDirectory
        {
            get => _outputDirectory;
            set
            {
                _outputDirectory = value;
                InvokePropertyChanged();
            }
        }

        public string RandomSeed
        {
            get => _randomSeed.ToString();
            set
            {
                if(int.TryParse(value, out var randomSeed))
                {
                    _randomSeed = randomSeed;
                    InvokePropertyChanged();
                }
            }
        }

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Let the GUI know that we have a change to propagate out to bindings.
        /// </summary>
        /// <param name="methodName">Leave this blank to automatically be filled out.</param>
        private void InvokePropertyChanged([CallerMemberName] string? methodName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(methodName));
        }

        /// <summary>
        /// Generate a configuration object from this model.
        /// </summary>
        /// <returns>A configuration object to pass along to the Synthesis.</returns>
        public Configuration GenerateConfiguraiton()
        {
            return new Configuration(PopulationForecastFile,
                ZoneSystemFile,
                InputPopulationDirectory,
                OutputDirectory,
                _randomSeed);
        }
    }

    /// <summary>
    /// This is the data model for the MainWindow
    /// </summary>
    readonly ConfigurationModel _model = new();

    public MainWindow()
    {
        DataContext = _model;
        InitializeComponent();
        // Don't allow re-sizing the height and make sure we have our min width
        Loaded += (_, _) =>
        {
            MinWidth = Width;
            MinHeight = MaxHeight = Height;
        };
    }

    private void InputSeedDirectory_Click(object sender, RoutedEventArgs e)
    {
        if (GetDirectory(out var dir))
        {
            _model.InputPopulationDirectory = dir;
        }
    }

    private void ZoneSystemFile_Click(object sender, RoutedEventArgs e)
    {
        if (GetFile(out var file))
        {
            _model.OutputDirectory = file;
        }
    }

    private void PopulationForecast_Click(object sender, RoutedEventArgs e)
    {
        if (GetFile(out var file))
        {
            _model.PopulationForecastFile = file;
        }
    }

    private void OutputDirectory_Click(object sender, RoutedEventArgs e)
    {
        if(GetDirectory(out var dir))
        {
            _model.OutputDirectory = dir;
        }
    }

    private async void Run_Click(object sender, RoutedEventArgs e)
    {
        var config = _model.GenerateConfiguraiton();
        this.IsEnabled = false;
        try
        {
            await Task.Run(() =>
            {
                Synthesis.RunSynthesis(config);
            });
        }
        finally
        {
            this.IsEnabled = true;
        }
    }

    /// <summary>
    /// Asks the user to select a directory.
    /// </summary>
    /// <param name="directoryName">The name of the directory selected, null if the operation is cancelled.</param>
    /// <returns>True if the operation succeeds, false otherwise.</returns>
    private bool GetDirectory([NotNullWhen(true)] out string? directoryName)
    {
        VistaFolderBrowserDialog openFileDialog = new()
        {
            Multiselect = false
        };
        if (openFileDialog.ShowDialog(this) == true)
        {
            directoryName = openFileDialog.SelectedPath;
            return true;
        }
        else
        {
            directoryName = null;
            return false;
        }
    }

    /// <summary>
    /// Asks the user to select a file.
    /// </summary>
    /// <param name="fileName">The name of the file selected, null if the operation is cancelled.</param>
    /// <returns>True if the operation succeeds, false otherwise.</returns>
    private bool GetFile([NotNullWhen(true)] out string? fileName)
    {
        VistaOpenFileDialog openFileDialog = new()
        {
            Filter = "Comma Separated Values (*.csv)|*.csv|Any File (*.*)|*.*",
            DefaultExt = ".csv",
            CheckPathExists = true,
            DereferenceLinks = true,
            Multiselect = false,
            ShowReadOnly = true,
        };
        if(openFileDialog.ShowDialog(this) == true)
        {
            fileName = openFileDialog.FileName;
            return true;
        }
        else
        {
            fileName = null;
            return false;
        }
    }
}
