using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WakaTime;

namespace vb6_wakatime.Settings
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly WakaTimeConfigFile configFile;

        public SettingsViewModel(WakaTimeConfigFile configFile)
        {
            this.configFile = configFile;
            this.configFile.Read();
        }

        public string ApiKey
        {
            get
            {
                return this.configFile.ApiKey;
            }

            set
            {
                Guid apiGuid;
                if (Guid.TryParse(value, out apiGuid))
                {
                    this.configFile.ApiKey = value;
                    this.NotifyPropertyChanged();
                }
                else
                {
                    throw new FormatException("The API key is in the wrong format");
                }
            }
        }

        public void Save()
        {
            this.configFile.Save();
        }

        public string Proxy
        {
            get
            {
                return this.configFile.ApiKey;
            }

            set
            {
                this.configFile.Proxy = value;
                this.NotifyPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName]string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
