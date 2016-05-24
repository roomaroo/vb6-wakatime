
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
    using log4net;
    using Settings;
    using VBIDE;

    [ComVisible(true)]
    [Guid("8AEAE01D-49CD-429A-B71C-1818821A051A")]
    public class WakatimeAddin : VBIDE.IDTExtensibility
    {
        private VBE vbInstance;
        private VBProject currentProject;
        private VBProjectsEvents projectEvents;
        private FileControlEvents fileEvents;
        private VBComponentsEvents componentEvents;
        private CommandBarEvents commandBarEvents;
        private IDisposable observableTimer;
        private string currentFileContents;

        private readonly Wakatime wakatime;
        private readonly ILog log = LogManager.GetLogger(nameof(WakatimeAddin));
        private readonly Reporter reporter;

        public WakatimeAddin() : this(new Wakatime(new PythonManager(), new WakaTimeCli(new PythonManager())), new Reporter(new PythonManager()))
        {
        }

        internal WakatimeAddin(Wakatime wakatime, Reporter reporter)
        {
            this.wakatime = wakatime;
            this.reporter = reporter;
        }

        public void OnConnection(object vbInstance, vbext_ConnectMode ConnectMode, AddIn addinInstance, ref Array custom)
        {
            log.Debug("Addin: OnConnection");

            var instance = vbInstance as VBE;
            if (instance == null)
            {
                throw new ArgumentException("VB instance is not a VBE object");
            }

            this.vbInstance = instance;
            this.AddMenuItem();
            this.HookupEvents();

           
        }

        public void OnDisconnection(vbext_DisconnectMode RemoveMode, ref Array custom)
        {
            log.Debug("Addin: OnDisconnection");

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
            this.GetDependenciesAsync().Wait();
            this.PromptForMissingSettings();
        }

        private async Task GetDependenciesAsync()
        {
            try
            {
                await this.wakatime.DownloadDependenciesAsync().ConfigureAwait(true);
            }
            catch (WebException we)
            {
                log.Error("Error downloading dependencies", we);
                this.ShowError("Could not download dependencies. Please ensure you have an internet connection and your proxy details are entered correctly");
            }
            catch (Exception ex)
            {
                log.Error("Unexpected exception", ex);
                this.ShowError($"Unexpected exception: {ex.Message} ({ex.GetType().Name})");
            }
        }

        private void PromptForMissingSettings()
        {
            Guid temp;
            string apiKey = Properties.Settings.Default.ApiKey;

            if (string.IsNullOrEmpty(apiKey) || !Guid.TryParse(apiKey, out temp))
            {
                this.ShowSettings();
            }
        }

        private void ShowSettings()
        {
            try
            {
                var settingsWindow = new SettingsView();
                settingsWindow.ShowDialog();
            }
            catch (Exception e)
            {
                log.Error("Error showing settings", e);
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
            this.ReportFileEvent(component);
            this.MonitorFile(component);
        }

        private async Task ReportFileEvent(VBComponent component, bool isSave = false)
        {
            string projectName = component.VBE.ActiveVBProject.Name;
            var fileName = string.IsNullOrEmpty(component.FileNames[1]) ? component.Name : component.FileNames[1];
            await this.ReportFileEvent(projectName, fileName, isSave);
        }

        private async Task ReportFileEvent(string project, string fileName, bool isSave = false)
        {
            await this.reporter.SendHeartbeat(project, $"\"{fileName}\"", isSave);
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

        private void FileSaved(VBProject project, string fileName, out bool cancel)
        {
            cancel = false;
            this.ReportFileEvent(project.Name, fileName, true);
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

        private void AddMenuItem()
        {
            var addinMenu = this.vbInstance.CommandBars["Add-ins"];
            if (addinMenu == null)
            {
                log.Error("Failed to get Add-ins menu");
            }

            var menuItem = addinMenu.Controls.Add(1);
            menuItem.Caption = "Wakatime";

            this.commandBarEvents = this.vbInstance.Events.CommandBarEvents[menuItem];
            commandBarEvents.Click += MenuClicked;
        }

        private void MenuClicked(object CommandBarControl, ref bool handled, ref bool CancelDefault)
        {
            handled = true;
            CancelDefault = true;
            this.ShowSettings();
        }
    }
}
 
