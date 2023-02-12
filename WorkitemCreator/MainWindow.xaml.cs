namespace WorkitemCreator
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;
    using Newtonsoft.Json;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Config _config;

        public MainWindow()
        {
            InitializeComponent();
            LoadTemplates();
        }

        public void LoadTemplates()
        {
            //var c = new Config();
            //c.ServiceUrl = "http://localhost";
            //c.Templates = new List<WorkitemTemplate>();
            //c.Templates.Add(new WorkitemTemplate
            //{
            //    Name = "Standard User Story",
            //    Title = "Add a feature",
            //    WorkitemType = WorkitemType.UserStory,
            //    Description = "This is a standard user story with development and testing tasks",
            //    Children = new List<WorkitemTemplate>
            //    {
            //        new WorkitemTemplate
            //        {
            //            Name = "Dev Task",
            //            WorkitemType = WorkitemType.Task,
            //            Title = "Development",
            //            Description = "Do the programming",
            //            Properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            //            {
            //                {"Activity","Development"},
            //                {"Remaining", "8"}
            //            }
            //        }
            //    }
            //});

            //var jsonned = JsonConvert.SerializeObject(c, Formatting.Indented);
            //File.WriteAllText("config2.json", jsonned);

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
    }
}