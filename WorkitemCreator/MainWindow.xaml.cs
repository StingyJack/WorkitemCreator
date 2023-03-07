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
    using Microsoft.TeamFoundation.Core.WebApi;
    using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

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
            //foreach (var template in _config.Templates)
            //{
            //    var templateViewControl = new WorkitemTemplateViewControl(template);
            //    var childTab = new TabItem
            //    {
            //        Header = template.Name,
            //        Content = templateViewControl,
            //        HorizontalContentAlignment = HorizontalAlignment.Stretch,
            //        VerticalContentAlignment = VerticalAlignment.Stretch,
            //    };
            //    WorkItemTemplates.Items.Add(childTab);
            //}


            WriteStatus("Configuration loaded, connect to Azure DevOps to continue");
        }

        private void WriteStatus(string message)
        {
            Dispatcher.Invoke(() =>
            {
                var line = $"{DateTime.Now:HH:mm:ss} - {message}";
                Trace.TraceInformation(line);
                LastMessage.Content = line;
                LastMessage.ToolTip = line;
                LogWindow.Text = $"{line}{Environment.NewLine}{LogWindow.Text}";
                File.AppendAllLines(_config.CurrentLogFilePath, new List<string> { line });
            });
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
                TeamProjectList.Items.Add(p);
                if (string.IsNullOrWhiteSpace(_config.LastSelectedTeamProject) == false
                    && string.Equals(p.Name, _config.LastSelectedTeamProject, StringComparison.OrdinalIgnoreCase))
                {
                    TeamProjectList.SelectedItem = p;
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
            var selectedProject = (TeamProjectReference)TeamProjectList.SelectedItem;
            _azDoService.ProjectName = selectedProject.Name;
            _azDoService.ProjectId = selectedProject.Id;

            //WriteStatus($"Getting workitem types for project {_azDoService.ProjectName}...");
            //var wiTypeResult = await _azDoService.GetWorkItemTypesAsync();
            //if (wiTypeResult.IsOk == false)
            //{
            //    ReportError(wiTypeResult.Errors, $"Failed to get WorkItem types for project {_azDoService.ProjectName}.");
            //    return;
            //}

            //WriteStatus($"Got {wiTypeResult.Data.Count} workitem types for project {_azDoService.ProjectName}");

            WriteStatus($"Getting teams for project {_azDoService.ProjectName}...");
            var teamsResult = await _azDoService.GetTeamsAsync();
            if (teamsResult.IsOk == false)
            {
                ReportError(teamsResult.Errors, $"Failed to get Teams for project {_azDoService.ProjectName}.");
                return;
            }

            TeamsList.ItemsSource = teamsResult.Data;
            TeamsList.IsEnabled = true;
            WriteStatus($"Got {teamsResult.Data.Count} teams for project {_azDoService.ProjectName}");


            //WriteStatus($"Getting iterations for project {_azDoService.ProjectName}...");
            //var iterationsResult = await _azDoService.GetIterationsAsync();
            //if (iterationsResult.IsOk == false)
            //{
            //    ReportError(iterationsResult.Errors, $"Failed to get iterations for project {_azDoService.ProjectName}.");
            //    return;
            //}

            //WriteStatus($"Got {iterationsResult.Data.Count} iterations for project {_azDoService.ProjectName}");

            //WriteStatus($"Getting area paths for project {_azDoService.ProjectName}...");
            //var areaPathsResult = await _azDoService.GetAreaPathsAsync();
            //if (areaPathsResult.IsOk == false)
            //{
            //    ReportError(areaPathsResult.Errors, $"Failed to get area paths for project {_azDoService.ProjectName}.");
            //    return;
            //}

            //WriteStatus($"Got {areaPathsResult.Data.Count} area paths for project {_azDoService.ProjectName}");


            //foreach (TabItem ti in WorkItemTemplates.Items)
            //{
            //    var witvc = ti.Content as WorkitemTemplateViewControl;
            //    if (witvc == null)
            //    {
            //        continue;
            //    }

            //    witvc.UpdateWithProjectConfiguration(wiTypeResult.Data, iterationsResult.Data, areaPathsResult.Data);
            //}

            if (TeamsList.SelectedIndex < 0 && TeamsList.Items.Count > 0)
            {
                TeamsList.SelectedIndex = 0;
            }
        }

        private async void TeamsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _azDoService.TeamName = ((WebApiTeam)TeamsList.SelectedValue).Name;

            var teamTemplatesResult = await _azDoService.GetTeamWorkitemTemplateReferencesAsync();
            if (teamTemplatesResult.IsOk == false)
            {
                ReportError(teamTemplatesResult.Errors, "Failed to get team templates");
                return;
            }

            var localWitReferences = BuildLocalWitReferences(teamTemplatesResult.Data);

            foreach (var configuredTemplate in _config.Templates)
            {
                var witr = localWitReferences.FirstOrDefault(w => w.Id.Equals(configuredTemplate.TemplateId));
                if (witr == null)
                {
                    var deadTab = new TabItem
                    {
                        Header = configuredTemplate.TemplateSetName,
                        Content = $"Configured template {configuredTemplate.TemplateSetName} parent ID was not found in the templates " +
                                  "retrieved from the server.",
                        IsEnabled = false
                    };
                    WorkItemTemplates.Items.Add(deadTab);
                    continue;
                }

                var currentLocalRef = witr.Clone();
                foreach (var ctc in configuredTemplate.Children)
                {
                    var ltc = localWitReferences.FirstOrDefault(w => w.Id.Equals(ctc.TemplateId));
                    if (ltc == null)
                    {
                        continue;
                    }

                    currentLocalRef.ChildTemplates.Add(ltc.Clone());
                }

                currentLocalRef.TemplateSetName = configuredTemplate.TemplateSetName;

                var templateSelectorControl = new TemplateSelector();
                var tab = new TabItem
                {
                    Header = currentLocalRef.TemplateSetName,
                    Content = templateSelectorControl,
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    VerticalContentAlignment = VerticalAlignment.Stretch
                };
                templateSelectorControl.UpdateFromSourceData(localWitReferences, currentLocalRef);
                WorkItemTemplates.Items.Add(tab);
            }

            WorkItemTemplates.IsEnabled = true;

            if (WorkItemTemplates.Items.Count > 0)
            {
                WriteStatus("Ready to create!");
                CreateWorkitems.IsEnabled = true;
            }
        }

        private List<LocalWiTemplateReference> BuildLocalWitReferences(List<WorkItemTemplateReference> workItemTemplateReferences)
        {
            var returnValue = new List<LocalWiTemplateReference>();

            foreach (var witr in workItemTemplateReferences)
            {
                var lwitr = new LocalWiTemplateReference(witr);
                returnValue.Add(lwitr);
            }

            return returnValue;
        }

        private async void CreateWorkitems_Click(object sender, RoutedEventArgs e)
        {
            CreateWorkitems.IsEnabled = false;
            WriteStatus("Creating workitems...");
            var witvc = WorkItemTemplates.SelectedContent as TemplateSelector;
            if (witvc == null)
            {
                WriteStatus("No template selected");
                CreateWorkitems.IsEnabled = true;
                return;
            }

            var wit = witvc.AsLocalWiTemplateReference();
            if (wit == null)
            {
                ReportError("Please make sure a parent template is selected.", "Unable to create workitems");
                CreateWorkitems.IsEnabled = true;

                return;
            }

            var wm = new WorkitemMaker(_azDoService);
            var wiCreationResult = await wm.CreateWorkitemsFromTemplateAsync(wit);
            if (wiCreationResult.IsOk == false)
            {
                ReportError(string.Join("\n", wiCreationResult.Errors), "Problems when creating workitems.");
                CreateWorkitems.IsEnabled = true;
                return;
            }

            //MessageBox.Show(JsonConvert.SerializeObject(wiCreationResult, Formatting.Indented), "Workitem Creation Result");
            WriteStatus("Workitems Created!");
            foreach (var wi in wiCreationResult.WorkitemsCreated)
            {
                WriteStatus($"Created workitem: {wi.Uri}");
            }
            CreateWorkitems.IsEnabled = true;
        }
    }
}