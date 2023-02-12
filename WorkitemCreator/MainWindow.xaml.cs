namespace WorkitemCreator
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.ServiceModel.Configuration;
    using System.Windows;
    using System.Windows.Controls;
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
        private Config _config;
        private VssConnection _vssConnection;

        public MainWindow()
        {
            InitializeComponent();
            LoadTemplates();
        }

        public void LoadTemplates()
        {
            if (File.Exists("config.json") == false)
            {
                return;
            }

            WorkItemTemplates.Items.Clear();
            var configContents = File.ReadAllText("config.json");
            _config = JsonConvert.DeserializeObject<Config>(configContents);
            ServiceUrl.Text = _config.ServiceUrl;
            foreach (var template in _config.Templates)
            {
                var templateViewControl = new WorkitemTemplateViewControl(template);
                var childTab = new TabItem
                {
                    Header = template.Name,
                    Content = templateViewControl,
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    VerticalContentAlignment = VerticalAlignment.Top,
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

            _vssConnection = new VssConnection(uri, creds);
            try
            {
                await _vssConnection.ConnectAsync();
            }
            catch (Exception ex)
            {
                WriteStatus($"Failed to connect! {ex.Message}");
                ReportError($"Error when connecting to Azure DevOps. \n{ex}", "Check your credentials and service url");
                return;
            }

            ConnectionState.Content = "Connected";
            WriteStatus($"Connected to AzDo server");
            CreateWorkitems.IsEnabled = true;

        }

        private void CreateWorkitems_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}