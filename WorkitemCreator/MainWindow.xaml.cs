namespace WorkitemCreator
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Threading;
    using Newtonsoft.Json;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public partial class MainWindow
    {
        private readonly Config _config;
        private readonly AzDoService _azDoService;

        public MainWindow(Config config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _azDoService = new AzDoService();
            InitializeComponent();
            LoadTemplates();
        }

        public void LoadTemplates()
        {
            WriteStatus("Loading templates from config");
            WorkItemTemplates.Items.Clear();
            ServiceUrl.Text = _config.ServiceUrl;
            foreach (var template in _config.Templates)
            {
                var templateViewControl = new WorkitemTemplateViewControl(template);
                var childTab = new TabItem
                {
                    Header = template.Name,
                    Content = templateViewControl,
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    VerticalContentAlignment = VerticalAlignment.Stretch,
                };
                WorkItemTemplates.Items.Add(childTab);
            }

            WriteStatus("Templates loaded from config");
        }

        private void WriteStatus(string message)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                var line = $"{DateTime.Now:HH:mm:ss} - {message}";
                Trace.TraceInformation(line);
                LastMessage.Content = line;
                LastMessage.ToolTip = line;
                File.AppendAllLines(_config.CurrentLogFilePath, new List<string> { line });
            }));
        }

        private void ReportError(string errorMessage, string title)
        {
            var line = $"{DateTime.Now:HH:mm:ss} - {title} {errorMessage}";
            File.AppendAllLines(_config.CurrentLogFilePath, new List<string> { line });

            MessageBox.Show(errorMessage, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private async void ConnectToAzDo_Click(object sender, RoutedEventArgs e)
        {
            var serviceUrl = ServiceUrl.Text.Trim();
            WriteStatus($"Connecting to {serviceUrl}...");
            
            ConnectionState.Content = "Connecting";

            var connResult = await _azDoService.ConnectAsync(serviceUrl, _config.AzDoPat);
            if (connResult.IsConnected == false)
            {
                WriteStatus($"Unable to connect. {connResult.ConnectionError}");
                return;
            }

            ConnectionState.Content = "Connected";
            WriteStatus("Connected to AzDo server");

            WriteStatus("Getting available projects");
            var projectResult = await _azDoService.GetProjectsAsync();
            WriteStatus($"Got {projectResult.Data.Count} projects");
            foreach (var p in projectResult.Data.OrderBy(p => p.Name))
            {
                TeamProjectList.Items.Add(p.Name);
                if (string.IsNullOrWhiteSpace(_config.LastSelectedTeamProject) == false
                    && string.Equals(p.Name, _config.LastSelectedTeamProject, StringComparison.OrdinalIgnoreCase))
                {
                    TeamProjectList.SelectedItem = p.Name;
                }
            }

            TeamProjectList.IsEnabled = true;
            ConnectToAzDo.IsEnabled = false;

            if (TeamProjectList.Items.Count > 0 && TeamProjectList.SelectedIndex < 0)
            {
                TeamProjectList.SelectedIndex = 0;
            }
        }

        private async void TeamProjectList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _azDoService.ProjectName = TeamProjectList.SelectedItem?.ToString();

            var wiTypeResult = await _azDoService.GetWorkItemTypesAsync();

            foreach (TabItem ti in WorkItemTemplates.Items)
            {
                var witvc = (WorkitemTemplateViewControl)ti.Content;
                witvc.UpdateWorkitemTypeList(wiTypeResult.Data);
            }

            if (TeamProjectList.SelectedIndex >= 0)
            {
                CreateWorkitems.IsEnabled = true;
                WorkItemTemplates.IsEnabled = true;
            }
        }

        private async void CreateWorkitems_Click(object sender, RoutedEventArgs e)
        {
            var witvc = WorkItemTemplates.SelectedContent as WorkitemTemplateViewControl;
            if (witvc == null)
            {
                WriteStatus("No template selected");
                return;
            }

            var wit = witvc.AsTemplateDefinition(false);
            var wm = new WorkitemMaker(_azDoService);
            var wiCreationResult = await wm.CreateWorkitemAsync(wit);

            MessageBox.Show(JsonConvert.SerializeObject(wiCreationResult, Formatting.Indented), "Workitem Creation Result");
        }
    }
}