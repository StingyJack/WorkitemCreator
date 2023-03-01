namespace WorkitemCreator
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

    /// <summary>
    /// Interaction logic for TemplateSelector.xaml
    /// </summary>
    public partial class TemplateSelector : UserControl
    {
        private List<WorkItemTemplateReference> _workItemTemplates;

        public TemplateSelector()
        {
            InitializeComponent();
        }

        private void AddChildWorkitemTemplate_OnClick(object sender, RoutedEventArgs e)
        {
            var insertionIndex = ChildTemplates.Children.Count - 1;
            var stackPanel = new StackPanel();
            stackPanel.Children.Add(new Label
            {
                Content = insertionIndex,
                Visibility = Visibility.Collapsed,
            });
            stackPanel.Children.Add(new Label
            {
                Content = "Select Child:",
                HorizontalAlignment = HorizontalAlignment.Right
            });
            stackPanel.Children.Add(new ComboBox
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                ItemsSource = _workItemTemplates,
                DisplayMemberPath = nameof(WorkItemTemplateReference.Name),
                SelectedValuePath = nameof(WorkItemTemplateReference.Id)
            });
            var btn = new Button
            {
                Content = "Remove",
                Height = 25,
                Margin = new Thickness(4)
            };
            btn.Click += ChildWorkitemTemplate_Remove;
            stackPanel.Children.Add(btn);

            ChildTemplates.Children.Insert(insertionIndex, stackPanel);
        }

        private void ChildWorkitemTemplate_Remove(object sender, RoutedEventArgs e)
        {
            //this is the "row" that was built by the AddChildWorkitemTemplate_OnClick
            // Button-> Parent StackPanel -> StackPanel's first child element, a label -> its content
            var removeIndex = Convert.ToInt32(((Label)((StackPanel)((Button)e.Source).Parent).Children[0]).Content);

            ChildTemplates.Children.RemoveAt(removeIndex);
        }

        public void UpdateFromProjectData(List<WorkItemTemplateReference> workItemTemplates)
        {
            _workItemTemplates = workItemTemplates ?? throw new ArgumentNullException(nameof(workItemTemplates));

            ParentWorkItemTemplate.ItemsSource = _workItemTemplates;
            foreach (StackPanel childStackPanel in ChildTemplates.Children)
            {
                if (childStackPanel.Children.Count < 3)
                {
                    continue;
                }

                var comboBox = childStackPanel.Children[2] as ComboBox;
                if (comboBox == null)
                {
                    continue;
                }

                comboBox.ItemsSource = _workItemTemplates;
            }
        }
    }
}