
namespace vb6_wakatime
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Reactive.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using System.Windows;
    using Settings;
    using VBIDE;
    using WakaTime;

    [ComVisible(true)]
    [Guid("8AEAE01D-49CD-429A-B71C-1818821A051A")]
    public class WakatimeAddin : VBIDE.IDTExtensibility
    {
        private VBE vbInstance;
        private VBProject currentProject;
        private VBProjectsEvents projectEvents;
        private FileControlEvents fileEvents;
        private VBComponentsEvents componentEvents;
        private IDisposable observableTimer;
        private string currentFileContents;

        private readonly Wakatime wakatime;

        public WakatimeAddin() : this(new Wakatime(new PythonManager(), new WakaTimeCli(new PythonManager())))
        {
        }

        internal WakatimeAddin(Wakatime wakatime)
        {
            this.wakatime = wakatime;
        }

        public void OnConnection(object vbInstance, vbext_ConnectMode ConnectMode, AddIn addinInstance, ref Array custom)
        {
            Debug.WriteLine("OnConnection");

            var instance = vbInstance as VBE;
            if (instance == null)
            {
                throw new ArgumentException("VB instance is not a VBE object");
            }

            this.vbInstance = instance;
            this.HookupEvents();
        }

        public void OnDisconnection(vbext_DisconnectMode RemoveMode, ref Array custom)
        {
            Debug.WriteLine("OnDisconnection");

            // Unhook events
            if (this.fileEvents != null)
            {
                this.fileEvents.RequestWriteFile -= FileSaved;
                this.fileEvents = null;
            }
            if (this.componentEvents != null)
            {
                this.componentEvents.ItemSelected -= FileSelected;
                this.componentEvents = null;
            }
            if (this.projectEvents != null)
            {
                this.projectEvents.ItemActivated -= MonitorProject;
                this.projectEvents.ItemAdded -= MonitorProject;
                this.projectEvents = null;
            }
        }

        public void OnAddInsUpdate(ref Array custom)
        {
        }
        public void OnStartupComplete(ref Array custom)
        {
            this.GetDependenciesAndSettingsAsync().Start();
        }

        private async Task GetDependenciesAndSettingsAsync()
        {
            try
            {
                await this.wakatime.DownloadDependenciesAsync().ConfigureAwait(true);
            }
            catch (WebException we)
            {
                this.ShowError("Could not download dependencies. Please ensure you have an internet connection and your proxy details are entered correctly");
            }
            catch (Exception ex)
            {
                this.ShowError($"Unexpected exception: {ex.Message} ({ex.GetType().Name})");
            }

            this.PromptForMissingSettings();
        }

        private void PromptForMissingSettings()
        {
            if (Properties.Settings.Default.ApiKey == Guid.Empty)
            {
                var settingsWindow = new SettingsView();
                settingsWindow.ShowDialog();
            }
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "WakaTime", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }

        void HookupEvents()
        {
            this.projectEvents = this.vbInstance.Events.VBProjectsEvents;

            this.projectEvents.ItemActivated += MonitorProject;
            this.projectEvents.ItemAdded += MonitorProject;

            // If the addin was not loaded at startup, there may already be a project
            // loaded that we need to monitor
            if (this.vbInstance.ActiveVBProject != null)
            {
                this.MonitorProject(this.vbInstance.ActiveVBProject);
            }
        }

        private void FileSelected(VBComponent component)
        {
            string name = string.IsNullOrEmpty(component.FileNames[1]) ? component.Name : component.FileNames[1];
            this.ReportFileEvent(name);
            this.MonitorFile(component);
        }

        private void ReportFileEvent(VBComponent component)
        {
            var name = string.IsNullOrEmpty(component.FileNames[1]) ? component.Name : component.FileNames[1];
            this.ReportFileEvent(name, false);
        }

        private void ReportFileEvent(string fileName, bool isSave = false)
        {
            if (isSave)
            {
                Debug.WriteLine($"{DateTime.Now.ToString()} - File {fileName} saved");
            }
            else
            {
                Debug.WriteLine($"{DateTime.Now.ToString()} - File {fileName} selected");
            }
        }

        private void MonitorProject(VBProject project)
        {
            // Remove event handlers for previously selected project
            if (this.componentEvents != null)
            {
                this.componentEvents.ItemSelected -= FileSelected;
            }
            if (this.fileEvents != null)
            {
                this.fileEvents.RequestWriteFile -= FileSaved;
            }

            // Hook up project-specific event handlers
            this.currentProject = project;
            this.componentEvents = this.vbInstance.Events.VBComponentsEvents[this.currentProject];
            this.fileEvents = this.vbInstance.Events.FileControlEvents[this.currentProject];

            this.componentEvents.ItemSelected += FileSelected;
            this.fileEvents.RequestWriteFile += FileSaved;
            
            if (this.vbInstance.SelectedVBComponent != null)
            {
                this.FileSelected(this.vbInstance.SelectedVBComponent);
            }
        }

        private void ReportEvent([CallerMemberName]string eventHandler = null)
        {
            Debug.WriteLine($"{DateTime.Now.ToString()} - Event raised: {eventHandler}");
        }

        private void FileSaved(VBProject project, string fileName, out bool cancel)
        {
            cancel = false;
            ReportEvent();
        }

        private void MonitorFile(VBComponent component)
        {
            // Dispose of old subscription
            this.observableTimer?.Dispose();
            currentFileContents = component.CodeModule.Lines[1, component.CodeModule.CountOfLines];

            this.observableTimer = Observable
                                    .Interval(TimeSpan.FromSeconds(5))
                                    .Subscribe(_ =>
                                    {
                                        string newFileContents = component.CodeModule.Lines[1, component.CodeModule.CountOfLines];
                                        if (newFileContents != currentFileContents)
                                        {
                                            this.ReportFileEvent(component);
                                            currentFileContents = newFileContents;
                                        }
                                    });
        }

    }
}
