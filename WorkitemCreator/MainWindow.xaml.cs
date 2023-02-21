namespace WorkitemCreator
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using Microsoft.TeamFoundation.Core.WebApi;
    using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
    using Microsoft.VisualStudio.Services.Client;
    using Microsoft.VisualStudio.Services.Common;
    using Microsoft.VisualStudio.Services.WebApi;
    using Newtonsoft.Json;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public partial class MainWindow
    {
        private readonly Config _config;
        private ConnectionInfo _connectionInfo;

        public MainWindow(Config config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            InitializeComponent();

            LoadTemplates();
        }

        public void LoadTemplates()
        {
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
        }

        private void WriteStatus(string message)
        {
            LastMessage.Content = $"{DateTime.Now:HH:mm:ss} - {message}";
        }

        private void ReportError(string errorMessage, string title)
        {
            MessageBox.Show(errorMessage, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private async void ConnectToAzDo_Click(object sender, RoutedEventArgs e)
        {
            WriteStatus("Connecting...");
            var uri = new Uri(_config.ServiceUrl);

            var creds = new VssClientCredentials(new WindowsCredential(false),
                new VssFederatedCredential(false),
                CredentialPromptType.PromptIfNeeded);

            var connection = new VssConnection(uri, creds);
            try
            {
                await connection.ConnectAsync();
            }
            catch (Exception ex)
            {
                WriteStatus($"Failed to connect! {ex.Message}");
                ReportError($"Error when connecting to Azure DevOps. \n{ex}", "Check your credentials and service url");
                return;
            }

            _connectionInfo = new ConnectionInfo(connection);

            ConnectionState.Content = "Connected";
            WriteStatus("Connected to AzDo server");

            var projectCollectionClient = _connectionInfo.CurrentConnection.GetClient<ProjectCollectionHttpClient>();
            var projectCollections = await projectCollectionClient.GetProjectCollections();
            foreach (var pc in projectCollections.OrderBy(p => p.Name))
            {
                TeamProjectCollectionList.Items.Add(pc.Name);
                if (string.IsNullOrWhiteSpace(_config.LastSelectedTeamProjectCollection) == false
                    && string.Equals(pc.Name, _config.LastSelectedTeamProjectCollection, StringComparison.OrdinalIgnoreCase))
                {
                    TeamProjectCollectionList.SelectedItem = pc.Name;
                }
            }

            TeamProjectCollectionList.IsEnabled = true;
            ConnectToAzDo.IsEnabled = false;
            if (TeamProjectCollectionList.SelectedItem == null && TeamProjectCollectionList.Items.Count > 0)
            {
                TeamProjectCollectionList.SelectedIndex = 0;
            }

            _connectionInfo.ProjectCollectionName = TeamProjectCollectionList.SelectedItem?.ToString();
        }

        private async void TeamProjectCollectionList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _connectionInfo.ProjectCollectionName = TeamProjectCollectionList.SelectedItem?.ToString();

            var projectClient = _connectionInfo.CurrentConnection.GetClient<ProjectHttpClient>();
            var projects = await projectClient.GetProjects();
            foreach (var p in projects)
            {
                TeamProjectList.Items.Add(p.Name);
                if (string.IsNullOrWhiteSpace(_config.LastSelectedTeamProject) == false
                    && string.Equals(p.Name, _config.LastSelectedTeamProject, StringComparison.OrdinalIgnoreCase))
                {
                    TeamProjectList.SelectedItem = p.Name;
                }
            }

            TeamProjectList.IsEnabled = true;

            if (TeamProjectList.SelectedItem == null && TeamProjectList.Items.Count > 0)
            {
                TeamProjectList.SelectedIndex = 0;
            }

            _connectionInfo.ProjectName = TeamProjectList.SelectedItem?.ToString();
        }

        private async void TeamProjectList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _connectionInfo.ProjectName = TeamProjectList.SelectedItem?.ToString();

            var witc = _connectionInfo.CurrentConnection.GetClient<WorkItemTrackingHttpClient>();
            var wiTypes = await witc.GetWorkItemTypesAsync(_connectionInfo.ProjectName);

            foreach (TabItem ti in WorkItemTemplates.Items)
            {
                var witvc = (WorkitemTemplateViewControl)ti.Content;
                witvc.UpdateWorkitemTypeList(wiTypes);
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
            var wm = new WorkitemMaker(_connectionInfo);
            var wiCreationResult = await wm.CreateWorkitemAsync(wit);

            MessageBox.Show(JsonConvert.SerializeObject(wiCreationResult, Formatting.Indented), "Workitem Creation Result");
        }
    }
}