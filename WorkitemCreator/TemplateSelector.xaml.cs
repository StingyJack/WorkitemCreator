namespace WorkitemCreator
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

    /// <summary>
    ///     Interaction logic for TemplateSelector.xaml
    /// </summary>
    public partial class TemplateSelector
    {
        private List<LocalWiTemplateReference> _allWorkItemTemplates;

        public TemplateSelector()
        {
            InitializeComponent();
        }

        private void AddChildWorkitemTemplate_OnClick(object sender, RoutedEventArgs e)
        {
            AddChildWorkitemTemplateUiControls();
        }

        public void UpdateFromSourceData(List<LocalWiTemplateReference> allWorkitemTemplates, LocalWiTemplateReference localWiTemplateReference)
        {
            _allWorkItemTemplates = allWorkitemTemplates ?? throw new ArgumentNullException(nameof(allWorkitemTemplates));

            ParentWorkItemTemplate.ItemsSource = _allWorkItemTemplates;
            if (allWorkitemTemplates.Any(w => w.Id.Equals(localWiTemplateReference.Id)))
            {
                ParentWorkItemTemplate.SelectedValue = localWiTemplateReference.Id;
            }

            foreach (var c in localWiTemplateReference.ChildTemplates)
            {
                AddChildWorkitemTemplateUiControls(c);
            }
        }

        private void AddChildWorkitemTemplateUiControls(WorkItemTemplateReference existingConfiguredChildTemplate = null)
        {
            var insertionIndex = ChildTemplates.RowDefinitions.Count;
            ChildTemplates.RowDefinitions.Add(new RowDefinition());

            var caption = new Label
            {
                Content = "Select Child:",
                HorizontalAlignment = HorizontalAlignment.Right
            };
            ChildTemplates.Children.Add(caption);
            Grid.SetRow(caption, insertionIndex);
            Grid.SetColumn(caption, 0);

            var comboBox = new ComboBox
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                ItemsSource = _allWorkItemTemplates,
                DisplayMemberPath = nameof(LocalWiTemplateReference.Title),
                SelectedValuePath = nameof(LocalWiTemplateReference.Id)
            };

            if (existingConfiguredChildTemplate != null)
            {
                comboBox.SelectedValue = existingConfiguredChildTemplate.Id;
            }

            ChildTemplates.Children.Add(comboBox);
            Grid.SetRow(comboBox, insertionIndex);
            Grid.SetColumn(comboBox, 1);

            var btn = new Button
            {
                Content = "Remove",
                Height = 25,
                Margin = new Thickness(4),
                Tag = insertionIndex
            };
            btn.Click += ChildWorkitemTemplate_Remove;
            ChildTemplates.Children.Add(btn);
            Grid.SetRow(btn, insertionIndex);
            Grid.SetColumn(btn, 2);
        }

        private void ChildWorkitemTemplate_Remove(object sender, RoutedEventArgs e)
        {
            //this is the "row" that was built by the AddChildWorkitemTemplateUiControls

            var removeIndex = Convert.ToInt32(((Button)e.Source).Tag);

            ChildTemplates.RowDefinitions.RemoveAt(removeIndex);
        }


        public LocalWiTemplateReference AsLocalWiTemplateReference()
        {
            var returnValue = ParentWorkItemTemplate.SelectedItem as LocalWiTemplateReference;
            if (returnValue == null)
            {
                return new LocalWiTemplateReference();
            }

            foreach (var childItem in ChildTemplates.Children)
            {
                var comboBox = childItem as ComboBox;
                if (comboBox == null)
                {
                    continue;
                }

                if (comboBox.SelectedIndex < 0)
                {
                    continue;
                }

                var cwitr = comboBox.SelectedItem as LocalWiTemplateReference;
                if (cwitr == null)
                {
                    continue;
                }

                returnValue.ChildTemplates.Add(cwitr);
            }

            return returnValue;
        }
    }
}