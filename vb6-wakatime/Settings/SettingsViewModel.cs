namespace vb6_wakatime.Settings
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    internal class SettingsViewModel : INotifyPropertyChanged
    {
        internal Guid ApiKey
        {
            get
            {
                return Properties.Settings.Default.ApiKey;
            }

            set
            {
                if (Properties.Settings.Default.ApiKey != value)
                {
                    Properties.Settings.Default.ApiKey = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        public void Save()
        {
            Properties.Settings.Default.Save();
        }

        public string Proxy
        {
            get
            {
                return Properties.Settings.Default.Proxy;
            }

            set
            {
                if (Properties.Settings.Default.Proxy != value)
                {
                    Properties.Settings.Default.Proxy = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName]string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
