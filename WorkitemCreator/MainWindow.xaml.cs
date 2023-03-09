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
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using Microsoft.TeamFoundation.Core.WebApi;
    using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
    using Newtonsoft.Json;
    using Process = System.Diagnostics.Process;

    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public partial class MainWindow
    {
        #region "fields and loading"

        private Config _config;
        private readonly AzDoService _azDoService;
        private List<LocalWiTemplateReference> _localWitReferences = new List<LocalWiTemplateReference>();

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

        #endregion //#region "fields and loading"

        #region "logging"

        private void WriteStatus(string rawMessage, params string[] hyperlinkTextLabels)
        {
            Dispatcher.Invoke(() =>
            {
                var line = $"{DateTime.Now:HH:mm:ss} - {rawMessage}";
                Trace.TraceInformation(line);
                LastMessage.Content = line;
                LastMessage.ToolTip = line;
                if (LogWindow.Inlines.Count <= 0)
                {
                    LogWindow.Inlines.Add(new Run(string.Empty));
                }

                LogWindow.Inlines.InsertBefore(LogWindow.Inlines.FirstInline, new LineBreak());
                var linkLabels = new List<string>(hyperlinkTextLabels);
                var parsedLine = line.Split(' ');
                for (var i = parsedLine.Length - 1; i >= 0; i--)
                {
                    var segment = parsedLine[i];

                    var segmentOnlyRun = new Run($" {segment}");
                    if (segment.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                        || segment.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    {
                        Uri uri;
                        if (Uri.TryCreate(segment, UriKind.Absolute, out uri) == false)
                        {
                            LogWindow.Inlines.InsertBefore(LogWindow.Inlines.FirstInline, segmentOnlyRun);
                            continue;
                        }

                        var hl = new Hyperlink { NavigateUri = uri };
                        var hlTextLabel = uri.ToString();
                        if (linkLabels.Count > 0)
                        {
                            hlTextLabel = linkLabels[linkLabels.Count - 1];
                            linkLabels.RemoveAt(linkLabels.Count - 1);
                        }

                        hl.Inlines.Add($"{hlTextLabel}");
                        hl.RequestNavigate += (sender, args) =>
                        {
                            Process.Start(new ProcessStartInfo(args.Uri.AbsoluteUri));
                            args.Handled = true;
                        };
                        LogWindow.Inlines.InsertBefore(LogWindow.Inlines.FirstInline, new Run(" "));
                        LogWindow.Inlines.InsertBefore(LogWindow.Inlines.FirstInline, hl);
                        LogWindow.Inlines.InsertBefore(LogWindow.Inlines.FirstInline, new Run(" "));
                    }
                    else
                    {
                        if (LogWindow.Inlines.Count <= 0)
                        {
                            LogWindow.Inlines.Add(new Run(string.Empty));
                        }

                        LogWindow.Inlines.InsertBefore(LogWindow.Inlines.FirstInline, segmentOnlyRun);
                    }
                }

                //LogWindow.Text = $"{line}{Environment.NewLine}{LogWindow.Text}";
                File.AppendAllLines(_config.CurrentLogFilePath, new List<string> { line });
            });
        }

        private void ReportError(string errorMessage, string title)
        {
            var line = $"{DateTime.Now:HH:mm:ss} - {title} {errorMessage}";
            File.AppendAllLines(_config.CurrentLogFilePath, new List<string> { line });

            MessageBox.Show(errorMessage, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        #endregion //#region "logging"

        #region "event handlers"

        private async void ConnectToAzDo_Click(object sender, RoutedEventArgs e)
        {
            var serviceUrl = ServiceUrl.Text.Trim();
            WriteStatus($"Connecting to {serviceUrl} ...");

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

            TeamsList.ItemsSource = teamsResult.Data.OrderBy(t => t.Name);
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
            if (TeamsList.SelectedIndex < 0)
            {
                SaveConfig.IsEnabled = false;
                AddTemplateSet.IsEnabled = false;
                return;
            }

            _azDoService.TeamName = ((WebApiTeam)TeamsList.SelectedValue).Name;

            var teamTemplatesResult = await _azDoService.GetTeamWorkitemTemplateReferencesAsync();
            if (teamTemplatesResult.IsOk == false)
            {
                ReportError(teamTemplatesResult.Errors, "Failed to get team templates");
                return;
            }

            _localWitReferences = BuildLocalWitReferences(teamTemplatesResult.Data);

            foreach (var configuredTemplate in _config.Templates)
            {
                var witr = _localWitReferences.FirstOrDefault(w => w.Id.Equals(configuredTemplate.TemplateId));
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
                    var ltc = _localWitReferences.FirstOrDefault(w => w.Id.Equals(ctc.TemplateId));
                    if (ltc == null)
                    {
                        continue;
                    }

                    currentLocalRef.ChildTemplates.Add(ltc.Clone());
                }

                currentLocalRef.TemplateSetName = configuredTemplate.TemplateSetName;

                var tab = CreateTemplateSelectorTabItem(currentLocalRef, out var templateSelectorControl);
                templateSelectorControl.UpdateFromSourceData(_localWitReferences, currentLocalRef);
                WorkItemTemplates.Items.Add(tab);
            }

            WorkItemTemplates.IsEnabled = true;

            if (WorkItemTemplates.Items.Count > 0)
            {
                WriteStatus("Ready to create!");
                CreateWorkitems.IsEnabled = true;
            }

            SaveConfig.IsEnabled = true;
            AddTemplateSet.IsEnabled = true;
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
                WriteStatus($"Created workitem: {wi.Uri}", $"{wi.Id} - {wi.WorkitemTypeName} - {wi.Title}");
            }

            CreateWorkitems.IsEnabled = true;
        }

        private void AddTemplateSet_OnClick(object sender, RoutedEventArgs e)
        {
            SaveConfig.Visibility = Visibility.Collapsed;
            NewTemplateSetName.Visibility = Visibility.Visible;
            NewTemplateSetName.Focus();
            WriteStatus("Enter a name for the template set and press Enter to create it.");
        }

        private void NewTemplateSetName_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                NewTemplateSetName.Visibility = Visibility.Collapsed;
                SaveConfig.Visibility = Visibility.Visible;
                WriteStatus("New template set creation aborted.");
                return;
            }

            if (e.Key != Key.Enter)
            {
                return;
            }

            var newTemplateSetName = NewTemplateSetName.Text.Trim();
            if (string.IsNullOrWhiteSpace(newTemplateSetName))
            {
                WriteStatus("A value needs to be provided. Press ESC to abort.");
                return;
            }

            //TODO: check to make sure there arent multiple sets with the same name

            NewTemplateSetName.Visibility = Visibility.Collapsed;
            SaveConfig.Visibility = Visibility.Visible;

            var emptyReference = new LocalWiTemplateReference { TemplateSetName = newTemplateSetName };
            TemplateSelector templateSelector;

            var tabItem = CreateTemplateSelectorTabItem(emptyReference, out templateSelector);
            templateSelector.UpdateFromSourceData(_localWitReferences, emptyReference);

            WorkItemTemplates.Items.Add(tabItem);
            WorkItemTemplates.SelectedIndex = WorkItemTemplates.Items.Count - 1;
            NewTemplateSetName.Text = string.Empty;
            WriteStatus("Ready to create!");
            CreateWorkitems.IsEnabled = true;
        }

        private void SaveConfig_OnClick(object sender, RoutedEventArgs e)
        {
            var updatedConfig = new Config
            {
                ServiceUrl = ServiceUrl.Text.Trim(),
                LastSelectedTeamProject = TeamProjectList.Text.Trim(),
                Templates = new List<ConfiguredWitReference>(),
                CurrentLogFilePath = _config.CurrentLogFilePath,
                AzDoPat = _config.AzDoPat
            };
            var currentIndex = 0;
            foreach (var rawTabItem in WorkItemTemplates.Items)
            {
                currentIndex++;
                var tabItem = (TabItem)rawTabItem;
                if (tabItem.HasHeader == false)
                {
                    WriteStatus($"Template Set at position {currentIndex} is missing a header and will be skipped.");

                    continue;
                }

                var templateSetName = tabItem.Header.ToString();
                if (string.IsNullOrEmpty(templateSetName))
                {
                    ReportError($"Template Set at position {currentIndex} is missing a name and will be skipped.", "Cant save one of the template sets.");
                    continue;
                }

                var templateSelectorControl = tabItem.Content as TemplateSelector;
                if (templateSelectorControl == null)
                {
                    WriteStatus($"Template Set {templateSetName} is not a {nameof(TemplateSelector)} control and cant be read");
                    continue;
                }

                var localWiTemplateReference = templateSelectorControl.AsLocalWiTemplateReference();
                var configuredWitReference = new ConfiguredWitReference
                {
                    TemplateSetName = templateSetName,
                    TemplateId = localWiTemplateReference.Id
                };
                foreach (var lwitKid in localWiTemplateReference.ChildTemplates)
                {
                    var configKid = new ConfiguredWitReference { TemplateId = lwitKid.Id };
                    configuredWitReference.Children.Add(configKid);
                }

                updatedConfig.Templates.Add(configuredWitReference);
            }

            var jsonned = JsonConvert.SerializeObject(updatedConfig, Formatting.Indented);
            File.WriteAllText(".\\config.json", jsonned);
            _config = updatedConfig;
            WriteStatus("Configuration written to disk");
        }

        #endregion //#region "event handlers"

        #region "helpers"

        private static TabItem CreateTemplateSelectorTabItem(LocalWiTemplateReference currentLocalRef, out TemplateSelector templateSelectorControl)
        {
            templateSelectorControl = new TemplateSelector();
            var dockPanel = new DockPanel();
            var headerLabel = new Label { Content = currentLocalRef.TemplateSetName };
            dockPanel.Children.Add(headerLabel);

            DockPanel.SetDock(headerLabel, Dock.Left);

            var closeButton = new Button
            {
                Content = "X",
                Height = 20,
                Margin = new Thickness(1),
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Top,
                Background = new SolidColorBrush(Color.FromRgb(244, 140, 150))
            };
            closeButton.Click += (o, args) =>
            {
                var tabItem = (TabItem)((DockPanel)((Button)args.Source).Parent).Parent;

                ((TabControl)tabItem.Parent).Items.Remove(tabItem);

                args.Handled = true;
            };
            dockPanel.Children.Add(closeButton);
            DockPanel.SetDock(closeButton, Dock.Right);
            dockPanel.LastChildFill = true;
            var tab = new TabItem
            {
                Header = dockPanel,
                Content = templateSelectorControl,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Stretch
            };
            return tab;
        }

        private static List<LocalWiTemplateReference> BuildLocalWitReferences(List<WorkItemTemplateReference> workItemTemplateReferences)
        {
            var returnValue = new List<LocalWiTemplateReference>();

            foreach (var witr in workItemTemplateReferences)
            {
                var lwitr = new LocalWiTemplateReference(witr);
                returnValue.Add(lwitr);
            }

            return returnValue;
        }

        #endregion #region "helpers"
    }
}