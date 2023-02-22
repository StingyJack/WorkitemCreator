namespace WorkitemCreator
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

    /// <summary>
    /// Interaction logic for WorkitemTemplateViewControl.xaml
    /// </summary>
    public partial class WorkitemTemplateViewControl
    {
        #region "fields and ctors"

        private List<WorkItemType> _workItemTypes = new List<WorkItemType>();

        internal WorkitemTemplateViewControl()
        {
            InitializeComponent();
        }

        internal WorkitemTemplateViewControl(WorkitemTemplate workitemTemplate) : this()
        {
            var workitemTemplate1 = workitemTemplate ?? throw new ArgumentNullException(nameof(workitemTemplate));
            TemplateName.Text = workitemTemplate1.Name;
            Title.Text = workitemTemplate1.Title;
            Description.Text = workitemTemplate1.Description;
            WorkitemType.ItemsSource = new List<string> { workitemTemplate.WorkitemType };
            WorkitemType.SelectedItem = workitemTemplate.WorkitemType;

            AdditionalFields.Children.Clear();
            foreach (var af in workitemTemplate.AdditionalFields.OrderBy(k => k.Key))
            {
                var afavm = new AdditionalFieldAndValueViewModel
                {
                    FieldName = af.Key,
                    Value = af.Value,
                    IncludeWhenCreating = false
                };
                var afUserControl = new AdditionalFieldAndValue(afavm);
                AdditionalFields.Children.Add(afUserControl);
            }

            WorkItemChildren.Items.Clear();
            WorkItemChildren.Visibility = Visibility.Collapsed;
            foreach (var child in workitemTemplate.Children ?? new List<WorkitemTemplate>())
            {
                var childControl = new WorkitemTemplateViewControl(child);
                var childTab = new TabItem
                {
                    Header = child.Name,
                    Content = childControl
                };
                WorkItemChildren.Items.Add(childTab);
                WorkItemChildren.Visibility = Visibility.Visible;
            }
        }

        #endregion //#region "fields and ctors"

        #region "event handlers"

        private void WorkitemType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var workItemType = _workItemTypes.FirstOrDefault(w => string.Equals(w.Name, WorkitemType.SelectedItem.ToString(), StringComparison.OrdinalIgnoreCase));

            if (workItemType == null)
            {
                return;
            }

            SetEligibleAdditionalFields(workItemType.Fields.ToList());
        }

        #endregion //#region "event handlers"

        #region "Data and View helpers"

        public WorkitemTemplate AsTemplateDefinition(bool forSavingOnly)
        {
            var returnValue = new WorkitemTemplate
            {
                Name = TemplateName.Text.Trim(),
                Title = Title.Text.Trim(),
                Description = Description.Text.Trim(),
                WorkitemType = WorkitemType.Text.Trim()
            };

            foreach (AdditionalFieldAndValue afav in AdditionalFields.Children)
            {
                var viewModel = afav.ViewModel;
                if (viewModel.IsEligible == false)
                {
                    continue;
                }

                if (forSavingOnly == false && viewModel.IncludeWhenCreating == false)
                {
                    continue; 
                }
                

                returnValue.AdditionalFields.Add(viewModel.FieldName, viewModel.Value);
            }

            foreach (TabItem ti in WorkItemChildren.Items)
            {
                var witvc = (WorkitemTemplateViewControl)ti.Content;

                var child = witvc.AsTemplateDefinition(forSavingOnly);
                returnValue.Children.Add(child);
            }

            return returnValue;
        }

        public void UpdateWorkitemTypeList(List<WorkItemType> workitemTypes)
        {
            _workItemTypes = workitemTypes;

            var workitemTypeNames = workitemTypes.Select(s => s.Name);

            WorkitemType.ItemsSource = workitemTypeNames;
            foreach (TabItem ti in WorkItemChildren.Items)
            {
                var witvc = (WorkitemTemplateViewControl)ti.Content;
                witvc.UpdateWorkitemTypeList(workitemTypes);
            }

            

        }

        public void SetEligibleAdditionalFields(List<WorkItemTypeFieldInstance> workItemFields)
        {
            foreach (AdditionalFieldAndValue afav in AdditionalFields.Children)
            {
                var viewModel = afav.ViewModel;
                var wiField = workItemFields.FirstOrDefault(f => f.Name.Equals(viewModel.FieldName, StringComparison.OrdinalIgnoreCase));
                if (wiField == null)
                {
                    viewModel.IsEligible = false;
                    continue;
                }

                viewModel.AllFields = workItemFields;

                viewModel.IsEligible = true;
            }
        }

        #endregion //#region "Data and View helpers"
    }
}