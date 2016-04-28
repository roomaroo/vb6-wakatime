using System.Windows;

namespace vb6_wakatime.Settings
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    partial class SettingsView : Window
    {
        private readonly SettingsViewModel viewModel;

        internal SettingsView() : this(new SettingsViewModel())
        {
        }
        
        internal SettingsView(SettingsViewModel viewModel)
        {
            this.viewModel = viewModel;
            this.DataContext = viewModel;
            InitializeComponent();
        }

        private void OkButtonClick(object sender, RoutedEventArgs e)
        {
            this.viewModel.Save();
            this.DialogResult = true;
        }
    }
}
