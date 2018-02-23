﻿using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Xceed.Wpf.Toolkit;

namespace EasyGarlic {
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class SettingsWindow : Window, INotifyPropertyChanged {

        private static Logger logger = LogManager.GetLogger("SettingsLogger");

        private MainWindow parentWindow;

        #region WPF Properties

        private string versionText;
        public string VersionText
        {
            get
            {
                return versionText;
            }
            set
            {
                versionText = value;

                OnPropertyChanged(nameof(VersionText));
            }
        }

        private bool showCPUOptions;
        public bool ShowCPUOptions
        {
            get
            {
                return showCPUOptions;
            }
            set
            {
                showCPUOptions = value;

                OnPropertyChanged(nameof(ShowCPUOptions));
            }
        }

        private bool showMiningTab;
        public bool ShowMiningTab
        {
            get
            {
                return showMiningTab;
            }
            set
            {
                showMiningTab = value;

                OnPropertyChanged(nameof(ShowMiningTab));
            }
        }

        private KeyValuePair<int, Miner> selectedMiner;
        private List<Miner> minerList;
        public List<Miner> MinerList
        {
            get
            {
                return minerList;
            }
            set
            {
                minerList = value;

                OnPropertyChanged(nameof(MinerList));
            }
        }

        private int intensityInput;
        public int IntensityInput
        {
            get
            {
                return intensityInput;
            }
            set
            {
                intensityInput = value;

                OnPropertyChanged(nameof(IntensityInput));
            }
        }

        private string customParameters;
        public string CustomParameters
        {
            get
            {
                return customParameters;
            }
            set
            {
                customParameters = value;

                OnPropertyChanged(nameof(CustomParameters));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
        

        public SettingsWindow(MainWindow mainWindow)
        {
            parentWindow = mainWindow;

            InitializeComponent();
            DataContext = this;
            Loaded += SettingsWindow_Loaded;
            Closing += SettingsWindow_Closing;
        }

        private async void SettingsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Load different Miner items in ComboBox
            MinerList = new List<Miner>(parentWindow.linker.minerManager.data.installed.Values);
            if (MinerList.Count > 0)
            {
                // Show the Mining tabs
                ShowMiningTab = true;

                // Load default miner data
                selectedMiner = new KeyValuePair<int,Miner>(0, MinerList[0]);
                IntensityInput = selectedMiner.Value.customIntensity;
                CustomParameters = selectedMiner.Value.customParameters;
            }

            // TODO: Make system so that cpuminer is replaced by cpuminer-opt when checking the box

            // Load Version Number
            VersionText = "v" + Config.VERSION;

            // Hide CPU Options
            ShowCPUOptions = false;
        }

        private async void SettingsWindow_Closing(object sender, CancelEventArgs e)
        {
            // Apply Miner changes
            Dictionary<string, Miner> installed = parentWindow.linker.minerManager.data.installed;
            for (int i = 0; i < MinerList.Count; i++)
            {
                installed[MinerList[i].GetID()] = MinerList[i];
            }
            
            // Save Data
            await parentWindow.linker.minerManager.data.SaveAsync();

            // TODO: Perhaps change it so you don't have to reload EVERYTHING
            // Reload data on main window
            parentWindow.ReloadData();

        }

        #region Data Changed
        private void MinerListCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedMiner = new KeyValuePair<int, Miner>((sender as ComboBox).SelectedIndex, MinerList[(sender as ComboBox).SelectedIndex]);

            // Use CPU view
            if (selectedMiner.Value.type == "cpu")
            {
                ShowCPUOptions = true;
            }
            else
            {
                ShowCPUOptions = false;
            }
        }

        private void IntensityInput_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (selectedMiner.Value != null)
            {
                selectedMiner.Value.customIntensity = IntensityInput;
            }
        }
        
        private void CustomParametersInput_ValueChanged(object sender, TextChangedEventArgs e)
        {
            if (selectedMiner.Value != null)
            {
                selectedMiner.Value.customParameters = CustomParameters;
            }
        }

        #endregion
    }
}
