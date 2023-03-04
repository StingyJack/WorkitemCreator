namespace WorkitemCreator
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

    /// <summary>
    /// Interaction logic for TemplateSelector.xaml
    /// </summary>
    public partial class TemplateSelector
    {
        private List<WorkItemTemplateReference> _workItemTemplates;

        public TemplateSelector()
        {
            InitializeComponent();
        }

        private void AddChildWorkitemTemplate_OnClick(object sender, RoutedEventArgs e)
        {
            AddChildWorkitemTemplateUiControls();
        }

        public void UpdateFromSourceData(List<WorkItemTemplateReference> workItemTemplates, LocalWiTemplateReference localWiTemplateReference)
        {
            _workItemTemplates = workItemTemplates ?? throw new ArgumentNullException(nameof(workItemTemplates));

            ParentWorkItemTemplate.ItemsSource = _workItemTemplates;
            if (workItemTemplates.Any(w => w.Id.Equals(localWiTemplateReference.Id)))
            {
                ParentWorkItemTemplate.SelectedValue = localWiTemplateReference.Id;
            }

            foreach (var c in localWiTemplateReference.ChildTemplates)
            {
                AddChildWorkitemTemplateUiControls(c);
            }
        }

        private void AddChildWorkitemTemplateUiControls(LocalWiTemplateReference existingConfiguredChildTemplate = null)
        {
            var insertionIndex = ChildTemplates.Children.Count - 1;
            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };
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
            var comboBox = new ComboBox
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                ItemsSource = _workItemTemplates,
                DisplayMemberPath = nameof(LocalWiTemplateReference.Title),
                SelectedValuePath = nameof(LocalWiTemplateReference.Id)
            };

            if (existingConfiguredChildTemplate != null)
            {
                comboBox.SelectedValue = existingConfiguredChildTemplate.Id;
            }

            stackPanel.Children.Add(comboBox);
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
            //this is the "row" that was built by the AddChildWorkitemTemplateUiControls
            // Button-> Parent StackPanel -> StackPanel's first child element, a label -> its content
            var removeIndex = Convert.ToInt32(((Label)((StackPanel)((Button)e.Source).Parent).Children[0]).Content);

            ChildTemplates.Children.RemoveAt(removeIndex);
        }

       
    }
}